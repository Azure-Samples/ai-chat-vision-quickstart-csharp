using Microsoft.Extensions.AI;
using AIChatImgApp.Model;
namespace AIChatImgApp.Services;

/// <summary>
/// Chat service for interacting with the chat client.
/// </summary>
/// <param name="client">The <see cref="IChatClient"/> implementatoin.</param>
public class ChatService(IChatClient client)
{
    /// <summary>
    /// The chat cient.
    /// </summary>
    private readonly IChatClient _client = client;
    
    /// <summary>
    /// Single/non-streaming chat.
    /// </summary>
    /// <param name="request">Outgoing request with history.</param>
    /// <returns>Incoming response from LLM.</returns>
    internal async Task<Message> Chat(ChatRequest request)
    {
        var history = CreateHistoryFromRequest(request);
        var response = await _client.CompleteAsync(history);

        return new Message(response.Message.Role != ChatRole.User, response.Message.Text ?? string.Empty);
    }

    /// <summary>
    /// Streaming chat.
    /// </summary>
    /// <param name="request">Initial request with history.</param>
    /// <returns>Stream of agent responses.</returns>
    internal async IAsyncEnumerable<string> Stream(ChatRequest request)
    {
        var history = CreateHistoryFromRequest(request);
        IAsyncEnumerable<StreamingChatCompletionUpdate> response = _client.CompleteStreamingAsync(history);

        await foreach (StreamingChatCompletionUpdate content in response)
        {
            yield return content.Text ?? string.Empty;
        }
    }

    /// <summary>
    /// Map from our UI entity to the Extensions.AI version.
    /// </summary>
    /// <param name="message">The UI message to convert.</param>
    /// <returns>The equivalent <see cref="ChatMessage"/>.</returns>
    private static ChatMessage ConvertMessage(Message message)
    {
        var textMessage = new ChatMessage(
            message.IsAssistant ? ChatRole.Assistant : ChatRole.User,
            message.TextContent);

        if (message.Image == null)
        {
            return textMessage;
        }

        var image = new ImageContent(
            message.Image.ImageBytes,
            message.Image.MimeType);       

        var imageMessage = new ChatMessage(
            textMessage.Role,
            [new TextContent(message.TextContent), image]);

        return imageMessage;
    }

    /// <summary>
    /// Rebuilds the history for the chat client.
    /// </summary>
    /// <param name="request">Request with history.</param>
    /// <returns>Converted history.</returns>
    private static IList<ChatMessage> CreateHistoryFromRequest(ChatRequest request)
    {
        var history = new List<ChatMessage>([new ChatMessage(
            ChatRole.System,
            "You are a helpful assistant.")]);

        foreach (Message message in request.Messages)
        {
            history.Add(ConvertMessage(message));
        }

        return history;
    }
}