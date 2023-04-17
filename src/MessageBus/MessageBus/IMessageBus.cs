using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Internals;
using EasyNetQ.Topology;
using MessageBus.Messages.Base;
using MessageBus.Messages.Response;

namespace MessageBus.MessageBus
{
    public interface IMessageBus : IDisposable
    {
        bool IsConnected { get; }
        IAdvancedBus AdvancedBus { get; }

        Task Publish<T>(T message) where T : IntegrationEvent;
        Task PublishAsync<T>(T message) where T : IntegrationEvent;
        Task PublishAsync<T>(T message, string exchangeName, ExchangeKind exchangeType, string routingKey) where T : IntegrationEvent;

        Task<Queue> CreateQueueWithBinding(string exchangeName, ExchangeKind exchangeType, string queueName, string routingKey);

        Task Subscribe<T>(string subscriptionId, Action<T> onMessage) where T : class;
        Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> onMessage) where T : class;

        Task<TResponse> Request<TRequest, TResponse>(TRequest request)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;

        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;

        Task<IDisposable> Respond<TRequest, TResponse>(Func<TRequest, TResponse> responder)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;

        Task<AwaitableDisposable<IDisposable>> RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> responder)
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;
    }
}
