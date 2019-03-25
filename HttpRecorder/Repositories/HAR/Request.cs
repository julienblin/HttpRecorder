using System;
using System.Collections.Generic;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Contains detailed info about performed request.
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#request
    /// </summary>
    public class Request : Message
    {
        /// <summary>
        /// Gets or sets the request method (GET, POST, ...).
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the absolute URL of the request (fragments are not included).
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets the list of <see cref="QueryParameter"/> parameters.
        /// </summary>
        public IList<QueryParameter> QueryString { get; private set; } = new List<QueryParameter>();

        /// <summary>
        /// Gets or sets the <see cref="PostData"/>.
        /// </summary>
        public PostData PostData { get; set; }
    }
}
