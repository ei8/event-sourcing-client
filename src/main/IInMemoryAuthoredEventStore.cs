using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.EventSourcing.Client
{
    public interface IInMemoryAuthoredEventStore : IAuthoredEventStore
    {
        void Initialize(IEnumerable<IEvent> initialEvents);
    }
}
