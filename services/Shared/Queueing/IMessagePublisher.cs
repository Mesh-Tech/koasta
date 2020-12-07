namespace Koasta.Shared.Queueing
{
    public interface IMessagePublisher
    {
        void DirectPublish<T>(string queue, string deadletterRoutingKey, string deadletterExchange, string deadletterQueue, T data) where T : class;

        void Publish<T>(string exchange, PublishStrategy strategy, T data) where T : class;

        void Publish<T>(string exchange, PublishStrategy strategy, string routingKey, T data) where T : class;
    }
}
