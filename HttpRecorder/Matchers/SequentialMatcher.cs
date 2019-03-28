using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

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
        /// <param name="part">Specify a <see cref="UriPartial"/> to restrict the matching to a subset of the request <see cref="Uri"/>.</param>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByRequestUri(UriPartial part = UriPartial.Query)
            => By((request, message) => string.Equals(request.RequestUri?.GetLeftPart(part), message.Response.RequestMessage.RequestUri?.GetLeftPart(part), StringComparison.InvariantCulture));

        /// <summary>
        /// Adds a rule that matches by comparing request header values.
        /// </summary>
        /// <param name="headerName">The name of the header to compare values from.</param>
        /// <param name="stringComparison">Allows customization of the string comparison.</param>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByHeader(string headerName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
            => By((request, message) =>
            {
                string requestHeader = null;
                string interactionHeader = null;

                if (request.Headers.TryGetValues(headerName, out var requestValues))
                {
                    requestHeader = string.Join(",", requestValues);
                }

                if (request.Content != null && request.Content.Headers.TryGetValues(headerName, out var requestContentValues))
                {
                    requestHeader = string.Join(",", requestContentValues);
                }

                if (message.Response.RequestMessage.Headers.TryGetValues(headerName, out var interactionValues))
                {
                    interactionHeader = string.Join(",", interactionValues);
                }

                if (message.Response.RequestMessage.Content != null && message.Response.RequestMessage.Content.Headers.TryGetValues(headerName, out var interactionContentValues))
                {
                    interactionHeader = string.Join(",", interactionContentValues);
                }

                return string.Equals(requestHeader, interactionHeader, stringComparison);
            });

        /// <summary>
        /// Adds a rule that matches by binary comparing the <see cref="HttpRequestMessage.Content"/>.
        /// </summary>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByContent()
            => By((request, message) => StructuralComparisons.StructuralComparer.Compare(
                    request.Content?.ReadAsByteArrayAsync()?.Result,
                    message.Response.RequestMessage.Content?.ReadAsByteArrayAsync()?.Result) == 0);

        /// <summary>
        /// Adds a rule that matches by comparing the JSON content of the requests.
        /// </summary>
        /// <typeparam name="T">The json object type.</typeparam>
        /// <param name="equalityComparer"><see cref="IEqualityComparer{T}"/> to use. Defaults to <see cref="EqualityComparer{T}.Default"/>.</param>
        /// <param name="jsonSerializerSettings">The <see cref="JsonSerializerSettings"/> to use.</param>
        /// <returns>A new <see cref="SequentialMatcher"/></returns>
        public SequentialMatcher ByJsonContent<T>(
            IEqualityComparer<T> equalityComparer = null,
            JsonSerializerSettings jsonSerializerSettings = null)
            => By((request, message) =>
            {
                var requestContent = request.Content?.ReadAsStringAsync()?.Result;
                var requestJson = !string.IsNullOrEmpty(requestContent) ? JsonConvert.DeserializeObject<T>(requestContent, jsonSerializerSettings) : default(T);

                var interactionContent = message.Response.RequestMessage.Content?.ReadAsStringAsync()?.Result;
                var interactionJson = !string.IsNullOrEmpty(interactionContent) ? JsonConvert.DeserializeObject<T>(interactionContent, jsonSerializerSettings) : default(T);

                if (equalityComparer == null)
                {
                    equalityComparer = EqualityComparer<T>.Default;
                }

                return equalityComparer.Equals(requestJson, interactionJson);
            });
    }
}
