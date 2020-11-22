using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.EventSourcing.Client
{
    public interface IEventSourceFactory
    {
        IEventSource Create(string inBaseUrl, string outBaseUrl, Guid authorId);
    }
}
