using AIChatImgApp.Model;
using AIChatImgApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AIChatImgApp.Components.Chat;

public partial class Chat
{    
    /// <summary>
    /// References to manage Enter keypress and focus
    /// </summary>
    ElementReference textArea;
    ElementReference button;

    /// <summary>
    /// The API for communicating with the LLM.
    /// </summary>
    [Inject]
    internal ChatService? ChatHandler { get; init; }

    /// <summary>
    /// If the user uploaded an image, it is stored here.
    /// </summary>
    private ImageRequest? imageContent;

    /// <summary>
    /// This context is used for communication between the parent chat control and the
    /// <see cref="ImageUploader"/> component.
    /// </summary>
    private ImageContext imageContext = new();

    /// <summary>
    /// This flag is used to control the UI flow between the components. When set to true,
    /// it will show the image upload dialog.
    /// </summary>
    bool imageUploader = false;

    /// <summary>
    /// This flag is used to control the UI flow between the components. When set to true,
    /// it signals that the user wishes to add an image to the message. The dialog will
    /// show when they tap "send" or press the Enter key.
    /// </summary>
    bool addImage = false;

    /// <summary>
    /// Chat history.
    /// </summary>
    readonly List<Message> messages = [];

    /// <summary>
    /// What the user is typing.
    /// </summary>
    string? userMessageText;

    /// <summary>
    /// Initial message for display purposes within the chat history.
    /// </summary>
    readonly Message initialMessage = new(true, "Hi, I'm a helpful assistant, how may I assist you?");

    protected override async void OnAfterRender(bool firstRender)
    {
        await JsOp.InvokeVoidAsync("init", [textArea, button]);
        base.OnAfterRender(firstRender);
    }

    /// <summary>
    /// This method is an event handler that allows the user to submit the message by pressing the Enter key.
    /// </summary>
    /// <param name="e">The <seealso cref="KeyboardEventArgs"/> that raise the event.</param>
    public async void CheckKeyPress(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await SendMessage();
        }
    }

    /// <summary>
    /// This is called any time the context is changed in the <see cref="ImageUploader"/> component. It is used to
    /// control the UI flow between the components.
    /// </summary>
    /// <param name="ctx">The changed instance of <see cref="ImageContext"/>.</param>
    public async void OnChanged(ImageContext ctx)
    {
        if (ctx.CancelRequested)
        {
            await CancelImageRequestAsync();
            return;
        }

        if (ctx.Confirmed)
        {
            await AttachImageRequestAsync(ctx);
        }
    }

    /// <summary>
    /// This is invoked when the user cancels their request to add an image to the message.
    /// </summary>
    /// <returns>An asynchronous operation.</returns>
    private async Task CancelImageRequestAsync()
    {
        imageUploader = false;
        addImage = false;
        imageContent = null;
        await InvokeAsync(() => StateHasChanged());
        return;
    }

    /// <summary>
    /// This is invoked when the user confirms their request to add an image to the message.
    /// </summary>
    /// <param name="ctx">The <see cref="ImageContext"/> instance with the user's image.</param>
    /// <returns>An asynchronous operation.</returns>
    private async Task AttachImageRequestAsync(ImageContext ctx)
    {
        imageUploader = false;
        addImage = false;
        imageContent = ctx.ImageContent;
        await InvokeAsync(() => StateHasChanged());
        await SendMessage();
        return;
    }

    /// <summary>
    /// This method is used to send the user's message to the LLM. It is the main "communication loop."
    /// </summary>
    /// <returns>An asynchronous operation.</returns>
    async Task SendMessage()
    {
        // should we show the attach image dialog?
        if (addImage == true && imageUploader == false && imageContent == null)
        {
            imageUploader = true;
            imageContext = new()
            {
                Message = userMessageText,
                Confirmed = false,
                CancelRequested = false,
                ImageContent = null
            };
            StateHasChanged();
            return;
        }

        if (ChatHandler is null) { return; }

        // if they entered something
        if (!string.IsNullOrWhiteSpace(userMessageText))
        {
            // a single message can contain multimodal content. The implementation of the Microsoft AI extensions will translate the payload as necessary for the chosen
            // LLM provider.
            var message = new Message(
                false,
                userMessageText,
                imageContent == null
                    ? null
                    : new ImageRequest(imageContent.Filename, imageContent.ImageBytes.ToArray(), imageContent.MimeType));

            messages.Add(message);
            imageContent = null;
            userMessageText = null;

            ChatRequest request = new(messages);
            var assistantText = "...";

            Message assistantMessage = new(true, assistantText)
            {
                Streaming = true
            };
            messages.Add(assistantMessage);
            StateHasChanged();

            IAsyncEnumerable<string> chunks = ChatHandler.Stream(request);

            // using streaming, the message can come in "chunks"so we'll build
            // the chunk and swap out the agent message until it's done
            await foreach (var chunk in chunks)
            {
                if (assistantText == "...")
                {
                    assistantText = string.Empty;
                }

                assistantText += chunk;
                assistantMessage.TextContent = assistantText;
                StateHasChanged();
            }

            assistantMessage.Streaming = false;
        }
    }
}