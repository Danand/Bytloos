using System.Runtime.InteropServices;
using System.Text;

namespace Bytloos
{
    /// <summary>
    /// Simple INI wrapper.
    /// </summary>
    public class INIFile
    {
        private const int       DEFAULT_STRING_BUILDER_CAPACITY = 255;
        private const string    KERNEL_LIB_NAME                 = "kernel32";
        private const string    DEFAULT_SECTION                 = "Settings";

        private readonly string path;

        [DllImport(KERNEL_LIB_NAME)]
        private static extern long WritePrivateProfileString(
            string  section,
            string  key,
            string  value,
            string  filePath);

        [DllImport(KERNEL_LIB_NAME)]
        private static extern int GetPrivateProfileString(
            string          section,
            string          key,
            string          defaultValue,
            StringBuilder   outValue,
            int             size,
            string          filePath);

        /// <summary>
        /// Creates or loads INI file.
        /// </summary>
        /// <param name="path"></param>
        public INIFile(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Read or write INI value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string this[string key]
        {
            get { return this[DEFAULT_SECTION, key]; }
            set { this[DEFAULT_SECTION, key] = value; }
        }

        /// <summary>
        /// Read or write INI value.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <returns>Value.</returns>
        public string this[string section, string key]
        {
            get { return Read(section, key); }
            set { Write(section, key, value); }
        }

        /// <summary>
        /// Reads value from INI file.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void Write(string key, string value)
        {
            Write(DEFAULT_SECTION, key, value);
        }

        /// <summary>
        /// Reads value from INI file.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(
                section:    section,
                key:        key,
                value:      value,
                filePath:   path);
        }

        /// <summary>
        /// Reads value from INI file.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="capacity">Capacity of StringBuilder getting value.</param>
        /// <returns>Value.</returns>
        public string Read(string key, uint capacity = DEFAULT_STRING_BUILDER_CAPACITY)
        {
            return Read(DEFAULT_SECTION, key, capacity);
        }

        /// <summary>
        /// Reads value from INI file.
        /// </summary>
        /// <param name="section">Section.</param>
        /// <param name="key">Key.</param>
        /// <param name="capacity">Capacity of StringBuilder getting value.</param>
        /// <returns>Value.</returns>
        public string Read(string section, string key, uint capacity = DEFAULT_STRING_BUILDER_CAPACITY)
        {
            var stringBuilder = new StringBuilder((int)capacity);

            GetPrivateProfileString(
                section:        section,
                key:            key,
                defaultValue:   string.Empty,
                outValue:       stringBuilder,
                size:           stringBuilder.Capacity,
                filePath:       path);

            return stringBuilder.ToString();
        }
    }
}
