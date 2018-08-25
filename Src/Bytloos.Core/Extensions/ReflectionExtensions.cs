using System.Reflection;

namespace Bytloos.Extensions
{
    public static class ReflectionExtensions
    {
        public static TValue GetFieldValue<TObject, TValue>(this TObject source, string fieldName)
        {
            var fieldInfo
                = typeof(TObject).GetField(
                    name:           fieldName,
                    bindingAttr:    BindingFlags.Instance | BindingFlags.NonPublic);

            var result = (TValue)fieldInfo.GetValue(source);

            return result;
        }

        public static void SetFieldValue<TObject, TValue>(this TObject source, string fieldName, TValue value)
        {
            var fieldInfo
                = typeof(TObject).GetField(
                    name:           fieldName,
                    bindingAttr:    BindingFlags.Instance | BindingFlags.NonPublic);

            fieldInfo.SetValue(source, value);
        }
    }
}
