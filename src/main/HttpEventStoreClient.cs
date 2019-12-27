using CQRSlite.Events;
using Newtonsoft.Json;
using org.neurul.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.EventSourcing.Client.In;
using works.ei8.EventSourcing.Common;

namespace works.ei8.EventSourcing.Client
{
    public class HttpEventStoreClient : IEventStore
    {
        private static readonly string eventStorePathTemplate = "{0}eventsourcing/eventstore{1}";
        private static readonly Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();

        private string inStoreUrl;
        private string outStoreUrl;
        private IEventSerializer serializer;
        private Guid authorId;

        public HttpEventStoreClient(string inStoreUrl, string outStoreUrl, IEventSerializer serializer, Guid authorId)
        {
            AssertionConcern.AssertArgumentNotNull(inStoreUrl, nameof(inStoreUrl));
            AssertionConcern.AssertArgumentNotEmpty(inStoreUrl, $"'{nameof(inStoreUrl)}' cannot be empty.", nameof(inStoreUrl));
            AssertionConcern.AssertArgumentNotNull(outStoreUrl, nameof(outStoreUrl));
            AssertionConcern.AssertArgumentNotEmpty(outStoreUrl, $"'{nameof(outStoreUrl)}' cannot be empty.", nameof(outStoreUrl));
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, authorId, "Id must not be equal to '00000000-0000-0000-0000-000000000000'.", nameof(authorId));

            this.inStoreUrl = inStoreUrl;
            this.outStoreUrl = outStoreUrl;
            this.serializer = serializer;
            this.authorId = authorId;
        }

        private static HttpClient GetCreateClient(string url)
        {
            Uri uri = null;
            AssertionConcern.AssertArgumentValid<string>(u => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri), url, "Specified URL must be valid", nameof(url));
            var baseUrl = uri.GetLeftPart(UriPartial.Authority);
            if (!HttpEventStoreClient.clients.ContainsKey(baseUrl))
                HttpEventStoreClient.clients.Add(baseUrl, new HttpClient() { BaseAddress = new Uri(baseUrl) });
            return HttpEventStoreClient.clients[baseUrl];
        }

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await HttpEventStoreClient.GetCreateClient(this.outStoreUrl).GetAsync(
                string.Format(HttpEventStoreClient.eventStorePathTemplate, outStoreUrl, "/" + aggregateId.ToString())
                );

            response.EnsureSuccessStatusCode();
            var notifications = JsonConvert.DeserializeObject<Notification[]>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                );

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

                var content = new StringContent(JsonConvert.SerializeObject(eventData));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await HttpEventStoreClient.GetCreateClient(this.inStoreUrl).PostAsync(
                    string.Format(HttpEventStoreClient.eventStorePathTemplate, inStoreUrl, string.Empty),
                    content
                    );

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
