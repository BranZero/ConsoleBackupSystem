
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
public class BackupData
{
    private ConcurrentQueue<DataPath> _dataPaths;
    private readonly string _folderPath;
    private BackupStat _filePathTotal;

    public BackupData()
    {

    }
}