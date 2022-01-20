using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class Transaction
    {
        private readonly IAuthoredEventStore authoredEventStore;
        private readonly IInMemoryAuthoredEventStore inMemoryAuthoredEventStore;
        private readonly Guid aggregateId;
        private readonly IEnumerable<IEvent> initialAggregateEvents;
        private readonly List<IEvent> allAggregateEvents;
        private int expectedVersion;

        private Transaction(IAuthoredEventStore authoredEventStore, IInMemoryAuthoredEventStore inMemoryAuthoredEventStore, Guid aggregateId, IEnumerable<IEvent> initialAggregateEvents, int expectedVersion)
        {
            this.authoredEventStore = authoredEventStore;
            this.inMemoryAuthoredEventStore = inMemoryAuthoredEventStore;
            this.aggregateId = aggregateId;
            this.initialAggregateEvents = initialAggregateEvents;
            this.allAggregateEvents = new List<IEvent>();
            this.expectedVersion = expectedVersion;
        }

        public static async Task<Transaction> Begin(IAuthoredEventStore authoredEventStore, IInMemoryAuthoredEventStore inMemoryAuthoredEventStore, Guid aggregateId, Guid authorId, int expectedVersion = 0)
        {
            authoredEventStore.SetAuthor(authorId);
            var initialEvents = new List<IEvent>(await authoredEventStore.Get(aggregateId, -1));
            inMemoryAuthoredEventStore.Initialize(initialEvents);
            return new Transaction(authoredEventStore, inMemoryAuthoredEventStore, aggregateId, initialEvents, expectedVersion);
        }

        public async Task InvokeAdapter(Assembly assemblyContainingRecognizedEvents, Func<int, Task> adapterMethod, IEnumerable<IEvent> preloadedOtherAggregatesEvents = null)
        {
            await Transaction.Update(this.allAggregateEvents, this.inMemoryAuthoredEventStore, this.aggregateId);
            var processedEvents = Transaction.ReplaceUnrecognizedEvents(this.allAggregateEvents, assemblyContainingRecognizedEvents);
            if (preloadedOtherAggregatesEvents != null)
                processedEvents = processedEvents.Concat(preloadedOtherAggregatesEvents);
            this.inMemoryAuthoredEventStore.Initialize(processedEvents);

            await adapterMethod.Invoke(this.expectedVersion);
            this.expectedVersion++;
        }

        public static IEnumerable<IEvent> ReplaceUnrecognizedEvents(IEnumerable<IEvent> events, Assembly assemblyContainingRecognizedEvents)
        {
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

        private static async Task Update(List<IEvent> allAggregateEvents, IInMemoryAuthoredEventStore inMemoryAuthoredEventStore, Guid aggregateId)
        {
            // update cache if there are more events in eventStore than in cache
            var aggregateEventsInEventStore = await inMemoryAuthoredEventStore.Get(aggregateId, -1);
            if (aggregateEventsInEventStore.Count() > allAggregateEvents.Count)
                allAggregateEvents.AddRange(aggregateEventsInEventStore.Skip(allAggregateEvents.Count));
        }

        public async Task Commit()
        {
            await Transaction.Update(this.allAggregateEvents, this.inMemoryAuthoredEventStore, this.aggregateId);
            var newEvents = this.allAggregateEvents.Except(this.initialAggregateEvents);
            await this.authoredEventStore.Save(newEvents);
        }
    }
}
