using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ei8.EventSourcing.Common;

namespace ei8.EventSourcing.Client.Out
{
    public interface INotificationClient
    {
        Task<NotificationLog> GetNotificationLog(string storeUrl, string notificationLogId, CancellationToken token = default(CancellationToken));
    }
}
