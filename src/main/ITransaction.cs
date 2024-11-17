using CQRSlite.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ei8.EventSourcing.Client
{
    /// <summary>
    /// Represents a Transaction to be used by aggregates that use event sourcing.
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Begins a transaction.
        /// </summary>
        /// <param name="authorId"></param>
        /// <returns></returns>
        Task BeginAsync(Guid authorId);

        /// <summary>
        /// Invokes an adapter method.
        /// </summary>
        /// <param name="aggregateId"></param>
        /// <param name="recognizedEventTypes"></param>
        /// <param name="adapterMethod"></param>
        /// <param name="expectedVersion"></param>
        /// <returns></returns>
        Task<int> InvokeAdapterAsync(Guid aggregateId, IEnumerable<Type> recognizedEventTypes, Func<int, Task> adapterMethod, int expectedVersion = 0);

        /// <summary>
        /// Commits a transaction.
        /// </summary>
        /// <returns></returns>
        Task CommitAsync();
    }
}
