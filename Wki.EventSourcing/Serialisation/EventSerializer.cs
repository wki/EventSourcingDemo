using System;
using Wki.EventSourcing.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
// using Newtonsoft.Json.Serialization;
using JsonNet.PrivateSettersContractResolvers;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Wki.EventSourcing.Serialisation
{
    public static class EventSerializer
    {
        // holds the constructor-generated serializer settings
        private static readonly JsonSerializerSettings JsonSettingsWithType;
        private static readonly JsonSerializerSettings JsonSettingsWithoutType;

        // if non-empty search thru these assemblies for type resolution
        public static List<Assembly> Assemblies = new List<Assembly>(new [] { typeof(EventSerializer).Assembly });
        private static Dictionary<string, Type> TypeForEvent = new Dictionary<string, Type>();

        static EventSerializer()
        {
            // would work, but DefaultMembersSearchFlags attribute is obsolete
            //var contractResolver = new CamelCasePropertyNamesContractResolver();
            //contractResolver.DefaultMembersSearchFlags |= BindingFlags.NonPublic;

            var contractResolver = new PrivateSetterContractResolver();

            JsonSettingsWithType = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.None,
                ContractResolver = contractResolver, // new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                TypeNameHandling = TypeNameHandling.All
            };

            JsonSettingsWithoutType = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.None,
                ContractResolver = contractResolver, // new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() },
                TypeNameHandling = TypeNameHandling.None
            };
        }

        /// <summary>
        /// convert an event to a properly formatted JSON
        /// </summary>
        /// <returns>The json.</returns>
        /// <param name="event">the event to serialize</param>
        public static string ToJson(Event @event)
        {
            return JsonConvert.SerializeObject(@event, JsonSettingsWithType);
        }

        /// <summary>
        /// deserialize a json string into an event
        /// </summary>
        /// <returns>Event</returns>
        /// <param name="json">Json string.</param>
        public static Event FromJson(string json)
        {
            return (Event)JsonConvert.DeserializeObject(json, JsonSettingsWithType);
        }

        /// <summary>
        /// Serialize to an envelope with additional data
        /// </summary>
        /// <returns>Envelope.</returns>
        /// <param name="event">Event to serialize.</param>
        public static Envelope ToEnvelope(Event @event)
        {
            return new Envelope(
                json: JsonConvert.SerializeObject(@event, JsonSettingsWithoutType),
                type: @event.GetType(),
                id: @event.GetId(),
                occuredOn: @event.OccuredOn
            );
        }

        /// <summary>
        /// Deserialize an event from an envelope
        /// </summary>
        /// <returns>Event</returns>
        /// <param name="envelope">Envelope to deserialize.</param>
        public static Event FromEnvelope(Envelope envelope)
        {
            if (!TypeForEvent.Any())
                ScanTypes();

            if (!TypeForEvent.ContainsKey(envelope.DataType))
                throw new ArgumentException($"Type '{envelope.DataType}' cannot be resolved");

            return (Event)JsonConvert.DeserializeObject(envelope.Json, TypeForEvent[envelope.DataType], JsonSettingsWithoutType);
        }

        // initially scan all assemblies for usable types (inherited from Event)
        private static void ScanTypes()
        {
            var eventType = typeof(Event);

            foreach (var assembly in Assemblies)
            {
                assembly
                    .GetTypes()
                    .Where(t => eventType.IsAssignableFrom(t))
                    .ToList()
                    .ForEach(t => TypeForEvent[t.Name] = t);
            }
        }
    }
}
