using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    public interface ITransaction
    {
        Task Begin(Guid aggregateId, Guid authorId);

        Task<int> InvokeAdapter(Assembly assemblyContainingRecognizedEvents, Func<int, Task> adapterMethod, int expectedVersion, IEnumerable<IEvent> preloadedOtherAggregatesEvents = null);

        Task Commit();
    }
}
