
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
public class BackupController
{
    private ConcurrentQueue<DataPath> _dataPaths;
    private string[] _priorBackups;
    private readonly string _folderPath;
    private BackupStat _backupStats;
    private HashSet<BackupArchives> _backupArchives;
    private List<BackupProcess> _backupProcesses;

    public BackupController(string folderPath, DataPath[] dataPaths, List<string> priorBackups)
    {
        _folderPath = folderPath;
        _priorBackups = priorBackups.ToArray();
        _backupStats = new BackupStat();
        _backupProcesses = new();

        //Load datapaths into queue and get all drives
        _dataPaths = new ConcurrentQueue<DataPath>();
        _backupArchives = new HashSet<BackupArchives>();
        foreach(var dataPath in dataPaths)
        {
            _dataPaths.Enqueue(dataPath);
            _backupArchives.Add(new BackupArchives(dataPath.Drive));
        }


    }
    public Result Start()
    {



        return Result.Success;
    }
}