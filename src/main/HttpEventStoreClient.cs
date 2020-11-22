using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class HttpEventStoreClient : IEventStore
    {
        private string inBaseUrl;
        private string outBaseUrl;
        private IEventStoreCoreClient eventStoreCoreClient;
        private IEventSerializer serializer;
        private Guid authorId;

        public HttpEventStoreClient(string inBaseUrl, string outBaseUrl, IEventStoreCoreClient eventStoreCoreClient, IEventSerializer serializer, Guid authorId)
        {
            AssertionConcern.AssertArgumentNotNull(inBaseUrl, nameof(inBaseUrl));
            AssertionConcern.AssertArgumentNotEmpty(inBaseUrl, $"'{nameof(inBaseUrl)}' cannot be empty.", nameof(inBaseUrl));
            AssertionConcern.AssertArgumentNotNull(outBaseUrl, nameof(outBaseUrl));
            AssertionConcern.AssertArgumentNotEmpty(outBaseUrl, $"'{nameof(outBaseUrl)}' cannot be empty.", nameof(outBaseUrl));
            AssertionConcern.AssertArgumentNotNull(eventStoreCoreClient, nameof(eventStoreCoreClient));
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, authorId, "Id must not be equal to '00000000-0000-0000-0000-000000000000'.", nameof(authorId));

            this.inBaseUrl = inBaseUrl;
            this.outBaseUrl = outBaseUrl;
            this.eventStoreCoreClient = eventStoreCoreClient;
            this.serializer = serializer;
            this.authorId = authorId;
        }
        
        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            var notifications = await this.eventStoreCoreClient.Get(this.outBaseUrl, aggregateId, fromVersion, cancellationToken);
            string id = aggregateId.ToString();
            var list = notifications.Where(e => e.Id == id && e.Version > fromVersion);

            return list.Select(ev => ev.ToDomainEvent(this.serializer)).ToArray();
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertionConcern.AssertArgumentNotNull(events, nameof(events));
            if (events.Any())
            {
                var eventData = events.Select(e => ((IEvent)e).ToNotification(this.serializer, this.authorId));

                await this.eventStoreCoreClient.Save(this.inBaseUrl, eventData, cancellationToken);
            }
        }
    }
}
