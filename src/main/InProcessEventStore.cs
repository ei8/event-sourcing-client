using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using IInEventAdapter = ei8.EventSourcing.Port.Adapter.In.InProcess.IEventAdapter;
using IOutEventAdapter = ei8.EventSourcing.Port.Adapter.Out.InProcess.IEventAdapter;

namespace ei8.EventSourcing.Client
{
    public class InProcessEventStore : IAuthoredEventStore
    {
        private readonly IInEventAdapter inEventAdapter;
        private readonly IOutEventAdapter outEventAdapter;
        private readonly IEventSerializer serializer;

        private Guid authorId;

        public InProcessEventStore(
            IInEventAdapter inEventAdapter,
            IOutEventAdapter outEventAdapter,
            IEventSerializer serializer
        )
        {
            AssertionConcern.AssertArgumentNotNull(inEventAdapter, nameof(inEventAdapter));
            AssertionConcern.AssertArgumentNotNull(outEventAdapter, nameof(outEventAdapter));
            AssertionConcern.AssertArgumentNotNull(serializer, nameof(serializer));

            this.inEventAdapter = inEventAdapter;
            this.outEventAdapter = outEventAdapter;
            this.serializer = serializer;
        }

        #region IDisposable
        private bool isDisposed;

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
            }

            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero)
            //{
            //    Marshal.FreeHGlobal(nativeResource);
            //    nativeResource = IntPtr.Zero;
            //}

            isDisposed = true;
        }
        #endregion

        public async Task<IEnumerable<IEvent>> Get(Guid aggregateId, int fromVersion, CancellationToken cancellationToken = default)
        {
            return (await this.outEventAdapter.Get(aggregateId, fromVersion, cancellationToken))
                .Select(ev => ev.ToDomainEvent(this.serializer));
        }

        public async Task Save(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
        {
            AssertionConcern.AssertArgumentNotNull(events, nameof(events));

            if (events.Any())
            {
                var notifications = events.Select(e => ((IEvent)e).ToNotification(this.serializer, this.authorId));
                await this.inEventAdapter.Save(notifications, cancellationToken);
            }
        }

        public void SetAuthor(Guid authorId)
        {
            AssertionConcern.AssertArgumentValid(i => i != Guid.Empty, authorId, $"Id must not be equal to '{Guid.Empty}'.", nameof(authorId));

            this.authorId = authorId;
        }
    }
}
