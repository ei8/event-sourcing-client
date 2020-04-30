using CQRSlite.Events;
using neurUL.Common.Domain.Model;
using System;
using System.Text.RegularExpressions;
using ei8.EventSourcing.Client.In;
using ei8.EventSourcing.Common;

namespace ei8.EventSourcing.Client
{
    public static class EventExtensions
    {
        public static Notification ToNotification(this IEvent @event, IEventSerializer serializer, Guid authorId)
        {
            var contentJson = serializer.Serialize(@event);

            if (string.IsNullOrEmpty(contentJson))
                throw new InvalidOperationException("Failed deserializing event.");

            return new Notification()
            {
                Id = @event.Id.ToString(),
                Data = contentJson,
                TypeName = @event.GetType().AssemblyQualifiedName,
                Timestamp = DateTimeOffset.Now.ToString("o"),
                Version = @event.Version,
                AuthorId = authorId.ToString()
            };
        }
                
        public static string GetEventName(this Notification @event)
        {
            var m = Regex.Match(
                @event.TypeName,
                Event.TypeName.Regex.Pattern,
                RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace
                );

            return m.Success ? m.Groups[Event.TypeName.Regex.CaptureName.EventName].Value : null;
        }

        public static IEvent ToDomainEvent(this Notification @event, IEventSerializer serializer)
        {
            return serializer.Deserialize(@event.TypeName, @event.Data) ??
                new UnrecognizedEvent()
                {
                    TypeName = @event.TypeName,
                    Data = @event.Data,
                    Id = Guid.Parse(@event.Id),
                    Version = @event.Version,
                    TimeStamp = DateTimeOffset.Parse(@event.Timestamp)
                };
        }
    }
}
