using CQRSlite.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ei8.EventSourcing.Client.In;

namespace ei8.EventSourcing.Client
{
    public class EventSerializer : IEventSerializer
    {
        public IEvent Deserialize(string typeName, string eventData)
        {
            var result = default(IEvent);
            Type eventType = Type.GetType(typeName, false);

            if (eventType != null)
                result = (IEvent)JsonConvert.DeserializeObject(eventData, eventType);

            return result;
        }

        public string Serialize(IEvent @event)
        {
            return JsonConvert.SerializeObject(@event);
        }
    }
}
