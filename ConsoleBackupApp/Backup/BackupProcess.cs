
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
            if (dataPath.Type == PathType.File)
            {
                ProducerFile(ref dataPath, archive);
            }
            else if (dataPath.Type == PathType.Directory)
            {
                ProducerDirectory(ref dataPath, archive);
            }
            else
            {
                //TODO: Log Error
                continue;
            }
        }
    }
    private void ProducerFile(ref DataPath dataPath, ArchiveQueue archive)
    {
        if (File.Exists(dataPath.GetSourcePath()))
        {
            if (dataPath.FileCopyMode == CopyMode.ForceCopy)
            {
                archive.InsertPath(dataPath.GetSourcePath());
                return;
            }
            //None or AllOrNone
            if (!IsInPriorBackups())
            {
                //TODO: Log Error
                archive.InsertPath(dataPath.GetSourcePath());
            }
        }
        else
        {
            //TODO: Log Error
        }
    }

    /// <summary>
    /// Producers role is to traverse the main systems file tree and get paths to files in that DataPath tree
    /// </summary>
    private void ProducerDirectory(ref DataPath dataPath, ArchiveQueue archive)
    {
        //Ignore Paths
        HashSet<string> ignorePaths = dataPath.GetIgnorePaths();

        Stack<DirectoryInfo> directories = new Stack<DirectoryInfo>();
        try
        {
            if (Directory.Exists(dataPath.GetSourcePath()))
            {
                directories.Push(new DirectoryInfo(dataPath.GetSourcePath()));
            }
            else
            {
                //TODO: Log Error Source Directory Not Found
                return;
            }

            while (directories.Count > 0)
            {
                DirectoryInfo currentDirectory = directories.Pop();
                //Ignore Paths
                if (ignorePaths.Count != 0)
                {
                    ignorePaths.Contains(currentDirectory.FullName);
                    continue;
                }
                // queue all files in the current directory
                foreach (var file in currentDirectory.GetFiles())
                if (dataPath.FileCopyMode == CopyMode.ForceCopy)
                {
                    InsertAll(archive, ignorePaths, currentDirectory);
                }
                else if (dataPath.FileCopyMode == CopyMode.AllOrNone)
                {
                    InsertAllOrNone(archive, ignorePaths, currentDirectory);
                }
                else //None
                    InsertCheckEach(archive, ignorePaths, currentDirectory);
                }

                // Push all subdirectories onto the stack
                foreach (var subDirectory in currentDirectory.GetDirectories())
                {
                    directories.Push(subDirectory);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (ignorePaths.Count != 0)
            {
                ignorePaths.Contains(file.FullName);
                continue;
            }
            {
                archive.InsertPath(file.FullName);
            }
    /// </summary>
    private void InsertAllOrNone(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
        // queue all files in the current directory
        {
            //Ignore Paths
            if (ignorePaths.Count != 0)
            {
                ignorePaths.Contains(file.FullName);
                continue;
            }
            if (!IsInPriorBackups())
            {
            }
        }
    }

    /// <summary>
    /// Inserts all files in a given directory but ignores ones that match ignorePaths and doesn't check prior backups.
    private static void InsertAll(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (ignorePaths.Count != 0)
            {
                ignorePaths.Contains(file.FullName);
            }

            archive.InsertPath(file.FullName);
        }
    }

    private bool IsInPriorBackups()
    {
    }
}