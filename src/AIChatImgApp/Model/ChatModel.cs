using Microsoft.Identity.Client;
using Microsoft.VisualBasic;

namespace AIChatImgApp.Model;

/// <summary>
/// Chat with history.
/// </summary>
/// <param name="Messages">The history of <see cref="Message"/>instances.</param>
public record ChatRequest(List<Message> Messages);

/// <summary>
/// Image request.
/// </summary>
/// <param name="Filename">Original filename.</param>
/// <param name="ImageBytes">Image content.</param>
/// <param name="MimeType">Image type, ex: image/jpeg.</param>
public record ImageRequest(string Filename, byte[] ImageBytes, string MimeType);

/// <summary>
/// Message with multimodal content.
/// </summary>
/// <param name="isAssistant">Direction of message.</param>
/// <param name="textContent">Text of the message.</param>
/// <param name="image">Optional image attachment</param>
public class Message
{
    public Message(bool isAssistant, string textContent, ImageRequest? image = null)
    {
        IsAssistant = isAssistant;
        TextContent = textContent;
        Image = image;
    }

    public bool IsAssistant { get; private set; }
    public string TextContent { get; set; }
    public ImageRequest? Image { get; private set; }
    public bool Streaming { get; set; }
    public string Id { get; init; } = Guid.NewGuid().ToString();
}