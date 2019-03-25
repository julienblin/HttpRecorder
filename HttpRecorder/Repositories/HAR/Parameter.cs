namespace HttpRecorder.Repositories.HAR
{
    /// <summary>
    /// Base class for HTTP Archive name/value parameters
    /// </summary>
    public abstract class Parameter
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
    }
}
