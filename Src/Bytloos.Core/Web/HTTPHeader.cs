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
            PresetKey = key;
            Value = value;
        }

        /// <summary>
        /// Creates HTTP header object.
        /// </summary>
        /// <param name="key">Custom key.</param>
        /// <param name="value">Value.</param>
        public HTTPHeader(string key, string value)
        {
            CustomKey = key;
            Value = value;
        }

        /// <summary>
        /// Preset enum key.
        /// </summary>
        public HTTPHeaderKey PresetKey { get; }

        /// <summary>
        /// Custom string key.
        /// </summary>
        public string CustomKey { get; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }

        public override string ToString()
        {
            return $"PresetKey = {PresetKey}, CustomKey = '{CustomKey}', Value = '{Value}'";
        }
    }
}
