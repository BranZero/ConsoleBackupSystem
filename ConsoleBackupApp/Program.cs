using ConsoleBackupApp.Logging;

namespace ConsoleBackupApp;
public class Program
{
    private static readonly CancellationTokenSource _loggerToken = new();
    public static void Main(string[] args)
    {
        Task.Run(() => Logger.ProcessLogQueue(_loggerToken.Token));
        if (args.Length > 0)
        {
            Command(args);
            return;
        }

        string? input;
        while (true) //'exit' with close the program
        {
            input = Console.ReadLine();
            if (input is null)
            {
                continue;
            }
            args = input.Split(' ');
            string result = Command(args);
            Console.WriteLine(result);
        }
    }
    public static string Command(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return "Empty";
        }
        switch (args[0].ToLower())
        {
            case "exit":
                Exit(args);
                return "Closing";
            case "add":
                return AppCommands.Add(args).ToString();
            case "remove":
                return AppCommands.Remove(args).ToString();
            case "help":
                return AppCommands.Help(args).ToString();
            case "version":
                return AppCommands.Version(args);
            case "backup":
                return AppCommands.Backup(args).ToString();
            case "list":
                AppCommands.List(args);
                return "";
            default:
                return "Invalid Command";
        }
    }
    public static void Exit(string[] args)
    {
        if (args.Length != 1)
        {
            return;
        }
        _loggerToken.CancelAsync();
        Environment.Exit(0);
    }
}