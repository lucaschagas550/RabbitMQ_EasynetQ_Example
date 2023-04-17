using EasyNetQ;
using MessageBus.MessageBus;
using MessageBus.Messages.Integration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace RabbitMQ_Integration_Example.Services
{
    public class IntegrationHandler : BackgroundService
    {
        private readonly IMessageBus _bus;
        public int Count;

        public IntegrationHandler(IMessageBus bus)
        {
            _bus = bus;
        }

        //Executa somente quando a aplicacao inicia a primeira vez
        protected override async Task<Task> ExecuteAsync(CancellationToken stoppingToken)
        {
            await SetSubscribers();
            return Task.CompletedTask;
        }

        private async Task SetSubscribers()
        {
            var personIntegrationQueue = await _bus.CreateQueueWithBinding($"{nameof(PersonIntegration)}Exchange", ExchangeKind.topic, $"{nameof(PersonIntegration)}Queue", $"{nameof(PersonIntegration)}Key");
            var carQueue = await _bus.CreateQueueWithBinding($"{nameof(PersonIntegration)}Exchange", ExchangeKind.topic, $"{nameof(CarIntegration)}Queue", $"{nameof(CarIntegration)}Key");

            _bus.AdvancedBus.Consume(personIntegrationQueue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                var message = Encoding.UTF8.GetString(body.ToArray());
                Console.WriteLine("Got message: '{0}'", message);
                Debug.WriteLine($"Mensagem recebida: {message}");
                return Task.CompletedTask;
            }));

            _bus.AdvancedBus.Consume(carQueue, (body, properties, info) => Task.Factory.StartNew(() =>
            {
                var message = Encoding.UTF8.GetString(body.ToArray());
                var car = JsonConvert.DeserializeObject<CarIntegration>(message);
                Console.WriteLine("Car1: '{0}'", message);
                Debug.WriteLine($"Car1: {message}");
                return Task.CompletedTask;
            }));

            //_bus.AdvancedBus.Consume<PersonIntegration>(queue, (body, properties, info) =>
            //{
            //    Debug.WriteLine($"Mensagem recebida: {body}");
            //    return Task.CompletedTask;
            //});

            //_bus.CreateExchangeTopic("PersonExchange", nameof(CarIntegration), nameof(CarIntegration), ExchangeType.fanout);

            //Criar metodo que cria a exchange, cria a queue e retorna a queue para ser consumida
            //_bus.AdvancedBus.Consume(queue, (body, properties, info) =>
            //{
            //    Debug.WriteLine($"Mensagem recebida: {Encoding.UTF8.GetString(body)}");
            //    return Task.CompletedTask;
            //});

            //Chamada Original da lib easynetq
            //Se for chamar outro servico aqui dentro, precisa injetar IServiceprovider e criar um scopo unico, porque MessageBus eh singleton

            _bus.SubscribeAsync<PersonIntegration>("Person", async request =>
            {
                Count++;
                if (Count == 1000)
                {
                    Debug.WriteLine($"{DateTime.Now}:{request.Age} {request.Name} => {Count}");
                    Count = 0;
                }
            });

            //_bus.SubscribeAsync<PersonIntegrationQueue>("teste", async request =>
            //{
            //    Count++;
            //    if (Count == 1000)
            //    {
            //        Debug.WriteLine($"{DateTime.Now}:{request} => {Count}");
            //        Count = 0;
            //    }
            //});
        }
    }
}