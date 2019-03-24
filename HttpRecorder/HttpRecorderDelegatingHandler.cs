using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpRecorder
{
    /// <summary>
    /// <see cref="DelegatingHandler" /> that records HTTP interactions for integration tests.
    /// </summary>
    public class HttpRecorderDelegatingHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRecorderDelegatingHandler" /> class.
        /// </summary>
        /// <param name="interactionName">The name of the interaction (path to file record)</param>
        /// <param name="mode">The <see cref="HttpRecorderMode" />. Defaults to <see cref="HttpRecorderMode.Auto" />.</param>
        /// <param name="innerHandler">The inner <see cref="HttpMessageHandler" /> to configure. If not provided, <see cref="HttpClientHandler" /> will be used.</param>
        public HttpRecorderDelegatingHandler(
        string interactionName,
        HttpRecorderMode mode = HttpRecorderMode.Auto,
        HttpMessageHandler innerHandler = null)
            : base(innerHandler ?? new HttpClientHandler())
        {
            this.Mode = mode;
            this.InteractionName = interactionName;
        }

        /// <summary>
        /// Gets the name of the interaction.
        /// </summary>
        public string InteractionName { get; }

        /// <summary>
        /// Gets the <see cref="HttpRecorderMode" />.
        /// </summary>
        public HttpRecorderMode Mode { get; }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (Mode == HttpRecorderMode.Passthrough)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            throw new NotImplementedException();
        }
    }
}
