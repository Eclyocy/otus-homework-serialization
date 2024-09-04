using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CustomSerialization.CustomSerializer
{
    /// <summary>
    /// Deserializer for <see cref="Serializer"/>.
    /// </summary>
    public static class Deserializer
    {
        #region private fields

        /// <summary>
        /// Regex for capturing named properties.<br/>
        /// Supported properties:<br/>
        /// * object;<br/>
        /// * string;<br/>
        /// * number;</br/>
        /// * boolean values;<br/>
        /// * null.
        /// </summary>
        private static readonly Regex _propertyRegex = new(
            """
            "(\w+)":\s*((?:\{[^}]*\})|(?:"[^"]*")|(?:-?\d+(?:\.\d+)?)|(?:true|false|null))
            """);

        #endregion

        #region public methods

        /// <summary>
        /// Deserialize <paramref name="serialized"/> as object of <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">Resulting object type.</typeparam>
        public static T? Deserialize<T>(string serialized)
        {
            return (T?)DeserializeObject(serialized, typeof(T));
        }

        /// <summary>
        /// Deserialize <paramref name="serialized"/> as object of <paramref name="type"/> type.
        /// </summary>
        public static object? Deserialize(string serialized, Type type)
        {
            return DeserializeObject(serialized, type);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Try deserializing <paramref name="serialized"/> string
        /// as an object of <paramref name="type"/> type.
        /// </summary>
        private static object? DeserializeObject(string serialized, Type type)
        {
            serialized = serialized.Trim();

            if (serialized == "null")
            {
                return null;
            }

            if (TryDeserialisePrimitiveObject(serialized, type, out object result))
            {
                return result;
            }

            if (serialized.StartsWith('{') &&
                serialized.EndsWith('}'))
            {
                return DeserializeComplexObject(serialized, type);
            }

            throw new ArgumentException($"Cannot deserialize: {serialized}");
        }

        /// <summary>
        /// Try deserializing <paramref name="serialized"/> string as a primitive object
        /// (an object of the provided <paramref name="type"/> primitive type).
        /// </summary>
        private static bool TryDeserialisePrimitiveObject(
            string serialized,
            Type type,
            out object? result)
        {
            result = null;

            if (type == typeof(string))
            {
                result = serialized.Trim('"');
                return true;
            }

            if (type == typeof(int) &&
                int.TryParse(serialized, out int intValue))
            {
                result = intValue;
                return true;
            }

            if (type == typeof(float) &&
                float.TryParse(serialized, CultureInfo.InvariantCulture, out float floatValue))
            {
                result = floatValue;
                return true;
            }

            if (type == typeof(double) &&
                double.TryParse(serialized, CultureInfo.InvariantCulture, out double doubleValue))
            {
                result = doubleValue;
                return true;
            }

            if (type == typeof(decimal) &&
                decimal.TryParse(serialized, CultureInfo.InvariantCulture, out decimal decimalValue))
            {
                result = decimalValue;
                return true;
            }

            if (type == typeof(bool) &&
                bool.TryParse(serialized, out bool boolValue))
            {
                result = boolValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try deserializing <paramref name="serialized"/> string as a complex object
        /// (an object of the provided <paramref name="type"/> complex type).
        /// </summary>
        private static object? DeserializeComplexObject(string serialized, Type type)
        {
            Dictionary<string, string> properties = ParseProperties(serialized);

            object? instance = CreateInstance(properties, type);

            foreach (KeyValuePair<string, string> property in properties)
            {
                SetProperty(instance, property.Key, property.Value);
            }

            return instance;
        }

        /// <summary>
        /// Parse object properties name/value pairs
        /// from the provided <paramref name="serialized"/> string.
        /// </summary>
        private static Dictionary<string, string> ParseProperties(string serialized)
        {
            Dictionary<string, string> properties = new();

            foreach (Match match in _propertyRegex.Matches(serialized))
            {
                string name = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                properties[name] = value;
            }

            return properties;
        }

        /// <summary>
        /// Try creating an instance of the <paramref name="type"/> type
        /// with provided <paramref name="properties"/> properties.<br/>
        /// Check its existing constructors first to support properties with protected setters first.
        /// </summary>
        private static object? CreateInstance(Dictionary<string, string> properties, Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();
                //.OrderByDescending(c => c.GetParameters().Length).ToArray();

            foreach (ConstructorInfo constructor in constructors)
            {
                bool isConstructorSuitable = false;

                ParameterInfo[] constructorParameters = constructor.GetParameters();

                object[] constructorArguments = new object[constructorParameters.Length];

                for (int i = 0; i < constructorParameters.Length; i++)
                {
                    ParameterInfo constructorParameter = constructorParameters[i];

                    if (TryGetValueForParameter(properties, constructorParameter, out object? value))
                    {
                        if (value != null)
                        {
                            constructorArguments[i] = value;
                        }
                    }
                    else
                    {
                        break;
                    }

                    isConstructorSuitable = true;
                }

                if (isConstructorSuitable)
                {
                    return constructor.Invoke(constructorArguments);
                }
            }

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Try getting a suitable value for the <paramref name="constructorParameter"/>
        /// from the <paramref name="properties"/>.
        /// </summary>
        private static bool TryGetValueForParameter(
            Dictionary<string, string> properties,
            ParameterInfo constructorParameter,
            out object? value)
        {
            if (constructorParameter.Name == null)
            {
                value = null;
                return false;
            }

            // Try exact name match.
            if (properties.TryGetValue(constructorParameter.Name, out string? stringValue))
            {
                value = DeserializeObject(stringValue, constructorParameter.ParameterType);
                return true;
            }

            // Try type match.
            KeyValuePair<string, string> matchingProperty = properties.FirstOrDefault(p =>
            {
                try
                {
                    DeserializeObject(p.Value, constructorParameter.ParameterType);
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            if (!string.IsNullOrEmpty(matchingProperty.Key))
            {
                value = DeserializeObject(matchingProperty.Value, constructorParameter.ParameterType);
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Try setting <paramref name="value"/> value to <paramref name="propertyName"/> property
        /// of <paramref name="instance"/> object.
        /// </summary>
        private static void SetProperty(object? instance, string propertyName, string value)
        {
            if (instance == null)
            {
                return;
            }

            PropertyInfo? property = instance.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                property.SetValue(instance, DeserializeObject(value, property.PropertyType));
                return;
            }

            FieldInfo? field = instance.GetType().GetField(propertyName);
            if (field != null)
            {
                field.SetValue(instance, DeserializeObject(value, field.FieldType));
            }
        }

        #endregion
    }
}
