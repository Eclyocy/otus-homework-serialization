using CustomSerialization.CustomSerializer;
using CustomSerialization.Helpers;
using CustomSerialization.Models;
using Newtonsoft.Json;

namespace CustomSerialization
{
    public static class Program
    {
        #region private constants

        private const int Attempts = 100000;

        #endregion

        #region public methods

        public static void Main()
        {
            F testF = new(1, 2, 3, 4, 5);
            G testG = new(5, "hello");

            object?[] testObjects = [
                new EmptyClass(),
                null,
                true,
                false,
                1,
                2.5,
                testF,
                testG,
                new H(testF, testG),
                new MultipleConstructorsClass(),
                new MultipleConstructorsClass(1.5),
                new MultipleConstructorsClass("hello!"),
                new MultipleConstructorsClass(2, "hiya")
            ];

            foreach (object? testObject in testObjects)
            {
                Type requiredType = testObject?.GetType() ?? typeof(string);

                TimedResult<string> customSerialized = SerializationTest(testObject, Serializer.Serialize);
                TimedResult<string> standardSerialized = SerializationTest(testObject, JsonConvert.SerializeObject);

                TimedResult<object?> customDeserialized = DeserializationTest(
                    customSerialized.Result ?? string.Empty,
                    requiredType,
                    Deserializer.Deserialize);
                TimedResult<object?> standardDeserialized = DeserializationTest(
                    standardSerialized.Result ?? string.Empty,
                    requiredType,
                    JsonConvert.DeserializeObject);

                Console.Write($"{testObject}:");
                Console.Write(GetFormattedOutput(customSerialized, "Custom Serialization"));
                Console.Write(GetFormattedOutput(standardSerialized, "Standard Serialization"));
                Console.Write(GetFormattedOutput(customDeserialized, "Custom Deserialization"));
                Console.Write(GetFormattedOutput(standardDeserialized, "Standard Deserialization"));
                Console.WriteLine("\n");
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Run several serialization attempts for the provided object.<br/>
        /// Returns the average time for the serializer across all attempts.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="serializer">Serializer to use.</param>
        /// <typeparam name="T">The type of object for serialization.</typeparam>
        private static TimedResult<string> SerializationTest<T>(
            T? obj, Func<T?, string> serializer)
        {
            return RunStopWatched.RunFunction(
                () => serializer(obj), Attempts);
        }

        /// <summary>
        /// Run several deserialization attempts for the provided serialization string.<br/>
        /// Returns the average time for the deserializer across all attempts.
        /// </summary>
        /// <param name="serialized">String to deserialize.</param>
        /// <param name="objType">Type of the deserialized object.</param>
        /// <param name="deserializer">Deserializer to use.</param>
        private static TimedResult<object?> DeserializationTest(
            string serialized, Type objType, Func<string, Type, object?> deserializer)
        {
            return RunStopWatched.RunFunction(
                () => deserializer(serialized, objType), Attempts);
        }

        private static string GetFormattedOutput<T>(TimedResult<T> timedResult, string timedResultName)
        {
            return $"\n\t{timedResultName}: {timedResult.Result}" +
                   $"\n\t{timedResultName} Time (avg in {Attempts}): {timedResult.Time} ms";
        }

        #endregion
    }
}
