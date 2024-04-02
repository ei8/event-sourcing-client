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

        public async Task BeginAsync(Guid aggregateId, Guid authorId) => await this.BeginAsync(new Guid[] { aggregateId }, authorId);

        public async Task BeginAsync(IEnumerable<Guid> aggregateIds, Guid authorId)
        {
            AssertionConcern.AssertArgumentNotNull(aggregateIds, nameof(aggregateIds));
            AssertionConcern.AssertArgumentValid(ais => !ais.Any(a => a == Guid.Empty), aggregateIds, $"None of the specified Guid values should be equal to '{Guid.Empty.ToString()}'", nameof(aggregateIds));
            AssertionConcern.AssertArgumentValid(ai => ai != Guid.Empty, authorId, $"Specified Guid value cannot be equal to '{Guid.Empty.ToString()}'", nameof(authorId));
            AssertionConcern.AssertStateFalse(this.begun, "Unable to 'Begin' transaction when it has already begun since last 'Commit.'");

            this.begun = true;
            this.eventStore.SetAuthor(authorId);

            this.aggregateEventCatalogs.Clear();
            foreach (var ai in aggregateIds)
            {
                var aec = await AggregateEventCatalog.CreateAsync(ai, await this.eventStore.Get(ai, -1), this.inMemoryEventStore);
                this.aggregateEventCatalogs.Add(aec.AggregateId, aec);                
            }            
        }
        
        public async Task<int> InvokeAdapterAsync(Guid aggregateId, IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion = 0)
        {
            AssertionConcern.AssertStateTrue(this.begun, "Unable to invoke adapter while transaction has not yet begun since last 'Commit.'");            
            
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
