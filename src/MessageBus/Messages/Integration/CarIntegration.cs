using MessageBus.Messages.Base;

namespace MessageBus.Messages.Integration
{
    public class CarIntegration : IntegrationEvent
    {
        public string Name { get; set; }

        public CarIntegration(string name)
        {
            Name = name;
        }
    }
}
