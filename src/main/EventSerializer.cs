using CQRSlite.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using works.ei8.EventSourcing.Client.In;

namespace works.ei8.EventSourcing.Client
{
    public class EventSerializer : IEventSerializer
    {
        public IEvent Deserialize(string typeName, string eventData)
        {
            var result = default(IEvent);

            var eventType = default(Type);
            eventType = Type.GetType(typeName, false);

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
