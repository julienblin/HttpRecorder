﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        /// Defaults to matching by <see cref="HttpMethod"/> and <see cref="HttpRequestMessage.RequestUri"/>.
        /// </param>
        /// <param name="repository">The <see cref="IInteractionRepository"/> to use to read/write the interaction.</param>
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
            _matcher = matcher;
            _repository = repository;
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
                return await base.SendAsync(request, cancellationToken);
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

                var innerResponse = await base.SendAsync(request, cancellationToken);
                if (_interaction == null)
                {
                    _interaction = new Interaction(InteractionName);
                }

                var newInteractionMessage = new InteractionMessage(innerResponse);
                _interaction.Messages.Add(newInteractionMessage);
                await _repository.StoreAsync(_interaction, cancellationToken);
                return newInteractionMessage.Response;
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
