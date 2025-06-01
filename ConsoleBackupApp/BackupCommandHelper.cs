using ConsoleBackupApp.Backup;
using ConsoleBackupApp.DataPaths;
using ConsoleBackupApp.Logging;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp;
public class BackupCommandHelper
{
    /// <summary>
    /// Add all priorbackups in backup folderPath
    /// </summary>
    /// <param name="priorBackups"></param>
    /// <param name="folderPath"></param>
    /// <returns>true if an error occured during this process</returns>
    private static void FindPriorBackupPathsInDirectory(string backupDir, List<PriorBackupPath> priorBackupPaths)
    {
        if (!Directory.Exists(backupDir))
        {
            return;
        }
        try
        {
            DirectoryInfo directoryInfo = new(backupDir);
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            foreach (DirectoryInfo dir in directoryInfos)
            {
                if (!PriorBackupPath.TryGetPriorBackup(dir.FullName, out PriorBackupPath priorBackupPath)) continue;

                priorBackupPaths.Add(priorBackupPath);
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Log(LogLevel.Error, ex.Message);
            throw;
        }
    }


    public static string FindBackupPathName(string backupDir)
    {
        DateTime dateTime = DateTime.Now;
        backupDir += $"Backup_{dateTime.Month}_{dateTime.Day}_{dateTime.Year}";

        //Add number if it already exists
        string temp = "";
        ushort count = 1;
        while (Directory.Exists(backupDir + temp))
        {
            temp = $"_{count}"; //Check if exists then increment number tile works
            count++;
        }
        return backupDir + temp + Path.DirectorySeparatorChar;
    }

    private static bool FindPriorBackupPathsByArgs(ReadOnlySpan<string> argsLeft, List<PriorBackupPath> priorBackupPaths)
    {
        for (int i = 0; i < argsLeft.Length; i++)
        {
            string path = argsLeft[i];
            if (PriorBackupPath.TryGetPriorBackup(path, out PriorBackupPath priorBackupPath))
            {
                //check if adding prior paths if path was already added
                if (priorBackupPaths.Contains(priorBackupPath))
                {
                    continue;
                }
                priorBackupPaths.Add(priorBackupPath);
            }
            else
            {
                Logger.Instance.Log(LogLevel.Warning, $"Can't find {priorBackupPaths[i]}");
                //exit Early invalid path
                return false;
            }
        }
        return true;
    }

    public static Result GetPriorBackupPaths(string backupDir, ReadOnlySpan<string> args, bool checkForBackupsInFolder, out List<PriorBackupPath> priorBackups)
    {
        priorBackups = [];
        if (checkForBackupsInFolder)
        {
            //Check in same folder as destination path
            FindPriorBackupPathsInDirectory(backupDir, priorBackups);
        }
        if (args.Length > 0)
        {
            if (!FindPriorBackupPathsByArgs(args, priorBackups))
            {
                return Result.Invalid_Path;
            }
        }
        return Result.Success;
    }

    //Collect DataPaths and StartBackup Process
    public static Result BackupData(string backupDir, List<PriorBackupPath> priorBackups)
    {
        List<DataPath> dataPaths = ValidDataPaths();

        //exit if there are no dataPaths to be backedup.
        if (dataPaths.Count == 0)
        {
            return Result.Empty;
        }

        string baseBackupFolder = FindBackupPathName(backupDir);

        //Check if the backup is a sub directory of one of the dataPaths
        foreach (DataPath dataPath in dataPaths)
        {
            if (backupDir.StartsWith(dataPath.SourcePath))
            {
                Logger.Instance.Log(LogLevel.Error, $"The selected backup path is a sub path of a directory to be backedup {dataPath.SourcePath} in {backupDir}");
                return Result.Invalid_Path;
            }
        }

        try
        {
            BackupController controller = BackupController.Init(baseBackupFolder, dataPaths, priorBackups);
            controller.Start(true);
        }
        catch (Exception)
        {

            Logger.Instance.Log(LogLevel.Fatal, $"Something Happened while backing up the data to {baseBackupFolder}");
            return Result.Error;
        }
        return Result.Success;
    }
    /// <summary>
    /// All DataPath are validated as either a file or directory if that can be located
    /// </summary>
    /// <returns></returns>
    private static List<DataPath> ValidDataPaths()
    {
        Queue<DataPath> unCheckedDataPaths = new(DataFileManager.GetDataPaths());
        List<DataPath> dataPaths = new(unCheckedDataPaths.Count);

        //Check if path currently exists in current files
        while (unCheckedDataPaths.TryDequeue(out DataPath dataPath))
        {
            string sourcePath = dataPath.SourcePath;
            if (dataPath.Type == PathType.File)
            {
                if (File.Exists(sourcePath))
                {
                    dataPaths.Add(dataPath);
                }
                else
                {
                    Console.WriteLine(LogLevel.Warning + $": Can't locate file: {sourcePath}");
                }
            }
            else if (dataPath.Type == PathType.Directory)
            {
                if (Directory.Exists(sourcePath))
                {
                    dataPaths.Add(dataPath);
                }
                else
                {
                    Console.WriteLine(LogLevel.Warning + $": Can't locate folder: {sourcePath}");
                }
            }
            else //PathType.Unknown
            {
                if (File.Exists(sourcePath))
                {
                    dataPath.Type = PathType.File;
                    dataPaths.Add(dataPath);
                }
                else if (Directory.Exists(sourcePath))
                {
                    dataPath.Type = PathType.Directory;
                    dataPaths.Add(dataPath);
                }
                else
                {
                    Console.WriteLine(LogLevel.Warning + $": Can't locate any folder or file at: {sourcePath}");
                }
            }
        }

        return dataPaths;
    }
}