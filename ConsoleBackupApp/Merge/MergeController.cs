using ConsoleBackupApp.Backup;
using ConsoleBackupApp.Logging;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp.Merge;

public class MergeController
{
    private readonly PriorBackupPath[] _priorBackups;
    private readonly string _folderPath;
    private List<MergeArchives> _mergeArchives;
    private List<MergeProcess> _mergeProcesses;

    private MergeController(string folderPath, List<MergeArchives> mergeArchives, List<MergeProcess> mergeProcesses, PriorBackupPath[] priorBackupPaths)
    {
        _folderPath = folderPath;
        _mergeArchives = mergeArchives;
        _mergeProcesses = mergeProcesses;
        _priorBackups = priorBackupPaths;
    }

    public static MergeController Init(List<PriorBackupPath> priorBackups)
    {
        priorBackups.Sort();//newest prior backup first
        CheckDriveLetters(priorBackups, out HashSet<char> drives);
        PriorBackupPath[] priorBackupsList = [.. priorBackups];

        // Used newest backup directory as most recent one
        string mergedDir = priorBackups[0].FullPath;

        //Create Merge Processes (MergeProcess)
        List<MergeArchives> mergeArchives = [];
        List<MergeProcess> mergeProcesses = [];
        foreach (var drive in drives)
        {
            MergeArchives mergeArchive = new(drive, mergedDir);
            mergeArchives.Add(mergeArchive);
            MergeProcess mergeProcess = new(mergeArchive, priorBackupsList, drive);
            mergeProcesses.Add(mergeProcess);
        }
        return new(mergedDir, mergeArchives, mergeProcesses, priorBackupsList);
    }

    public Result Start()
    {
        if (!BackupController.SetupDirectory(_folderPath))
        {
            return new(ResultType.Error, $"Setting up the folder: {_folderPath}");
        }

        CancellationTokenSource cancellationToken = new();

        //Start the (Archives)
        Stack<Thread> threadsProcess = new();
        foreach (var archive in _mergeArchives)
        {
            archive.Start();
        }

        //Start the (Processes)
        foreach (var process in _mergeProcesses)
        {
            threadsProcess.Push(new Thread(() => process.Start(cancellationToken.Token)));
            threadsProcess.Peek().Start();
        }

        foreach (var process in _mergeProcesses)
        {
            Thread thread = threadsProcess.Pop();
            thread.Join();
        }

        //Request normal finish
        cancellationToken.Cancel();
        foreach (var archive in _mergeArchives)
        {
            archive.Close();
        }
        GC.Collect();
        //Delete Priors
        CleanUpMergedBackups([.. _priorBackups[1..]]);

        return new(ResultType.Success);
    }

    private static void CheckDriveLetters(List<PriorBackupPath> priorBackups, out HashSet<char> drives)
    {
        drives = [];
        foreach (var item in priorBackups)
        {
            try
            {
                DirectoryInfo dir = new(item.FullPath);
                foreach (var zipfile in dir.GetFiles("*.zip"))
                {
                    drives.Add(zipfile.Name[0]);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
            }
        }
    }

    private static void CleanUpMergedBackups(List<PriorBackupPath> priorBackups)
    {
        try
        {
            foreach (var prior in priorBackups)
            {
                Directory.Delete(prior.FullPath, true);
            }
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
        }
    }
}