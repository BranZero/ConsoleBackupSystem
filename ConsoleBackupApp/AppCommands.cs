
using ConsoleBackupApp.DataPaths;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp;

public class AppCommands
{
    private const char FORCE_COPY = 'c';
    private const char ALL_OR_NONE = 'a';

    private const string HELP_MESSAGE = @"
add [-options] <path> [Files/Directories Names To Ignore...]
    -f : Force add the path even if it doesn't exist.
    -c : Use ForceCopy mode (always copy files).
    -a : Use AllOrNone mode (copy all files in a directory only if any file has changed).
    Default: If no mode is specified, None mode is used (copy files only if not present or modified since prior backups).

remove <path>

updatec [-c | -a] <sourcePath>
    -c: Set copy mode to ForceCopy.
    -a: Set copy mode to AllOrNone.
    -No option: Sets copy mode to None (default).

updatei [-a | -r] <sourcePath> [ignorePaths...] 
    -a: Add new ignorePaths.
    -d: Remove existing ignorePaths.

list

backup [-options] <destinationDirectory> [priorBackupDirectories...]
    -n : Use the list of args for prior backups.
    -c : Check for prior backups in the destination folder.
";

    /// <summary>
    /// Support only adding one path at a time and ignore paths after it
    /// </summary>
    /// <param name="args"></param>
    public static Result Add(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            return new(ResultType.Too_Few_Arguments, "Usage: add [-options] <path> [Files/Directories Names To Ignore...]");
        }

        PathType pathType = PathType.Unknown;
        CopyMode copyMode = CopyMode.None;
        ResultType optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == ResultType.Duplicate_Option)
        {
            return new(optResult);
        }
        else if (optResult == ResultType.Valid_Option)
        {
            index++;
        }

        //CopyMode Checks (Mutually Exclusive)
        if (options.Remove(FORCE_COPY))//ForceCopy
        {
            copyMode = CopyModeExtensions.FromChar(FORCE_COPY);
        }
        else if (options.Remove(ALL_OR_NONE))//AllOrNone
        {
            copyMode = CopyModeExtensions.FromChar(ALL_OR_NONE);
        }

        //PathType Checks
        //-f force is option to overwrite the check if it exists in the directory
        if (!options.Remove('f'))
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
                return new(ResultType.Path_Not_Found, "Add -f option to override this if this is the correct path");
            }
        }

        if (options.Count > 0)
        {
            return new(ResultType.Invalid_Option);
        }

        ReadOnlySpan<string> argsLeft = new(args, index, args.Length - index);
        if (!DataPath.Init(pathType, copyMode, argsLeft, out DataPath dataPath)) return new(ResultType.Path_Invalid);

        return DataFileManager.TryAddDataPath(dataPath);
    }

    public static Result Remove(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            Console.WriteLine("Error: no arguments passed in.");
            return new(ResultType.Too_Few_Arguments, "Usage: remove <path>");
        }
        else if (args.Length > 2)
        {
            Console.WriteLine("Error: too many arguments passed in.");
            return new(ResultType.Too_Many_Arguments, "Usage: remove <path>");
        }
        if (CheckOptions(args[index], out _) != ResultType.No_Options)
        {
            return new(ResultType.Invalid_Option, "Usage: remove <path>");
        }
        //no options
        return DataFileManager.TryRemoveDataPath(args[1]);

    }

    public static Result Backup(string[] args)
    {
        int index = 1;
        if (args.Length < 2)
        { // Check if their are arguments passed in
            return new(ResultType.Too_Few_Arguments, "Usage: backup [-options] <destinationDirectory> [priorBackupDirectories...]");
        }

        //Select Options
        bool checkSecondaryPriorBackups = false;
        bool checkForBackupsInFolder = false;
        ResultType optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == ResultType.Valid_Option)
        {
            index++;
            checkSecondaryPriorBackups = options.Remove('n');
            checkForBackupsInFolder = options.Remove('c');
            if (options.Count > 0)
            {
                return new(ResultType.Invalid_Option);
            }
        }
        else if (optResult != ResultType.No_Options)
        {
            return new(optResult);
        }

        //Valid Destination Directory of Backup
        string backupDir = args[index++];
        if (Directory.Exists(backupDir))
        {
            if (backupDir[^1] != Path.DirectorySeparatorChar)
            {
                backupDir += Path.DirectorySeparatorChar;
            }
        }
        else
        {
            return new(ResultType.Path_Not_Found);
        }

        //Check Prior Backup Directories
        ReadOnlySpan<string> argsLeft = [];
        if (checkSecondaryPriorBackups)
        {
            if (args.Length <= index)
            {
                return new(ResultType.Too_Few_Arguments, "Expected additional arguments for secondary prior backup locations.");
            }
            argsLeft = new ReadOnlySpan<string>(args, index, args.Length - index);
        }
        ResultType result = BackupCommandHelper.GetPriorBackupPaths(backupDir, argsLeft, checkForBackupsInFolder, out List<PriorBackupPath> priorBackups);
        if (result != ResultType.Success)
        {
            return new(result, "Problem occured getting prior backup paths (check logs for error message).");
        }

        return BackupCommandHelper.BackupData(backupDir, priorBackups);//if priorBackups is empty don't check prior backups
    }

    internal static string Help(string[] args)
    {
        if (args.Length > 1)
        {
            return ResultType.Too_Many_Arguments.ToString();
        }
        return HELP_MESSAGE;
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

        DataPath[] dataPaths = DataFileManager.GetDataPaths();
        if (dataPaths.Length == 0)
        {
            Console.WriteLine("List is empty");
            return;
        }
        //Sort the data for easy viewing
        dataPaths = dataPaths.OrderBy(path => path).ToArray();
        foreach (var path in dataPaths)
        {
            Console.WriteLine("{0,-80}", path.SourcePath);
        }
    }


    public static ResultType CheckOptions(string input, out HashSet<char> options)
    {
        options = new HashSet<char>();
        if (string.IsNullOrEmpty(input)) return ResultType.No_Options;
        if (input.Length < 2) return ResultType.No_Options;
        if (input[0] != '-') return ResultType.No_Options;

        for (int i = 1; i < input.Length; i++) // Start from the second character
        {
            if (!options.Add(input[i]))
            {
                return ResultType.Duplicate_Option;
            }
        }
        return ResultType.Valid_Option;
    }

    /// <summary>
    /// Sets the CopyMode of a DataPath based on updatec [option] <sourcePath>
    /// </summary>
    /// <param name="args"></param>
    /// <returns>result of command</returns>
    public static Result UpdateCopyMode(string[] args)
    {
        if (args.Length < 2)
        {
            return new(ResultType.Too_Few_Arguments, "Usage: updatec [-copyMode] <sourcePath>");
        }
        else if (args.Length > 3)
        {
            return new(ResultType.Too_Many_Arguments, "Usage: updatec [-copyMode] <sourcePath>");
        }
        int index = 1;
        var optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == ResultType.Valid_Option)
        {
            index++;
            if (options.Count != 1)
            {
                return new(ResultType.Invalid_Option, "Usage: updatec accepts either a = AllOrNone or c = ForceCopy or no option for default mode");
            }
            if (options.Remove(FORCE_COPY))
            {
                if (!DataFileManager.TryUpdateDataPathCopyMode(args[index], CopyMode.ForceCopy))
                {
                    return new(ResultType.Not_Found, $"DataPath Not Found: {args[index]}");
                }
            }
            else if (options.Remove(ALL_OR_NONE))
            {
                if (!DataFileManager.TryUpdateDataPathCopyMode(args[index], CopyMode.AllOrNone))
                {
                    return new(ResultType.Not_Found, $"DataPath Not Found: {args[index]}");
                }
            }
            else
            {
                return new(ResultType.Invalid_Option, "Usage: updatec accepts either a = AllOrNone or c = ForceCopy or no option for default mode");
            }
        }
        else if (optResult == ResultType.No_Options)
        { // Default mode
            if (!DataFileManager.TryUpdateDataPathCopyMode(args[index], CopyMode.None))
            {
                return new(optResult, $"DataPath Not Found: {args[index]}");
            }
        }
        else
        {
            return new(optResult, "Usage: updatec [-copyMode] <sourcePath>");
        }
        return new(ResultType.Success);
    }
    /// <summary>
    /// Changes ignore paths based on updatei [a|d] <sourcePath> [ignorePaths...] 
    /// </summary>
    /// <param name="args"></param>
    /// <returns>result of command</returns>
    public static Result UpdateIgnorePaths(string[] args)
    {
        if (args.Length < 4)
        {
            return new(ResultType.Too_Few_Arguments, "Usage: updatei [-a | -d] <sourcePath> [ignorePaths...]");
        }
        int index = 1;
        var optResult = CheckOptions(args[index], out HashSet<char> options);
        if (optResult == ResultType.Valid_Option)
        {
            index++;
            if (options.Count > 1)
            {
                return new(ResultType.Invalid_Option, "Usage: updatei accepts either a = Add or r = Remove");
            }

            if (options.Remove('a'))
            {
                // Add ignore paths logic here
                var result = DataFileManager.TryAddIgnorePaths(args[index], args[(index + 1)..]);
                return result;


            }
            else if (options.Remove('r'))
            {
                // Remove ignore paths logic here
                var result = DataFileManager.TryRemoveIgnorePaths(args[index], args[(index + 1)..]);
                return result;
            }
            else
            {
                return new(ResultType.Invalid_Option, "Usage: updatei accepts either a = Add or r = Remove");
            }
        }
        else
        {
            return new(optResult, "Usage: updatei [-a | -d] <sourcePath> [ignorePaths...]");
        }
    }
}


