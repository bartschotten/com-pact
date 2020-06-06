using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ComPact.JsonHelpers
{
    public class StringEnumWithDefaultConverter: StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch
            {
                return existingValue;
            }
        }
    }
}
