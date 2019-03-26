using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Contains detailed info about performed request.
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#request
    /// </summary>
    public class Request : Message
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>
        public Request()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class from <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to initialize from.</param>
        public Request(HttpRequestMessage request)
        {
            HttpVersion = $"{HTTPVERSIONPREFIX}{request.Version}";
            Method = request.Method.ToString();
            Url = request.RequestUri;

            foreach (var header in request.Headers)
            {
                Headers.Add(new Header(header));
            }

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    Headers.Add(new Header(header));
                }

                BodySize = request.Content.ReadAsByteArrayAsync().Result.Length;
                PostData = new PostData(request.Content);
            }
        }

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

        /// <summary>
        /// Returns a <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <returns>The <see cref="HttpRequestMessage"/> created from this.</returns>
        public HttpRequestMessage ToHttpRequestMessage()
        {
            var request = new HttpRequestMessage
            {
                Content = PostData?.ToHttpContent(),
                Method = new HttpMethod(Method),
                RequestUri = Url,
                Version = GetVersion(),
            };
            AddHeadersWithoutValidation(request.Headers);
            AddHeadersWithoutValidation(request.Content?.Headers);

            return request;
        }
    }
}
