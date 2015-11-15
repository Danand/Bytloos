namespace Bytloos.Web
{
    /// <summary>
    /// HTTP header.
    /// </summary>
    public class HTTPHeader
    {
        /// <summary>
        /// Creates HTTP header object.
        /// </summary>
        /// <param name="key">Preset key.</param>
        /// <param name="value">Value.</param>
        public HTTPHeader(HTTPHeaderKey key, string value)
        {
            this.PresetKey = key;
            this.Value = value;
        }

        /// <summary>
        /// Creates HTTP header object.
        /// </summary>
        /// <param name="key">Custom key.</param>
        /// <param name="value">Value.</param>
        public HTTPHeader(string key, string value)
        {
            this.CustomKey = key;
            this.Value = value;
        }

        /// <summary>
        /// Preset enum key.
        /// </summary>
        public HTTPHeaderKey PresetKey { get; private set; }

        /// <summary>
        /// Custom string key.
        /// </summary>
        public string CustomKey { get; private set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }
    }
}
