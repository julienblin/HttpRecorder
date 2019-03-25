using System.Collections.Generic;

namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Describes posted data.
    /// https://w3c.github.io/web-performance/specs/HAR/Overview.html#postData
    /// </summary>
    public class PostData
    {
        /// <summary>
        /// Gets or sets the mime type of posted data.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets the list of <see cref="PostedParam"/>.
        /// </summary>
        public IList<PostedParam> Params { get; private set; } = new List<PostedParam>();

        /// <summary>
        /// Gets or sets plain text posted data.
        /// </summary>
        public string Text { get; set; }
    }
}
