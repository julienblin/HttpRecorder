namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Represents an HTTP Archive file content (https://w3c.github.io/web-performance/specs/HAR/Overview.html).
    /// </summary>
    public class HttpArchive
    {
        /// <summary>
        /// Gets or sets the <see cref="Log"/>.
        /// </summary>
        public Log Log { get; set; } = new Log();
    }
}
