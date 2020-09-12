using ei8.EventSourcing.Common;
using neurUL.Common.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class HttpEventStoreCoreClient : IEventStoreCoreClient
    {
        private static readonly Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();
        private static readonly string eventStorePathTemplate = "{0}eventsourcing/eventstore{1}";

        public HttpEventStoreCoreClient()
        { 
        }

        private static HttpClient GetCreateClient(string url)
        {
            Uri uri = null;
            AssertionConcern.AssertArgumentValid<string>(u => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri), url, "Specified URL must be valid", nameof(url));
            var baseUrl = uri.GetLeftPart(UriPartial.Authority);
            if (!HttpEventStoreCoreClient.clients.ContainsKey(baseUrl))
                HttpEventStoreCoreClient.clients.Add(baseUrl, new HttpClient() { BaseAddress = new Uri(baseUrl) });
            return HttpEventStoreCoreClient.clients[baseUrl];
        }

        public async Task<IEnumerable<Notification>> Get(string outBaseUrl, Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
        {
            var response = await HttpEventStoreCoreClient.GetCreateClient(outBaseUrl).GetAsync(
                string.Format(HttpEventStoreCoreClient.eventStorePathTemplate, outBaseUrl, "/" + aggregateId.ToString())
                );

            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<Notification[]>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                );
        }

        public async Task Save(string inBaseUrl, IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
        {
            var content = new StringContent(JsonConvert.SerializeObject(notifications));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await HttpEventStoreCoreClient.GetCreateClient(inBaseUrl).PostAsync(
                string.Format(HttpEventStoreCoreClient.eventStorePathTemplate, inBaseUrl, string.Empty),
                content
                );

            response.EnsureSuccessStatusCode();
        }
    }
}
