using CQRSlite.Events;
using neurUL.Common.CqrsLite;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class AggregateEventCatalog
    {
        private readonly IEnumerable<IEvent> initial;
        private readonly IInMemoryAuthoredEventStore eventStore;
        private readonly List<IEvent> all;

        private AggregateEventCatalog(Guid aggregateId, IEnumerable<IEvent> initial, IInMemoryAuthoredEventStore eventStore)
        {
            this.AggregateId = aggregateId;
            this.initial = initial;
            this.eventStore = eventStore;
            this.all = new List<IEvent>();
        }

        public static async Task<AggregateEventCatalog> CreateAsync(Guid aggregateId, IEnumerable<IEvent> initial, IInMemoryAuthoredEventStore eventStore)
        {
            AssertionConcern.AssertArgumentValid(ai => ai != Guid.Empty, aggregateId, $"Specified '{nameof(aggregateId)}' should not be equal to '{Guid.Empty}'", nameof(aggregateId));
            AssertionConcern.AssertArgumentNotNull(initial, nameof(initial));
            AssertionConcern.AssertArgumentNotNull(eventStore, nameof(eventStore));

            var result = new AggregateEventCatalog(aggregateId, initial, eventStore);
            await result.UpdateCore(result.initial);
            return result;
        }

        public async Task Update(IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion)
        {
            AssertionConcern.AssertArgumentNotNull(recognizedEventTypes, nameof(recognizedEventTypes));
            AssertionConcern.AssertArgumentNotNull(adapterMethod, nameof(adapterMethod));

            // replace unrecognized events
            var filteredEvents = this.all.Select(
                e => recognizedEventTypes.Contains(e.GetType()) ?
                e :
                new UnrecognizedEvent()
                {
                    Id = e.Id,
                    Version = e.Version,
                    TimeStamp = e.TimeStamp
                }
                );
            
            await this.UpdateCore(filteredEvents, adapterMethod, expectedVersion);
        }

        private async Task UpdateCore(IEnumerable<IEvent> initialEvents, Func<int, Task> adapterMethod = null, int expectedVersion = 0)
        {
            this.eventStore.Initialize(initialEvents);

            if (adapterMethod != null)
                await adapterMethod.Invoke(expectedVersion);

            // update cache if there are more events in eventStore than in cache
            var aggregateEventsInInMemoryEventStore = await this.eventStore.Get(this.AggregateId, -1);
            if (aggregateEventsInInMemoryEventStore.Count() > this.all.Count)
                this.all.AddRange(aggregateEventsInInMemoryEventStore.Skip(this.all.Count));
        }

        public Guid AggregateId { get; private set; }

        public IEnumerable<IEvent> New => this.all.Except(this.initial);
    }
}
