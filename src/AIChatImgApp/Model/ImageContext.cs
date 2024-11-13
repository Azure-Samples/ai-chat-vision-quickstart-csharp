namespace AIChatImgApp.Model
{
    /// <summary>
    /// A view model for interacting with the image uploader.
    /// </summary>
    public class ImageContext
    {
        /// <summary>
        /// Set to true to close dialog.
        /// </summary>
        public bool CancelRequested { get; set; }

        /// <summary>
        /// Set to true to attach image to message.
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// The message to send.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The image to attach to the message.
        /// </summary>
        public ImageRequest? ImageContent { get; set; }
    }
}
