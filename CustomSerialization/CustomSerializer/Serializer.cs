using System.Text;
using System.Reflection;
using System.Globalization;

namespace CustomSerialization.CustomSerializer
{
    /// <summary>
    /// Serializer.
    /// </summary>
    public static class Serializer
    {
        #region public methods

        /// <summary>
        /// Serialize <paramref name="obj"/> as a string.
        /// </summary>
        public static string Serialize(object? obj)
        {
            return SerializeRecursive(obj);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Serialize <paramref name="obj"/> as a string recursively.
        /// </summary>
        private static string SerializeRecursive(object? obj)
        {
            if (obj == null)
            {
                return "null";
            }

            Type objType = obj.GetType();

            if (objType == typeof(string))
            {
                return $"\"{obj}\"";
            }

            if (objType.IsPrimitive || objType == typeof(decimal))
            {
                return FormatPrimitive(obj);
            }

            StringBuilder stringBuilder = new();
            stringBuilder.Append('{');

            FieldInfo[] fieldInfos = objType.GetFields();
            PropertyInfo[] propertyInfos = objType.GetProperties();

            if (fieldInfos.Length == 0 && propertyInfos.Length == 0)
            {
                stringBuilder.Append('}');
                return stringBuilder.ToString();
            }

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                AppendProperty(
                    stringBuilder,
                    fieldInfo.Name,
                    SerializeRecursive(fieldInfo.GetValue(obj)));
            }

            foreach (PropertyInfo propertyInfo in objType.GetProperties())
            {
                AppendProperty(
                    stringBuilder,
                    propertyInfo.Name,
                    SerializeRecursive(propertyInfo.GetValue(obj)));
            }

            stringBuilder.Length = stringBuilder.Length - 1;
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Format provided primitive <paramref name="obj"/> as a string.
        /// </summary>
        private static string FormatPrimitive(object obj)
        {
            return obj switch
            {
                float f => f.ToString("G", CultureInfo.InvariantCulture),
                double d => d.ToString("G", CultureInfo.InvariantCulture),
                decimal m => m.ToString(CultureInfo.InvariantCulture),
                _ => Convert.ToString(obj, CultureInfo.InvariantCulture) ?? string.Empty
            };
        }

        /// <summary>
        /// Append the property name/value pair in a formatter way
        /// to the <paramref name="stringBuilder"/>.
        /// </summary>
        private static void AppendProperty(
            StringBuilder stringBuilder,
            string propertyName,
            string propertyValue)
        {
            stringBuilder.Append($"\"{propertyName}\":{propertyValue},");
        }

        #endregion
    }
}
