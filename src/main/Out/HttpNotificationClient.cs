using Newtonsoft.Json;
using neurUL.Common;
using neurUL.Common.Constants;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ei8.EventSourcing.Common;

namespace ei8.EventSourcing.Client.Out
{
    public class HttpNotificationClient : INotificationClient
    {
        private static string getEventsPathTemplate = "{0}eventsourcing/notifications/{1}";
        private static readonly Dictionary<string, HttpClient> clients = new Dictionary<string, HttpClient>();
        
        public HttpNotificationClient()
        {
        }

        private static HttpClient GetCreateClient(string url)
        {
            Uri uri = null;
            AssertionConcern.AssertArgumentValid<string>(u => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri), url, "Specified URL is not valid.", nameof(url));
            var baseUrl = uri.GetLeftPart(UriPartial.Authority);
            if (!HttpNotificationClient.clients.ContainsKey(baseUrl))
                HttpNotificationClient.clients.Add(baseUrl, new HttpClient() { BaseAddress = new Uri(baseUrl) });
            return HttpNotificationClient.clients[baseUrl];
        }

        public async Task<NotificationLog> GetNotificationLog(string storeUrl, string notificationLogId, CancellationToken token = default(CancellationToken))
        {
            var response = await HttpNotificationClient.GetCreateClient(storeUrl).GetAsync(
                string.Format(HttpNotificationClient.getEventsPathTemplate, storeUrl, notificationLogId)
                ).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var eventInfoItems = JsonConvert.DeserializeObject<Notification[]>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                );
            var linkHeader = response.Headers.GetValues(Response.Header.Link.Key).First();

            AssertionConcern.AssertStateTrue(linkHeader != null, "'Link' header is missing in server response.");

            NotificationLogId.TryParse(
                HttpNotificationClient.GetLogId(linkHeader, Response.Header.Link.Relation.Self),
                out NotificationLogId selfLogId
                );
            NotificationLogId.TryParse(
                HttpNotificationClient.GetLogId(linkHeader, Response.Header.Link.Relation.First),
                out NotificationLogId firstLogId
                );
            NotificationLogId.TryParse(
                HttpNotificationClient.GetLogId(linkHeader, Response.Header.Link.Relation.Next),
                out NotificationLogId nextLogId
                );
            NotificationLogId.TryParse(
                HttpNotificationClient.GetLogId(linkHeader, Response.Header.Link.Relation.Previous),
                out NotificationLogId previousLogId
                );
            return new NotificationLog(
                selfLogId,
                firstLogId,
                nextLogId,
                previousLogId,
                eventInfoItems,
                nextLogId != null,
                int.Parse(response.Headers.GetValues(Response.Header.TotalCount.Key).First())
                );
        }
        
        private static string GetLogId(string linkHeader, Response.Header.Link.Relation relation)
        {
            string result = string.Empty;
            if (ResponseHelper.Header.Link.TryGet(linkHeader, relation, out string link))
            {
                link = link.TrimEnd('/');
                result = link.Substring(link.LastIndexOf('/') + 1);
            }
            return result;
        }
    }
}