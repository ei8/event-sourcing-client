using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class Transaction : ITransaction
    {
        private readonly IAuthoredEventStore eventStore;
        private readonly IInMemoryAuthoredEventStore inMemoryEventStore;
        private Guid aggregateId;
        private IEnumerable<IEvent> initialAggregateEvents;
        private readonly List<IEvent> allAggregateEvents;

        private Transaction(IAuthoredEventStore eventStore, IInMemoryAuthoredEventStore inMemoryEventStore)
        {
            this.eventStore = eventStore;
            this.inMemoryEventStore = inMemoryEventStore;
            this.allAggregateEvents = new List<IEvent>();
        }

        public async Task Begin(Guid aggregateId, Guid authorId)
        {
            this.eventStore.SetAuthor(authorId);

            this.initialAggregateEvents = new List<IEvent>(await this.eventStore.Get(aggregateId, -1));
            this.inMemoryEventStore.Initialize(this.initialAggregateEvents);
            this.aggregateId = aggregateId;

            await Transaction.Update(this.allAggregateEvents, this.inMemoryEventStore, this.aggregateId);
        }
        
        public async Task<int> InvokeAdapter(Assembly assemblyContainingRecognizedEvents, Func<int, Task> adapterMethod, int expectedVersion, IEnumerable<IEvent> preloadedOtherAggregatesEvents = null)
        {            
            var processedEvents = Transaction.ReplaceUnrecognizedEvents(this.allAggregateEvents, assemblyContainingRecognizedEvents);
            if (preloadedOtherAggregatesEvents != null)
                processedEvents = processedEvents.Concat(preloadedOtherAggregatesEvents);
            this.inMemoryEventStore.Initialize(processedEvents);

            await adapterMethod.Invoke(expectedVersion);
            await Transaction.Update(this.allAggregateEvents, this.inMemoryEventStore, this.aggregateId);

            return ++expectedVersion;
        }

        public static IEnumerable<IEvent> ReplaceUnrecognizedEvents(IEnumerable<IEvent> events, Assembly assemblyContainingRecognizedEvents)
        {
            // get events from assembly
            var recognizedEvents = assemblyContainingRecognizedEvents.GetTypes().Where(t => typeof(IEvent).IsAssignableFrom(t));
            return events.Select(
                e => recognizedEvents.Contains(e.GetType()) ?
                e :
                new UnrecognizedEvent()
                {
                    Id = e.Id,
                    Version = e.Version,
                    TimeStamp = e.TimeStamp
                }
                );
        }

        private static async Task Update(List<IEvent> allAggregateEvents, IInMemoryAuthoredEventStore inMemoryEventStore, Guid aggregateId)
        {
            // update cache if there are more events in eventStore than in cache
            var aggregateEventsInInMemoryEventStore = await inMemoryEventStore.Get(aggregateId, -1);
            if (aggregateEventsInInMemoryEventStore.Count() > allAggregateEvents.Count)
                allAggregateEvents.AddRange(aggregateEventsInInMemoryEventStore.Skip(allAggregateEvents.Count));
        }

        public async Task Commit()
        {            
            var newEvents = this.allAggregateEvents.Except(this.initialAggregateEvents);
            await this.eventStore.Save(newEvents);
        }
    }
}
