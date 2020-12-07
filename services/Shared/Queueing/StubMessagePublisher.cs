namespace Koasta.Shared.Queueing
{
    public class StubMessagePublisher : IMessagePublisher
    {
        public void DirectPublish<T>(string queue, string deadletterRoutingKey, string deadletterExchange, string deadletterQueue, T data) where T : class
        {
        }

        public void Publish<T>(string exchange, PublishStrategy strategy, T data) where T : class
        {
        }

        public void Publish<T>(string exchange, PublishStrategy strategy, string routingKey, T data) where T : class
        {
        }
    }
}
