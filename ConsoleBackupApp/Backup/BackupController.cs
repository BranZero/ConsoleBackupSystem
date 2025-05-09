
using ConsoleBackupApp.DataPaths;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp.Backup;
public class BackupController
{
    private readonly PriorBackupPath[] _priorBackups;
    private readonly string _folderPath;
    private List<BackupArchives> _backupArchives;
    private List<BackupProcess> _backupProcesses;
    private BackupShared _backupShared;


    public BackupController(string folderPath, List<DataPath> dataPaths, List<PriorBackupPath> priorBackups)
    {
        _folderPath = folderPath;
        priorBackups.Sort();
        _priorBackups = priorBackups.ToArray();
        _backupProcesses = [];

        //Load datapaths into queue and get all drives
        Queue<DataPath> dataPathsQueue = new Queue<DataPath>();
        HashSet<char> drives = new();
        foreach(var dataPath in dataPaths)
        {
            dataPathsQueue.Enqueue(dataPath);
            drives.Add(dataPath.Drive);
        }

        //Create Backup Consumers (Archives)
        List<ArchiveQueue> archiveQueues = [];
        _backupArchives = [];
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
        if (_backupShared.DataPathsIsEmpty())
        {
            return Result.Empty;
        }
        if (!SetupBackupDirectory(_folderPath))
        {
            return Result.Error;
        }

        CancellationTokenSource cancellationToken = new();

        //Start the Consumers (Archives)
        Stack<Thread> threadsArchive = new();
        Stack<Thread> threadsProcess = new();
        foreach (var archive in _backupArchives)
        {
            threadsArchive.Push(new Thread(() => archive.Start(_folderPath, cancellationToken.Token)));
            threadsArchive.Peek().Start();
        }

        //Start the Producers (Processes)
        foreach (var process in _backupProcesses)
        {
            threadsProcess.Push(new Thread(() => process.Start(cancellationToken.Token)));
            threadsProcess.Peek().Start();
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

    private bool SetupBackupDirectory(string path)
    {
        try
        {
            DirectoryInfo directoryInfo = Directory.CreateDirectory(path);
            return directoryInfo.Exists;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}