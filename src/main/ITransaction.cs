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
        Task BeginAsync(Guid aggregateId, Guid authorId);

        Task BeginAsync(IEnumerable<Guid> aggregateIds, Guid authorId);

        Task<int> InvokeAdapterAsync(Guid aggregateId, IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion = 0);

        Task CommitAsync();
    }
}
