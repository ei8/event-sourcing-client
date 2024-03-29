﻿using CQRSlite.Events;
using ei8.EventSourcing.Common;
using neurUL.Common.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public class HttpEventStoreClient : IAuthoredEventStore
    {
        private static readonly Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();
        private static readonly string eventStorePathTemplate = "{0}eventsourcing/eventstore{1}";

        private readonly IEventStoreUrlService eventStoreUrls;
        private readonly IEventSerializer serializer;
        private Guid authorId;

        public HttpEventStoreClient(IEventStoreUrlService eventStoreUrls, IEventSerializer serializer)
        {
            AssertionConcern.AssertArgumentNotNull(eventStoreUrls, nameof(eventStoreUrls));
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));

            this.eventStoreUrls = eventStoreUrls;
            this.serializer = serializer;
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
            var response = await HttpEventStoreClient.GetCreateClient(this.eventStoreUrls.OutBaseUrl).GetAsync(
                string.Format(HttpEventStoreClient.eventStorePathTemplate, this.eventStoreUrls.OutBaseUrl, "/" + aggregateId.ToString())
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
                var notifications = events.Select(e => ((IEvent)e).ToNotification(this.serializer, this.authorId));

                var content = new StringContent(JsonConvert.SerializeObject(notifications));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await HttpEventStoreClient.GetCreateClient(this.eventStoreUrls.InBaseUrl).PostAsync(
                    string.Format(HttpEventStoreClient.eventStorePathTemplate, this.eventStoreUrls.InBaseUrl, string.Empty),
                    content
                    );

                response.EnsureSuccessStatusCode();
            }
        }

        public void SetAuthor(Guid authorId)
        {
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, authorId, "Id must not be equal to '00000000-0000-0000-0000-000000000000'.", nameof(authorId));

            this.authorId = authorId;
        }
    }
}
