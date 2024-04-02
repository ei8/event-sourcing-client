using CQRSlite.Events;
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
            var result = new AggregateEventCatalog(aggregateId, initial, eventStore);
            await result.Initialize();
            return result;
        }

        private async Task Initialize()
        {
            this.eventStore.Initialize(this.initial);
            await AggregateEventCatalog.UpdateEvents(this.eventStore, this);
        }

        public async Task Update(IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion)
        {
            AssertionConcern.AssertArgumentNotNull(recognizedEventTypes, nameof(recognizedEventTypes));
            AssertionConcern.AssertArgumentNotNull(adapterMethod, nameof(adapterMethod));
            AssertionConcern.AssertArgumentNotNull(expectedVersion, nameof(expectedVersion));

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
            this.eventStore.Initialize(filteredEvents);

            await adapterMethod.Invoke(expectedVersion);
            await AggregateEventCatalog.UpdateEvents(this.eventStore, this);
        }

        private async static Task UpdateEvents(IInMemoryAuthoredEventStore eventStore, AggregateEventCatalog catalog)
        {
            // update cache if there are more events in eventStore than in cache
            var aggregateEventsInInMemoryEventStore = await eventStore.Get(catalog.AggregateId, -1);
            if (aggregateEventsInInMemoryEventStore.Count() > catalog.all.Count)
                catalog.all.AddRange(aggregateEventsInInMemoryEventStore.Skip(catalog.all.Count));
        }

        public Guid AggregateId { get; private set; }

        public IEnumerable<IEvent> New => this.all.Except(this.initial);
    }
}
