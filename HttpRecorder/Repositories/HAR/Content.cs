namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Describes details about response content
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#content
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Gets or sets the length of the returned content in bytes.
        /// </summary>
        public long Size { get; set; } = -1;

        /// <summary>
        /// Gets or sets the MIME type of the response text (value of the Content-Type response header).
        /// The charset attribute of the MIME type is included (if available).
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the response body sent from the server or loaded from the browser cache.
        /// This field is populated with textual content only. The text field is either HTTP decoded text
        /// or a encoded (e.g. "base64") representation of the response body. Leave out this field if the information is not available.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the encoding used for response text field e.g "base64".
        /// Leave out this field if the text field is HTTP decoded (decompressed and unchunked), than trans-coded from its original character set into UTF-8.
        /// </summary>
        public string Encoding { get; set; }
    }
}
