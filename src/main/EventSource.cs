using CQRSlite.Domain;
using CQRSlite.Events;
using works.ei8.EventSourcing.Client.Out;

namespace works.ei8.EventSourcing.Client
{
    public class EventSource : IEventSource
    {
        public EventSource(string inStoreUrl, string outStoreUrl, ISession session, IRepository repository, IEventStore eventStoreClient, INotificationClient notificationClient)
        {
            this.InStoreUrl = inStoreUrl;
            this.OutStoreUrl = outStoreUrl;
            this.Session = session;
            this.Repository = repository;
            this.EventStoreClient = eventStoreClient;
            this.NotificationClient = notificationClient;
        }

        public string InStoreUrl { get; private set; }

        public string OutStoreUrl { get; private set; }

        public ISession Session { get; private set; }

        public IRepository Repository { get; private set; }

        public IEventStore EventStoreClient { get; private set; }

        public INotificationClient NotificationClient { get; private set; }
    }
}
