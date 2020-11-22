using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using ei8.EventSourcing.Client.In;
using ei8.EventSourcing.Client.Out;

namespace ei8.EventSourcing.Client
{
    public class EventSourceFactory : IEventSourceFactory
    {
        private IEventSerializer serializer;

        public EventSourceFactory(IEventSerializer serializer)
        {
            this.serializer = serializer;
        }

        public IEventSource Create(string inBaseUrl, string outBaseUrl, Guid authorId)
        {
            var esc = new HttpEventStoreCoreClient();
            var es = new HttpEventStoreClient(inBaseUrl, outBaseUrl, esc, this.serializer, authorId);
            var r = new Repository(es);
            var s = new Session(r);
            var nc = new HttpNotificationClient();
            return new EventSource(inBaseUrl, outBaseUrl, s, r, es, nc);
        }
    }
}
