
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
public class BackupController
{
    private readonly string[] _priorBackups;
    private readonly string _folderPath;
    private List<BackupArchives> _backupArchives;
    private List<BackupProcess> _backupProcesses;
    private BackupShared _backupShared;


    public BackupController(string folderPath, List<DataPath> dataPaths, List<string> priorBackups)
    {
        _folderPath = folderPath;
        _priorBackups = priorBackups.ToArray();
        _backupProcesses = new();

        //Load datapaths into queue and get all drives
        Queue<DataPath> dataPathsQueue = new Queue<DataPath>();
        HashSet<char> drives = new();
        foreach(var dataPath in dataPaths)
        {
            dataPathsQueue.Enqueue(dataPath);
            drives.Add(dataPath.Drive);
        }

        //Create Backup Consumers (Archives)
        List<ArchiveQueue> archiveQueues = new();
        _backupArchives = new();
        foreach (char drive in drives)
        {
            ArchiveQueue archiveQueue = new(drive);
            archiveQueues.Add(archiveQueue);
            _backupArchives.Add(new BackupArchives(archiveQueue));
        }

        _backupShared = new(dataPathsQueue, archiveQueues);

        //Create Backup Produces (Processes)
        for (int i = 0; i < _backupArchives.Count*2; i++)//TODO: Change double backup archives
        {
            BackupProcess backupProcess = new(_backupShared, _priorBackups);
            _backupProcesses.Add(backupProcess);
        }
    }
    public Result Start()
    {
        CancellationTokenSource cancellationToken = new();

        //Start the Consumers (Archives)
        Stack<Thread> threadsArchive = new();
        Stack<Thread> threadsProcess = new();
        foreach (var archive in _backupArchives)
        {
            threadsArchive.Push(new Thread(() => archive.Start(_folderPath, cancellationToken.Token)));
            threadsArchive.Last().Start();
        }

        //Start the Producers (Processes)
        foreach (var process in _backupProcesses)
        {
            threadsArchive.Push(new Thread(() => process.Start(cancellationToken.Token)));
            threadsArchive.Last().Start();
        }

        foreach (var process in _backupProcesses)
        {
            Thread thread = threadsProcess.Pop();
            thread.Join();
        }

        //Request normal finish
        cancellationToken.Cancel();
        for (int i = _backupArchives.Count; i > 0; i--)
        {
            Thread thread = threadsArchive.Pop();
            thread.Join();
        }


        return Result.Success;
    }
}