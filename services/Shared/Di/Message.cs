namespace Koasta.Shared.Queueing
{
    public class Message<T> where T : class
    {
        public string Type { get; set; }
        public T Data { get; set; }
    }
}
