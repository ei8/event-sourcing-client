using CQRSlite.Events;
using System;

namespace ei8.EventSourcing.Client
{
    public interface IAuthoredEventStore : IEventStore
    {
        void SetAuthor(Guid authorId);
    }
}
