
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
/// <summary>
/// Producer of file paths to be backed up
/// </summary>
public class BackupProcess
{
    private readonly ConcurrentQueue<DataPath> _dataPaths;
    private readonly BackupArchives[] _backupArchives;
    private CancellationToken _cancellationToken;
    private Thread? _producer;

    public BackupProcess(ref ConcurrentQueue<DataPath> dataPaths, ref BackupArchives[] backupArchives)
    {
        _dataPaths = dataPaths;
        _backupArchives = backupArchives;
    }
    public void Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _producer = new Thread(StartProducing);
        _producer.Start();
    }

    private void StartProducing()
    {
        while (!_dataPaths.IsEmpty)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                //exit early
                return;
            }
            if (!_dataPaths.TryDequeue(out DataPath dataPath))
            {
                //Retry
                continue;
            }
            if (!TryGetArchive(dataPath.Drive, out BackupArchives archive))
            {
                //TODO: Log Error
                return;
            }
            //Process DataPath
            Producer(ref dataPath, archive);
        }
    }
    /// <summary>
    /// Producers role is to traverse the main systems file tree and get paths to files in that DataPath tree
    /// </summary>
    private void Producer(ref DataPath dataPath, BackupArchives archive)
    {
        //TODO: Ignore Paths

        //TODO: InPrior Backups

        Stack<DirectoryInfo> directories = new();
        while (directories.Count > 0)
        {
            
        }
    }

    private bool TryGetArchive(char drive, out BackupArchives archive)
    {
        foreach (BackupArchives backupArchive in _backupArchives)
        {
            if (drive == backupArchive.Drive)
            {
                archive = backupArchive;
                return true;
            }
        }
        //TODO: Log Critial
        archive = null;
        return false;
    }
    public async Task Close()
    {
        _producer?.Join();
    }
}