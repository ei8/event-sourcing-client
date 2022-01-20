using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.EventSourcing.Client
{
    public class EventStoreUrlService : IEventStoreUrlService
    {
        private readonly string inBaseUrl;
        private readonly string outBaseUrl;

        public EventStoreUrlService(string inBaseUrl, string outBaseUrl)
        {
            AssertionConcern.AssertArgumentNotNull(inBaseUrl, nameof(inBaseUrl));
            AssertionConcern.AssertArgumentNotEmpty(inBaseUrl, $"'{nameof(inBaseUrl)}' cannot be empty.", nameof(inBaseUrl));
            AssertionConcern.AssertArgumentNotNull(outBaseUrl, nameof(outBaseUrl));
            AssertionConcern.AssertArgumentNotEmpty(outBaseUrl, $"'{nameof(outBaseUrl)}' cannot be empty.", nameof(outBaseUrl));

            this.inBaseUrl = inBaseUrl;
            this.outBaseUrl = outBaseUrl;
        }

        public string InBaseUrl => this.inBaseUrl;

        public string OutBaseUrl => this.outBaseUrl;
    }
}
