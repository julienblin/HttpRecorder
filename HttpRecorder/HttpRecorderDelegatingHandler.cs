using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpRecorder.Matchers;
using HttpRecorder.Repositories;
using HttpRecorder.Repositories.HAR;

namespace HttpRecorder
{
    /// <summary>
    /// <see cref="DelegatingHandler" /> that records HTTP interactions for integration tests.
    /// </summary>
    public class HttpRecorderDelegatingHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, Interaction, InteractionMessage> _matcher;
        private readonly IInteractionRepository _repository;

        private readonly SemaphoreSlim _interactionLock = new SemaphoreSlim(1, 1);
        private Interaction _interaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRecorderDelegatingHandler" /> class.
        /// </summary>
        /// <param name="interactionName">The name of the interaction (path to file record)</param>
        /// <param name="mode">The <see cref="HttpRecorderMode" />. Defaults to <see cref="HttpRecorderMode.Auto" />.</param>
        /// <param name="innerHandler">The inner <see cref="HttpMessageHandler" /> to configure. If not provided, <see cref="HttpClientHandler" /> will be used.</param>
        /// <param name="matcher">
        /// The function to use to match interactions with incoming <see cref="HttpRequestMessage"/>.
        /// Defaults to matching by <see cref="HttpMethod"/> and <see cref="HttpRequestMessage.RequestUri"/> - <see cref="DefaultMatcher"/>.
        /// </param>
        /// <param name="repository">
        /// The <see cref="IInteractionRepository"/> to use to read/write the interaction.
        /// Defaults to <see cref="HttpArchiveInteractionRepository"/>.
        /// </param>
        public HttpRecorderDelegatingHandler(
            string interactionName,
            HttpRecorderMode mode = HttpRecorderMode.Auto,
            HttpMessageHandler innerHandler = null,
            Func<HttpRequestMessage, Interaction, InteractionMessage> matcher = null,
            IInteractionRepository repository = null)
            : base(innerHandler ?? new HttpClientHandler())
        {
            InteractionName = interactionName;
            Mode = mode;
            _matcher = matcher ?? DefaultMatcher.Matcher;
            _repository = repository ?? new HttpArchiveInteractionRepository();
        }

        /// <summary>
        /// Gets the name of the interaction.
        /// </summary>
        public string InteractionName { get; }

        /// <summary>
        /// Gets or sets the <see cref="HttpRecorderMode" />.
        /// </summary>
        public HttpRecorderMode Mode { get; set; }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Mode == HttpRecorderMode.Passthrough)
            {
                var response = await base.SendAsync(request, cancellationToken);
                return response;
            }

            await _interactionLock.WaitAsync();
            try
            {
                var executionMode = await ResolveRecorderMode(cancellationToken);

                if (executionMode == HttpRecorderMode.Replay)
                {
                    if (_interaction == null)
                    {
                        _interaction = await _repository.LoadAsync(InteractionName, cancellationToken);
                    }

                    var interactionMessage = _matcher(request, _interaction);
                    if (interactionMessage == null)
                    {
                        throw new HttpRecorderException($"Unable to find a matching interaction for request {request.Method} {request.RequestUri}.");
                    }

                    return interactionMessage.Response;
                }

                var start = DateTimeOffset.Now;
                var sw = Stopwatch.StartNew();
                var innerResponse = await base.SendAsync(request, cancellationToken);
                sw.Stop();
                if (_interaction == null)
                {
                    _interaction = new Interaction(InteractionName);
                }

                _interaction.Messages.Add(
                    new InteractionMessage(
                        innerResponse,
                        new InteractionMessageTimings(start, sw.Elapsed)));
                _interaction = await _repository.StoreAsync(_interaction, cancellationToken);

                // We do intentionally return the round-triped (from repository) interaction.
                return _interaction.Messages.Last().Response;
            }
            finally
            {
                _interactionLock.Release();
            }
        }

        /// <summary>
        /// Resolves <see cref="HttpRecorderMode.Auto"/>, if this is the case, otherwise returns the current <see cref="Mode"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The resolve <see cref="HttpRecorderMode"/> guaranteed to never be <see cref="HttpRecorderMode.Auto"/>.</returns>
        private async Task<HttpRecorderMode> ResolveRecorderMode(CancellationToken cancellationToken)
        {
            if (Mode == HttpRecorderMode.Auto)
            {
                return (await _repository.ExistsAsync(InteractionName, cancellationToken))
                    ? HttpRecorderMode.Replay
                    : HttpRecorderMode.Record;
            }

            return Mode;
        }
    }
}
