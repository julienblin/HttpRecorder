using System.Diagnostics.CodeAnalysis;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Contains detailed info about the response.
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#response
    /// </summary>
    public class Response : Message
    {
        /// <summary>
        /// Gets or sets the response status.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the response status description.
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Gets or sets the details about the response body.
        /// </summary>
        public Content Content { get; set; } = new Content();

        /// <summary>
        /// Gets or sets the redirection target URL from the Location response header.
        /// </summary>
        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "Conform to specification that can include empty strings.")]
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
