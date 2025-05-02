
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


    public BackupController(string folderPath, DataPath[] dataPaths, List<string> priorBackups)
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

    }
    public Result Start()
    {
        CancellationToken cancellationToken = new();

        //Start the Consumers (Archives)
        foreach (var archive in _backupArchives)
        {
            new Thread(() => archive.Start(_folderPath, cancellationToken)).Start();
        }

        //Start the Producers (Processes)


        return Result.Success;
    }
}