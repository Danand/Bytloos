namespace Bytloos.CSV
{
    /// <summary>
    /// Provides CSV document serialization for fields and properties
    /// of objects of classes implementing this interface.
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// Gets string representation of field.
        /// </summary>
        /// <returns>String representation of field.</returns>
        string GetStringValue();

        /// <summary>
        /// Parses object from it's string representation.
        /// </summary>
        /// <param name="value">String representation of field.</param>
        void SetValueFromString(string value);
    }
}
