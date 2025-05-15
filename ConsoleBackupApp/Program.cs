using System.Text;
using ConsoleBackupApp.Logging;

namespace ConsoleBackupApp;
public class Program
{
    private static readonly CancellationTokenSource _loggerToken = new();
    private static Task? _loggerTask;
    public static void Main(string[] args)
    {
        _loggerTask = Task.Run(() => Logger.Instance.StartLoggingProcess(_loggerToken.Token));
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
            args = GetArgs(input);
            string result = Command(args);
            Console.WriteLine(result);
        }
    }

    private static string[] GetArgs(string input)
    {
        var args = new List<string>();
        var currentArg = new StringBuilder();
        bool singleQuoteBlock = false;
        bool doubleQuoteBlock = false;


        foreach (char c in input)
        {
            //Single Quote Block
            if (c == '\'' && !singleQuoteBlock && !doubleQuoteBlock)
            {
                singleQuoteBlock = true;
                continue;
            }
            else if (c == '\'' && singleQuoteBlock)
            {
                singleQuoteBlock = false;
                continue;
            }
            //Double Quote Block
            else if (c == '\"' && !singleQuoteBlock && !doubleQuoteBlock)
            {
                singleQuoteBlock = true;
                continue;
            }
            else if (c == '\"' && doubleQuoteBlock)
            {
                doubleQuoteBlock = false;
                continue;
            }


            if (char.IsWhiteSpace(c) && !singleQuoteBlock && !doubleQuoteBlock)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
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
    public static async void Exit(string[] args)
    {
        if (args.Length != 1)
        {
            return;
        }
        _loggerToken.Cancel();
        if (_loggerTask != null)
        {
            await _loggerTask;
        }
        Environment.Exit(0);
    }
}