using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ComPact.JsonConverters
{
    public class StringEnumWithDefaultConverter: StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!Enum.TryParse(objectType, reader.Value.ToString(), out var _))
            {
                return existingValue;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
