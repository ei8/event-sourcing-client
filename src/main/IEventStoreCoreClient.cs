using ei8.EventSourcing.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public interface IEventStoreCoreClient
    {
        Task<IEnumerable<Notification>> Get(string outBaseUrl, Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default);

        Task Save(string inBaseUrl, IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    }
}
