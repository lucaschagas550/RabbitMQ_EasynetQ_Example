using EasyNetQ;
using EasyNetQ.Internals;
using EasyNetQ.Topology;
using MessageBus.Messages.Base;
using MessageBus.Messages.Response;
using Polly;
using RabbitMQ.Client.Exceptions;

namespace MessageBus.MessageBus
{
    public class MessageBus : IMessageBus
    {
        private IBus _bus;
        private IAdvancedBus _advancedBus;

        private readonly string _connectionString;

        public MessageBus(string connectionString)
        {
            _connectionString = connectionString;
            EnsureConnection();
        }

        public bool IsConnected => _bus?.Advanced?.IsConnected ?? false;
        public IAdvancedBus AdvancedBus => _bus?.Advanced;

        public async Task Publish<T>(T message) where T : IntegrationEvent
        {
            await EnsureConnection();
            _bus.PubSub.Publish(message);
        }

        public async Task PublishAsync<T>(T message) where T : IntegrationEvent
        {
            await EnsureConnection();
            await _bus.PubSub.PublishAsync(message);
        }

        public async Task PublishAsync<T>(T message, string exchangeName, ExchangeKind exchangeType, string routingKey) where T : IntegrationEvent
        {
            await EnsureConnection();

            var exchange = await _advancedBus.ExchangeDeclareAsync(exchangeName, exchangeType.ToString());

            await _advancedBus.PublishAsync(exchange, routingKey, false, new Message<T>(message));
        }

        public async Task<Queue> CreateQueueWithBinding(string exchangeName, ExchangeKind exchangeType, string queueName, string routingKey)
        {
            await EnsureConnection();

            var exchange = _advancedBus.ExchangeDeclare(exchangeName, exchangeType.ToString());
            var queue = await _advancedBus.QueueDeclareAsync(queueName);
            await _advancedBus.BindAsync(exchange, queue, routingKey, new Dictionary<string, object>(), CancellationToken.None);

            return queue;
        }

        public async Task Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class
        {
            await EnsureConnection();
            _bus.PubSub.Subscribe(subscriptionId, onMessage);
        }

        public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class
        {
            await EnsureConnection();
            await _bus.PubSub.SubscribeAsync(subscriptionId, onMessage);
        }

        public async Task<TResponse> Request<TRequest, TResponse>(TRequest request) where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            await EnsureConnection();
            return _bus.Rpc.Request<TRequest, TResponse>(request);
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IntegrationEvent where TResponse : ResponseMessage
        {
            await EnsureConnection();
            return await _bus.Rpc.RequestAsync<TRequest, TResponse>(request);
        }

        public async Task<IDisposable> Respond<TRequest, TResponse>(Func<TRequest, TResponse> response)
            where TRequest : IntegrationEvent where TResponse : ResponseMessage
        {
            await EnsureConnection();
            return _bus.Rpc.Respond(response);
        }

        public async Task<AwaitableDisposable<IDisposable>> RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> response)
            where TRequest : IntegrationEvent where TResponse : ResponseMessage
        {
            await EnsureConnection();
            return _bus.Rpc.RespondAsync(response);
        }

        /// <summary>
        /// to try to connect, however if there is already a connection it just returns
        /// </summary>
        private async Task EnsureConnection()
        {
            if (IsConnected)
                return;

            var policy = Policy.Handle<EasyNetQException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            await policy.Execute(async () =>
            {
                _bus = RabbitHutch.CreateBus(_connectionString);
                await _bus.Advanced.ConnectAsync();
                _advancedBus = _bus.Advanced;
                _advancedBus.Disconnected += OnDisconnect;
            });
        }

        private void OnDisconnect(object s, EventArgs e)
        {
            var policy = Policy.Handle<EasyNetQException>()
                .Or<BrokerUnreachableException>()
                .RetryForever();

            policy.Execute(EnsureConnection);
        }

        public void Dispose()
        {
            _bus.Dispose();
        }
    }
}
