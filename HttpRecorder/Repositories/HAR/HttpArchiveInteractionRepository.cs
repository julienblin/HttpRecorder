using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// <see cref="IInteractionRepository"/> implementation that stores <see cref="Interaction"/>
    /// in files in the HTTP Archive format (https://en.wikipedia.org/wiki/.har / https://w3c.github.io/web-performance/specs/HAR/Overview.html).
    /// </summary>
    /// <remarks>
    /// The interactionName parameter is used as the file path.
    /// </remarks>
    public class HttpArchiveInteractionRepository : IInteractionRepository
    {
        /*private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };*/

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string interactionName, CancellationToken cancellationToken = default)
        {
            return File.Exists(interactionName);
        }

        /// <inheritdoc />
        public Task<Interaction> LoadAsync(string interactionName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Interaction> StoreAsync(Interaction interaction, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
