using Newtonsoft.Json;
using System;

namespace Wki.EventSourcing.Util
{
    /// <summary>
    /// Universal serializer build for easy replacement
    /// </summary>
    public static class Serializer
    {
        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.None,
        };

        public static string Serialize(object o) =>
            JsonConvert.SerializeObject(o, JsonSettings);

        public static T Deserialize<T>(string s) =>
            (T) Deserialize(s, typeof(T));

        public static object Deserialize(string s, Type type) =>
            JsonConvert.DeserializeObject(s, type, JsonSettings);
    }
}
