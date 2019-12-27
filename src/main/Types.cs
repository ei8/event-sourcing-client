using System;
using System.Collections.Generic;
using System.Text;

namespace works.ei8.EventSourcing.Client
{
    public struct Event
    {
        public struct TypeName
        {
            public struct Regex
            {
                public const string Pattern = @"^
(
	[^\x2C\x2E]+
	\x2E
)+
(
	(?<EventName>[^\x2C\x2E]+)
	\x2C
)
.*
\z";
                public struct CaptureName
                {
                    public const string EventName = "EventName";
                }
            }
        }
    }
}
