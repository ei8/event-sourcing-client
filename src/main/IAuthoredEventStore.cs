using CQRSlite.Events;
using System;

namespace ei8.EventSourcing.Client
{
    public interface IAuthoredEventStore : IEventStore, IDisposable
    {
        void SetAuthor(Guid authorId);
    }
}
