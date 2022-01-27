using System.Text.Json;
using AutoMapper;
using CommandService.Data;
using CommandService.Dtos;
using CommandService.Models;

namespace CommandService.EventProcessing
{
    internal enum EventType
    {
        PlatformPublished,
        Undetermined,
    }

    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);
            switch (eventType)
            {
                case EventType.PlatformPublished:
                    AddPlatform(message);
                    break;

                case EventType.Undetermined:
                    break;

                default:
                    break;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);
                try
                {
                    var plat = _mapper.Map<Platform>(platformPublishedDto);
                    if (!repo.ExternalPlatformExist(plat.ExternalId))
                    {
                        repo.CreatePlatform(plat);
                        repo.SaveChanges();
                        Console.WriteLine($"--> Platform Added!");
                    }
                    else
                    {
                        Console.WriteLine($"--> Platform already exisits...");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"--> Could not add platform to DB {e.Message}");
                }
            }
        }

        private EventType DetermineEvent(string notifcationMessage)
        {
            Console.WriteLine($"--> Determining Event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notifcationMessage);
            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine($"Platform Published Event Detected.");
                    return EventType.PlatformPublished;

                default:
                    Console.WriteLine($"--> Could not determine the event type");
                    return EventType.Undetermined;
            }
        }
    }
}