
using System.Collections.Concurrent;

namespace ConsoleBackupApp.Logging;
public class Logger
{
    private static ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
    private const string Log_File_Path = @".\Log.log";
    private static CancellationToken _token = CancellationToken.None;

    public static void Log(LogLevel logLevel, string message)
    {
        if (_token == CancellationToken.None) //The Logger has yet to startup yet
        {
            Console.WriteLine($"{DateTime.Now:dd/MM/yy HH:mm:ss} : {logLevel} : {message}");
            return; 
        }
        string logEntry = $"{DateTime.Now:dd/MM/yy HH:mm:ss} : {logLevel} : {message}";
        _logQueue.Enqueue(logEntry);
    }

    public static async Task ProcessLogQueue(CancellationToken token)
    {
        //only allow for one to be created
        if (_token != CancellationToken.None)
        {
            return;
        }
        _token = token;

        while (!_token.IsCancellationRequested)
        {
            await Task.Delay(100); //for last second log messages have delay before Dequeue
            while (_logQueue.TryDequeue(out string? logEntry))
            {
                await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
            }
        }
        //empty queue
        while (_logQueue.TryDequeue(out string? logEntry))
        {
            await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
        }
    }
}
