
using ConsoleBackupApp.DataPaths;
using ConsoleBackupApp.PriorBackup;

namespace ConsoleBackupApp.Backup;
/// <summary>
/// Producer of file paths to be backed up
/// </summary>
public class BackupProcess
{
    private readonly BackupShared _sharedData;
    private CancellationToken _cancellationToken;
    private readonly PriorBackupPath[] _priorBackups;

    public BackupProcess(BackupShared backupShared, PriorBackupPath[] priorBackups)
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
        string filePath = dataPath.SourcePath;
        if (File.Exists(filePath))
        {
            if (dataPath.FileCopyMode == CopyMode.ForceCopy)
            {
                archive.InsertPath(filePath);
                return;
            }
            //None or AllOrNone
            if (!IsInPriorBackups(filePath))
            {
                archive.InsertPath(filePath);
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
            if (Directory.Exists(dataPath.SourcePath))
            {
                directories.Push(new DirectoryInfo(dataPath.SourcePath));
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
                if (IsInIgnorePaths(ignorePaths, currentDirectory.FullName))
                {
                    continue;
                }
                // queue all files in the current directory
                if (dataPath.FileCopyMode == CopyMode.ForceCopy)
                {
                    InsertAll(archive, ignorePaths, currentDirectory);
                }
                else if (dataPath.FileCopyMode == CopyMode.AllOrNone)
                {
                    InsertAllOrNone(archive, ignorePaths, currentDirectory);
                }
                else //None
                {
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

    private void InsertCheckEach(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (IsInIgnorePaths(ignorePaths, file.FullName))
            {
                continue;
            }
            if (!IsInPriorBackups(file.FullName))
            {
                archive.InsertPath(file.FullName);
            }
        }
    }
    /// <summary>
    /// Inserts all files or none for a given directory but ignores ones that match ignorePaths.
    /// </summary>
    private void InsertAllOrNone(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (IsInIgnorePaths(ignorePaths, file.FullName))
            {
                continue;
            }
            if (!IsInPriorBackups(file.FullName))
            {
                InsertAll(archive, ignorePaths, currentDirectory);
                return;
            }
        }
    }

    /// <summary>
    /// Inserts all files in a given directory but ignores ones that match ignorePaths and doesn't check prior backups.
    /// </summary>
    private void InsertAll(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (IsInIgnorePaths(ignorePaths, file.FullName))
            {
                continue;
            }

            archive.InsertPath(file.FullName);
        }
    }

    private bool IsInPriorBackups(string fullName)
    {
        //TODO: Check Prior Backups
        try
        {
            FileInfo fileInfo = new(fullName);
            for (int i = 0; i < _priorBackups.Length; i++)
            {
                //Go from when the folder was created which is before any files are writen to it.
                if (fileInfo.LastWriteTimeUtc >= _priorBackups[i].LastModified)
                {
                    //Modifed after last backup in list was created
                    return false;
                }
                else
                {
                    CheckPriorBackup(fullName, _priorBackups[i].FullPath);
                }
            }
        }
        catch (Exception)
        {
            //TODO: Log Error
            throw;
        }
        return false;
    }

    private static bool CheckPriorBackup(string fullName, string fullPath)
    {
        string zipFile = 
        if (File.Exists(zipFile))
        {
            
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool IsInIgnorePaths(HashSet<string> ignorePaths, string path)
    {

        return ignorePaths.Count != 0 && ignorePaths.Contains(path);
    }
}