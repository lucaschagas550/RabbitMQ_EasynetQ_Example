using MessageBus.Messages.Base;

namespace MessageBus.Messages.Integration
{
    public class PersonIntegration : IntegrationEvent
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public PersonIntegration(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}
