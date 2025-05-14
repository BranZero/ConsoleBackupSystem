
using System.Collections.Concurrent;

namespace ConsoleBackupApp.Logging;
public class Logger
{
    private static BlockingCollection<string> _logQueue = [];
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
        _logQueue.Add(logEntry);
    }

    public static async Task StartLoggingProcess(CancellationToken token)
    {
        //only allow for one to be created
        if (_token != CancellationToken.None)
        {
            return;
        }
        _token = token;

        while (!_token.IsCancellationRequested)
        {
            await Task.Delay(100, token); //for last second log messages have delay before Dequeue
            while (_logQueue.TryTake(out string? logEntry))
            {
                await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
            }
        }
        //empty queue
        while (_logQueue.TryTake(out string? logEntry))
        {
            await File.AppendAllTextAsync(Log_File_Path, logEntry + Environment.NewLine);
        }
    }
}
