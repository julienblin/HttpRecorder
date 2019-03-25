using System;
using System.Linq;
using System.Net.Http;

namespace HttpRecorder.Matchers
{
    /// <summary>
    /// Default interaction matcher provider.
    /// </summary>
    public static class DefaultMatcher
    {
        /// <summary>
        /// Gets the default interaction matcher that matches based on <see cref="HttpMethod"/> and <see cref="HttpRequestMessage.RequestUri"/>.
        /// </summary>
        public static Func<HttpRequestMessage, Interaction, InteractionMessage> Matcher { get; } = (HttpRequestMessage request, Interaction interaction) =>
        {
            var matchedInteraction = interaction.Messages.FirstOrDefault(
                x => x.Response.RequestMessage.Method == request.Method && x.Response.RequestMessage.RequestUri == request.RequestUri);
            if (matchedInteraction != null)
            {
                interaction.Messages.Remove(matchedInteraction);
                return matchedInteraction;
            }

            return null;
        };
    }
}
