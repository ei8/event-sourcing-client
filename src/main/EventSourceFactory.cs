using CQRSlite.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using works.ei8.EventSourcing.Client.In;
using works.ei8.EventSourcing.Client.Out;

namespace works.ei8.EventSourcing.Client
{
    public class EventSourceFactory : IEventSourceFactory
    {
        private IEventSerializer serializer;

        public EventSourceFactory(IEventSerializer serializer)
        {
            this.serializer = serializer;
        }

        public IEventSource Create(string inStoreUrl, string outStoreUrl, Guid authorId)
        {
            var es = new HttpEventStoreClient(inStoreUrl, outStoreUrl, this.serializer, authorId);
            var r = new Repository(es);
            var s = new Session(r);
            var nc = new HttpNotificationClient();
            return new EventSource(inStoreUrl, outStoreUrl, s, r, es, nc);
        }
    }
}
