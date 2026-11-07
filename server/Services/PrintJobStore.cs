using System.Collections.Concurrent;
using SmartCv.Models;

namespace SmartCv.Services;

public interface IPrintJobStore
{
    PrintJob Add(PrintJob job);
    PrintJob? Get(string id);
    void Remove(string id);
}

public class PrintJobStore : IPrintJobStore
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, (PrintJob Job, DateTime Created)> _jobs = new();

    public PrintJob Add(PrintJob job)
    {
        foreach (var expired in _jobs.Where(pair => DateTime.UtcNow - pair.Value.Created > Lifetime).Select(pair => pair.Key))
        {
            _jobs.TryRemove(expired, out _);
        }

        job.Id = Guid.NewGuid().ToString("N");
        _jobs[job.Id] = (job, DateTime.UtcNow);
        return job;
    }

    public PrintJob? Get(string id) => _jobs.TryGetValue(id, out var entry) ? entry.Job : null;

    public void Remove(string id) => _jobs.TryRemove(id, out _);
}
