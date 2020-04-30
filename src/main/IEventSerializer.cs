using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.EventSourcing.Client
{
    public interface IEventSerializer
    {
        IEvent Deserialize(string typeName, string eventData);

        string Serialize(IEvent @event);
    }
}
