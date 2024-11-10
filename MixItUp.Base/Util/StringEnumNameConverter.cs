using Newtonsoft.Json;
using System;

namespace MixItUp.Base.Util
{
    /// <summary>
    /// This converts from Enum value to name and back
    /// </summary>
    public class StringEnumNameConverter : JsonConverter
    {
        /// <summary>
        /// Checks to see if this converter can work on the requested type.
        /// </summary>
        /// <param name="objectType">The type to convert to.</param>
        /// <returns>True if it can convert.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        /// <summary>
        /// Reads the raw json to parse to enum
        /// </summary>
        /// <param name="reader">The Json reader to read from.</param>
        /// <param name="objectType">The type being converted to.</param>
        /// <param name="existingValue">The current value.</param>
        /// <param name="serializer">The serializer being used.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var enumText = reader.Value?.ToString();
                return EnumHelper.GetEnumValueFromString(objectType, enumText);
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing enum.");
        }

        /// <summary>
        /// Write out the enum as a string.
        /// </summary>
        /// <param name="writer">The writer being used.</param>
        /// <param name="value">The enum value.</param>
        /// <param name="serializer">The serializer being used.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;
            var name = EnumHelper.GetEnumName(value.GetType(), e);
            writer.WriteValue(name);
        }
    }
}
