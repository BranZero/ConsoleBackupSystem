
using System.Collections.Concurrent;

namespace ConsoleBackupApp.Logging;
public class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger(), true);
    public static Logger Instance => _instance.Value;
    private CancellationToken _token;

    private readonly BlockingCollection<string> _logQueue;
    private const string Log_File_Path = @".\Log.log";
    private readonly object _startLock = new();
    private bool _started = false;
    private Logger()
    {
        _logQueue = [];
    }
    public void Log(LogLevel logLevel, string message)
    {
        string logEntry = $"{DateTime.Now:dd/MM/yy HH:mm:ss} : {logLevel} : {message}";
        if (_token == CancellationToken.None || _logQueue.IsAddingCompleted) //The Logger has yet to startup yet
        {
            Console.WriteLine(logEntry);
        }
        else
        {
            _logQueue.Add(logEntry);
        }
    }

    public async Task StartLoggingProcess(CancellationToken token)
    {
        lock (_startLock)
        {
            if (_started)
            {
                return;
            }
            _started = true;
            _token = token;
        }

        while (!_token.IsCancellationRequested)
        {
            await Task.Delay(100, token); //for last second log messages have delay before Dequeue
            while (_logQueue.TryTake(out string? logEntry))
            {
                await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
            }
        }
        //empty queue
        _logQueue.CompleteAdding();
        foreach (string logEntry in _logQueue.GetConsumingEnumerable())
        {
            await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
        }
        //cleanup
        _logQueue.Dispose();
    }
}
