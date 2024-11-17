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
        private readonly Dictionary<Guid, AggregateEventCatalog> aggregateEventCatalogs;
        private bool begun;
        
        public Transaction(IAuthoredEventStore eventStore, IInMemoryAuthoredEventStore inMemoryEventStore)
        {
            this.eventStore = eventStore;
            this.inMemoryEventStore = inMemoryEventStore;
            this.aggregateEventCatalogs = new Dictionary<Guid, AggregateEventCatalog>();
            this.begun = false;
        }

        public async Task BeginAsync(Guid authorId)
        {
            AssertionConcern.AssertArgumentValid(ai => ai != Guid.Empty, authorId, $"Specified Guid value cannot be equal to '{Guid.Empty.ToString()}'", nameof(authorId));
            AssertionConcern.AssertStateFalse(this.begun, "Unable to 'Begin' transaction when it has already begun since last 'Commit.'");

            this.begun = true;
            this.eventStore.SetAuthor(authorId);

            this.aggregateEventCatalogs.Clear();
        }
        
        public async Task<int> InvokeAdapterAsync(Guid aggregateId, IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion = 0)
        {
            AssertionConcern.AssertArgumentValid(ai => ai != Guid.Empty, aggregateId, $"Specified aggregateId should not be equal to '{Guid.Empty}'", nameof(aggregateId));
            AssertionConcern.AssertArgumentNotNull(recognizedEventTypes, nameof(recognizedEventTypes));
            AssertionConcern.AssertArgumentNotNull(adapterMethod, nameof(adapterMethod));
            AssertionConcern.AssertStateTrue(this.begun, "Unable to invoke adapter while transaction has not yet begun since last 'Commit.'");            
            
            if (!this.aggregateEventCatalogs.ContainsKey(aggregateId))
            {
                var aec = await AggregateEventCatalog.CreateAsync(aggregateId, await this.eventStore.Get(aggregateId, -1), this.inMemoryEventStore);
                this.aggregateEventCatalogs.Add(aec.AggregateId, aec);
            }

            await this.aggregateEventCatalogs[aggregateId].Update(recognizedEventTypes, adapterMethod, expectedVersion);

            return ++expectedVersion;
        }

        public async Task CommitAsync()
        {
            AssertionConcern.AssertStateTrue(this.begun, "Unable to 'Commit' while transaction has not yet begun since last 'Commit.'");

            var newEvents = this.aggregateEventCatalogs.SelectMany(aec => aec.Value.New)
                .ToList()
                .OrderBy(e => e.TimeStamp);
            
            await this.eventStore.Save(newEvents);

            this.begun = false;
        }
    }
}
