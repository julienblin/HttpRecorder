using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace HttpRecorder.Matchers
{
    /// <summary>
    /// <see cref="IRequestMatcher"/> implementation that matches <see cref="HttpRequestMessage"/>
    /// in sequence by removing used <see cref="InteractionMessage"/> from the <see cref="Interaction"/>.
    /// Additional rules can be specified.
    /// </summary>
    public sealed class SequentialMatcher : IRequestMatcher
    {
        private readonly IEnumerable<Func<HttpRequestMessage, InteractionMessage, bool>> _rules;

        private SequentialMatcher(IEnumerable<Func<HttpRequestMessage, InteractionMessage, bool>> rules = null)
        {
            _rules = rules ?? new List<Func<HttpRequestMessage, InteractionMessage, bool>>();
        }

        /// <summary>
        /// Gets a new <see cref="SequentialMatcher"/>.
        /// </summary>
        public static SequentialMatcher Match { get => new SequentialMatcher(); }

        /// <inheritdoc />
        InteractionMessage IRequestMatcher.Match(HttpRequestMessage request, Interaction interaction)
        {
            IEnumerable<InteractionMessage> query = interaction.Messages;

            foreach (var rule in _rules)
            {
                query = query.Where(x => rule(request, x));
            }

            var matchedInteraction = query.FirstOrDefault();
            if (matchedInteraction != null)
            {
                interaction.Messages.Remove(matchedInteraction);
                return matchedInteraction;
            }

            return null;
        }

        /// <summary>
        /// Returns a new <see cref="SequentialMatcher"/> with the added <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <returns>A new <see cref="SequentialMatcher"/>.</returns>
        public SequentialMatcher By(Func<HttpRequestMessage, InteractionMessage, bool> rule)
            => new SequentialMatcher(_rules.Concat(new[] { rule }));

        /// <summary>
        /// Adds a rule that matches by <see cref="HttpMethod"/>.
        /// </summary>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByHttpMethod()
            => By((request, message) => request.Method == message.Response.RequestMessage.Method);

        /// <summary>
        /// Adds a rule that matches by <see cref="HttpRequestMessage.RequestUri"/>.
        /// </summary>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByRequestUri()
            => By((request, message) => request.RequestUri == message.Response.RequestMessage.RequestUri);
    }
}
