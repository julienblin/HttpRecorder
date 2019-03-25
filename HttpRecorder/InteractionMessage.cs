using System.Net.Http;

namespace HttpRecorder
{
    /// <summary>
    /// Represents a single HTTP Interaction (Request/Response).
    /// <see cref="HttpRequestMessage"/> is in the <see cref="HttpResponseMessage.RequestMessage"/> property.
    /// </summary>
    public class InteractionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionMessage"/> class.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        public InteractionMessage(HttpResponseMessage response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public HttpResponseMessage Response { get; }
    }
}
