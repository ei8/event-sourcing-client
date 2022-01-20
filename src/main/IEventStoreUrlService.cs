using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.EventSourcing.Client
{
    public interface IEventStoreUrlService
    {
        string InBaseUrl { get; }
        string OutBaseUrl { get; }
    }
}
