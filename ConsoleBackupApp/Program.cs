

namespace ConsoleBackupApp;
public class Program
{
    public static void Main(string[] args)
    {
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
                AppCommands.Exit(args);
                return "Closing";
            case "add":
                return AppCommands.Add(args).ToString();
            case "remove":
                return AppCommands.Remove(args).ToString();
            case "help":
                return AppCommands.Help(args).ToString();
            case "version":
                return AppCommands.Version(args).ToString();
            case "backup":
                return AppCommands.Backup(args).ToString();
            default:
                return "Invalid Command";
        }
    }
}