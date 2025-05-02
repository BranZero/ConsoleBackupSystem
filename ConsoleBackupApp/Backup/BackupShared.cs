
using System.Collections.Concurrent;
using ConsoleBackupApp.Backup;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
public class BackupShared
{
    private BackupStat _backupStats;
    private Mutex _statsMutex;
    private Queue<DataPath> _dataPaths;
    private Mutex _dataPathMutex;
    private List<ArchiveQueue> _archiveQueues;

    public BackupShared(Queue<DataPath> dataPaths, List<ArchiveQueue> archiveQueues)
    {
        _backupStats = new();

        _statsMutex = new();
        _dataPathMutex = new();

        _dataPaths = dataPaths;
        _archiveQueues = archiveQueues;
    }

}



public class ArchiveQueue(char drive)
{
    public readonly char Drive = drive;
    public readonly BlockingCollection<string> PathsToCopy = new();
}