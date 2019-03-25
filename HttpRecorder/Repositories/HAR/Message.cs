using System.Collections.Generic;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Base class for HAR messages.
    /// </summary>
    public abstract class Message
    {
        /// <summary>
        /// Gets or sets the HTTP version.
        /// </summary>
        public string HttpVersion { get; set; }

        /// <summary>
        /// Gets the list of cookie objects. NOT SUPPORTED.
        /// </summary>
        public IList<object> Cookies { get; private set; } = new List<object>();

        /// <summary>
        /// Gets the list of <see cref="Header"/>
        /// </summary>
        public IList<Header> Headers { get; private set; } = new List<Header>();

        /// <summary>
        /// Gets or sets the total number of bytes from the start of the HTTP request message until (and including) the double CRLF before the body.
        /// Set to -1 if the info is not available.
        /// </summary>
        public int HeadersSize { get; set; } = -1;

        /// <summary>
        /// Gets or sets the size of the request body (POST data payload) in bytes.
        /// Set to -1 if the info is not available.
        /// </summary>
        public int BodySize { get; set; } = -1;
    }
}
