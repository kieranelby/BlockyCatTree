using BlockyCatTree.Generation.IO;

namespace BlockyCatTree.Generation;

public class Runner
{
    private const int MaxConcurrent = 2;

    private readonly IGenerator _generator;
    private readonly SnapshotWriter _snapshotWriter;
    private readonly bool _finalOnly;
    private readonly List<Task> _tasks;

    public Runner(IGenerator generator, SnapshotWriter snapshotWriter, bool finalOnly)
    {
        _generator = generator;
        _snapshotWriter = snapshotWriter;
        _finalOnly = finalOnly;
        _tasks = new List<Task>();
    }

    public void Run()
    {
        while (!_generator.IsDone)
        {
            _generator.DoNextStep();
            Console.WriteLine($"{_generator.TotalStepNumber},{_generator.StageName},{_generator.StageStepNumber},{_generator.IsDone}");
            WriteSnapshotIfNeeded();
        }
        Task.WaitAll(_tasks.ToArray());
    }

    private void RemoveCompletedTasks()
    {
        _tasks.RemoveAll(t => t.IsCompleted);
    }

    private void WriteSnapshotIfNeeded()
    {
        RemoveCompletedTasks();
        var writerIdle = _tasks.Count == 0;
        var snapshotNeeded = _generator.IsDone || _generator.StageStepNumber == 0;
        if ((!snapshotNeeded && !writerIdle) || (_finalOnly && !_generator.IsDone))
        {
            return;
        }
        while (true)
        {
            RemoveCompletedTasks();
            if (_tasks.Count < MaxConcurrent)
            {
                break;
            }
            Task.WaitAny(_tasks.ToArray());
        }
        var snapshot = _generator.TakeSnapshot();
        var task = Task.Run(() => { _snapshotWriter.Write(snapshot); });
        _tasks.Add(task);
    }
}
