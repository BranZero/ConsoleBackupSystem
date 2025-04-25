
using System.Reflection;
using ConsoleBackupApp.DataPaths;

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
    /// Support only adding one path at a time and ignore paths after it
    /// </summary>
    /// <param name="args"></param>
    public static Result Add(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            Console.WriteLine("Error: no arguments passed in.");
            return Result.No_Arguments;
        }

        PathType pathType = PathType.Unknown;
        Result optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == Result.Valid_Option)
        {
            if (!options.Contains('f') || options.Count != 1)//only one valid option
            {
                return Result.Invalid_Option;
            }
            //-f force is option to overwrite the file if it exists
            index++;
        }
        else if (optResult == Result.No_Options)
        {
            if (File.Exists(args[index]))
            {
                pathType = PathType.File;
            }
            else if (Directory.Exists(args[index]))
            {
                pathType = PathType.Directory;
                if (args[index][^1] != Path.DirectorySeparatorChar)
                {
                    args[index] += Path.DirectorySeparatorChar;
                }
            }
            else
            {
                return Result.Invalid_Path;
            }
        }
        else
        {
            return optResult;
        }

        ReadOnlySpan<string> argsLeft = new ReadOnlySpan<string>(args, index, args.Length - index);
        if (!DataPath.Init(pathType, argsLeft, out DataPath dataPath)) return Result.Invalid_Path;

        if (!DataPathFile.TryAddDataPath(dataPath)) return Result.Exists;

        return Result.Success;
    }

    public static Result Remove(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            Console.WriteLine("Error: no arguments passed in.");
            return Result.No_Arguments;
        }
        else if (args.Length > 2)
        {
            Console.WriteLine("Error: too many arguments passed in.");
            return Result.Too_Many_Arguments;
        }
        if (CheckOptions(args[index], out _) != Result.No_Options)
        {
            return Result.Invalid_Option;
        }
        //no options
        if (DataPathFile.TryRemoveDataPath(args[1]))
        {
            return Result.Success;
        }
        return Result.Failure;
    }

    internal static object Backup(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            Console.WriteLine("Error: no arguments passed in.");
            return Result.No_Arguments;
        }

        //Select Options
        bool checkPriorBackups = false;
        bool checkForBackupsInFolder = false;
        Result optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == Result.Valid_Option)
        {
            checkPriorBackups = options.Remove('n');
            checkForBackupsInFolder = options.Remove('c');
            index++;
            if (options.Count > 0)
            {
                return Result.Invalid_Option;
            }
        }
        else if (optResult != Result.No_Options)
        {
            return optResult;
        }

        //Check Backup Directory
        if (Directory.Exists(args[index]))
        {
            if (args[index][^1] != Path.DirectorySeparatorChar)
            {
                args[index] += Path.DirectorySeparatorChar;
            }
        }
        else
        {
            return Result.Invalid_Path;
        }

        //Check Prior Backups if Checked
        List<string> priorBackups = new List<string>();
        if(checkForBackupsInFolder)
        {
            if(BackupSystem.TryFindPriorBackupPathsInDirectory(args[index], out priorBackups))
            {

            }
        }

        throw new NotImplementedException();
    }

    internal static object Help(string[] args)
    {
        throw new NotImplementedException();
    }

    internal static string Version(string[] args)
    {
        var assembly = System.AppContext.BaseDirectory;
        if (assembly == null || !File.Exists(assembly))
        {
            return "Error: Can't retrieve the version number.";
        }
        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly);
        string version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";
        return version;
    }

    internal static void List(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Error: Too Many Arguments");
            return;
        }

        byte[] data = DataPathFile.ReadDataFile();
        DataPath[] dataPaths = DataPathFile.GetDataPaths(data);
        if (dataPaths.Length == 0)
        {
            Console.WriteLine("List is empty");
            return;
        }
        //Sort the data for easy viewing
        dataPaths = dataPaths.OrderBy(path => path).ToArray();
        foreach (var path in dataPaths)
        {
            Console.WriteLine("{0,-80}", path.GetSourcePath());
        }
    }


    public static Result CheckOptions(string input, out HashSet<char> options)
    {
        options = new HashSet<char>();
        if (string.IsNullOrEmpty(input)) return Result.No_Options;
        if (input.Length < 2) return Result.No_Options;
        if (input[0] != '-') return Result.No_Options;

        for (int i = 1; i < input.Length; i++) // Start from the second character
        {
            if (!options.Add(input[i]))
            {
                return Result.Duplicate_Option;
            }
        }
        return Result.Valid_Option;
    }
}


public enum Result
{
    Success,
    Failure,
    Error,
    Exists,

    No_Arguments,
    Too_Many_Arguments,

    //Options
    Invalid_Option,
    Duplicate_Option,
    Valid_Option,
    No_Options,
    Invalid_Path,
}
