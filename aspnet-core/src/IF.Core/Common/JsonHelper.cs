using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Common
{
    public static class JsonHelper
    {
        public static T ConvertToObject<T>(object value)
        {
            return DeserializeObject<T>(SerializeObject(value));
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        public static string SerializeObjectWithCamelCasePropertyNames(object value)
        {
            JsonSerializerSettings settings1 = new JsonSerializerSettings();
            settings1.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(value, Formatting.Indented, settings1);
        }
    }
}
