using System.IO.Compression;
using ConsoleBackupApp.Logging;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp.Merge;

public class MergeProcess
{
    private CancellationToken _cancellationToken;
    private readonly PriorBackupPath[] _priorBackups;
    private MergeArchives _mergeArchives;
    private HashSet<string> _pathsComplete;
    private readonly char _drive;

    public MergeProcess(MergeArchives mergeArchives, PriorBackupPath[] priorBackups, char drive)
    {
        _mergeArchives = mergeArchives;
        _priorBackups = priorBackups;
        _pathsComplete = [];
        _drive = drive;
    }

    public void Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        Process();
    }

    private void Process()
    {
        LoadCurrentArchive();

        for (int index = 1; !_cancellationToken.IsCancellationRequested && index < _priorBackups.Length; index++)
        {
            try
            {
                string path = Path.Combine(_priorBackups[index].FullPath, _drive + ".zip");
                using ZipArchive zipArchive = ZipFile.Open(path, ZipArchiveMode.Read);
                WorkingArchive(zipArchive);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
            }
        }
    }

    private void WorkingArchive(ZipArchive zipArchive)
    {
        foreach (var item in zipArchive.Entries)
        {
            if (_pathsComplete.Add(item.FullName))
            {
                _mergeArchives.AddFile(item);
            }
        }
    }
    private void LoadCurrentArchive()
    {
        var list = _mergeArchives.GetArchivePaths();
        if (list == null || list.Count == 0)
        {
            return;
        }
        foreach (var item in list)
        {
            _pathsComplete.Add(item);
        }
    }
}