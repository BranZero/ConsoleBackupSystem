
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
public class BackupController
{
    private ConcurrentQueue<DataPath> _dataPaths;
    private string[] _priorBackups;
    private readonly string _folderPath;
    private BackupStat _backupStats;
    private char[] _drives;
    private Thread[] _threads;

    public BackupController(string folderPath, DataPath[] dataPaths, List<string> priorBackups)
    {
        _folderPath = folderPath;
        _priorBackups = priorBackups.ToArray();
        _backupStats = new BackupStat();
        _dataPaths = new ConcurrentQueue<DataPath>();
        HashSet<char> driveLetters = new();
        foreach(var dataPath in dataPaths)
        {
            _dataPaths.Enqueue(dataPath);
            driveLetters.Add(dataPath.Drive);
        }
        _drives = driveLetters.ToArray();
    }
    public Result Start()
    {



        return Result.Success;
    }
}