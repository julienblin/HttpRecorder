using System;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Represents an exported HTTP requests.
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#entries
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Gets or sets the date and time stamp of the request start.
        /// </summary>
        public DateTimeOffset StartedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the total elapsed time of the request in milliseconds.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Request"/>.
        /// </summary>
        public Request Request { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Response"/>.
        /// </summary>
        public Response Response { get; set; }

        /// <summary>
        /// Gets or sets info about cache usage. NOT SUPPORTED.
        /// </summary>
        public object Cache { get; set; } = new object();

        /// <summary>
        /// Gets or sets the <see cref="Timings"/>.
        /// </summary>
        public Timings Timings { get; set; } = new Timings();
    }
}
