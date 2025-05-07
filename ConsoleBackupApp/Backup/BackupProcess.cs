
using System.Collections.Concurrent;
using ConsoleBackupApp.DataPaths;

namespace ConsoleBackupApp.Backup;
/// <summary>
/// Producer of file paths to be backed up
/// </summary>
public class BackupProcess
{
    private readonly BackupShared _sharedData;
    private CancellationToken _cancellationToken;
    private string[] _priorBackups;

    public BackupProcess(BackupShared backupShared, string[] priorBackups)
    {
        _sharedData = backupShared;
        _priorBackups = priorBackups;
    }
    public void Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        Process();
    }


    private void Process()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (!_sharedData.TryGetDataPath(out DataPath dataPath))
            {
                //none left
                return;
            }
            if (!_sharedData.TryGetArchive(dataPath.Drive, out ArchiveQueue? archive) || archive is null)
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
    private void Producer(ref DataPath dataPath, ArchiveQueue archive)
    {
        if (dataPath.Type == PathType.File)
        {
            if (File.Exists(dataPath.SourcePath))
            {
                archive.InsertPath(dataPath.SourcePath);
            }
            else
            {
                //TODO: Log Error
            }
            return;
        }
        //TODO: Ignore Paths

        //TODO: InPrior Backups


        Stack<DirectoryInfo> directories = new Stack<DirectoryInfo>();
        try
        {
            if (Directory.Exists(dataPath.SourcePath))
            {
                directories.Push(new DirectoryInfo(dataPath.SourcePath));
            }
            else
            {
                //TODO: Log Error
                return;
            }
        }
        catch (System.Exception)
        {
            throw;
        }

        while (directories.Count > 0)
        {
            DirectoryInfo currentDirectory = directories.Pop();
            try
            {
                // queue all files in the current directory
                foreach (var file in currentDirectory.GetFiles())
                {
                    archive.InsertPath(file.FullName);
                }

                // Push all subdirectories onto the stack
                foreach (var subDirectory in currentDirectory.GetDirectories())
                {
                    directories.Push(subDirectory);
                }
            }
            catch (Exception)
            {
                // TODO: Log Errors
                throw;
            }
        }
    }

    public void Close()
    {

    }
}