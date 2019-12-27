using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using works.ei8.EventSourcing.Common;

namespace works.ei8.EventSourcing.Client.Out
{
    public interface INotificationClient
    {
        Task<NotificationLog> GetNotificationLog(string notificationLogId, CancellationToken token = default(CancellationToken));
    }
}
