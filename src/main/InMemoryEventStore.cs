using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class InMemoryEventStore : IInMemoryAuthoredEventStore
    {
        private List<IEvent> events;
        
        public InMemoryEventStore()
        {            
        }

        public Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default) =>
            Task.FromResult((IEnumerable<IEvent>)this.events.ToArray().Where(e => e.Id == aggregateId && e.Version > fromVersion));

        public void Initialize(IEnumerable<IEvent> initialEvents)
        {
            this.events = new List<IEvent>(initialEvents);
        }

        public Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            this.events.AddRange(events);
            return Task.CompletedTask;
        }

        public void SetAuthor(Guid authorId){}
    }
}
