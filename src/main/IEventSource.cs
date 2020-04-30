using CQRSlite.Domain;
using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Text;
using ei8.EventSourcing.Client.Out;

namespace ei8.EventSourcing.Client
{
    public interface IEventSource
    {
        string InStoreUrl { get; }

        string OutStoreUrl { get; }

        ISession Session { get; }

        IRepository Repository { get; }

        IEventStore EventStoreClient { get; }

        INotificationClient NotificationClient { get; }
    }
}
