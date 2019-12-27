using System;
using System.Collections.Generic;
using System.Text;

namespace works.ei8.EventSourcing.Client
{
    public interface IEventSourceFactory
    {
        IEventSource Create(string inStoreUrl, string outStoreUrl, Guid authorId);
    }
}
