using System.Threading.Tasks;

namespace Koasta.Service.Scheduler.Jobs
{
    public interface IJob
    {
        string Name { get; }

        string TriggerTime { get; }

        Task ProcessJob();
    }
}
