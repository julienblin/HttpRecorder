using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string interactionName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(File.Exists(interactionName));
        }

        /// <inheritdoc />
        public Task<Interaction> LoadAsync(string interactionName, CancellationToken cancellationToken = default)
        {
            var archive = JsonConvert.DeserializeObject<HttpArchive>(
                File.ReadAllText(interactionName, Encoding.UTF8),
                _jsonSettings);

            return Task.FromResult(archive.ToInteraction(interactionName));
        }

        /// <inheritdoc />
        public Task<Interaction> StoreAsync(Interaction interaction, CancellationToken cancellationToken = default)
        {
            var archive = new HttpArchive(interaction);
            var archiveDirectory = Path.GetDirectoryName(interaction.Name);
            if (!string.IsNullOrWhiteSpace(archiveDirectory) && !Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            File.WriteAllText(interaction.Name, JsonConvert.SerializeObject(archive, Formatting.Indented, _jsonSettings));

            return Task.FromResult(archive.ToInteraction(interaction.Name));
        }
    }
}
