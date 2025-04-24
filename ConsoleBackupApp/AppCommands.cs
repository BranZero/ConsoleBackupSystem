
namespace ConsoleBackupApp;
public class AppCommands
{
    public static void Exit(string[] args)
    {
        if (args.Length != 1)
        {
            return;
        }
        Environment.Exit(0);
    }

    /// <summary>
    /// Support only adding one path at a time and add it to the data file
    /// </summary>
    /// <param name="args"></param>
    public static Result Add(string[] args)
    {
        return Result.Failure;
    }

    public static Result Remove(string[] args)
    {
        return Result.Failure;
    }

    internal static object Help(string[] args)
    {
        throw new NotImplementedException();
    }

    internal static object Version(string[] args)
    {
        throw new NotImplementedException();
    }

    internal static object Backup(string[] args)
    {
        throw new NotImplementedException();
    }
}


public enum Result
{
    Success,
    Failure,
    Error,

    InvalidOption,
    InvalidPath,
}
