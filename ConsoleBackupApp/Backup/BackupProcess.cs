
using System.IO.Compression;
using System.Security.Cryptography;
using ConsoleBackupApp.DataPaths;
using ConsoleBackupApp.Logging;
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
                Logger.Instance.Log(LogLevel.Error, $"Unable to retrieve archive for drive: {dataPath.Drive}");
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
                Logger.Instance.Log(LogLevel.Error, $"Unsupported PathType was passed in {dataPath.Type} for {dataPath.SourcePath}");
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
            Logger.Instance.Log(LogLevel.Warning, $"File no longer exists: {filePath}");
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
                Logger.Instance.Log(LogLevel.Error, "Log Error Source Directory Not Found");
                return;
            }

            while (directories.Count > 0)
            {
                DirectoryInfo currentDirectory = directories.Pop();
                //Ignore Paths
                if (IsInIgnorePaths(ignorePaths, currentDirectory.Name))
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
#region Directory CopyModes
    private void InsertCheckEach(ArchiveQueue archive, HashSet<string> ignorePaths, DirectoryInfo currentDirectory)
    {
        // queue all files in the current directory
        foreach (FileInfo file in currentDirectory.GetFiles())
        {
            //Ignore Paths
            if (IsInIgnorePaths(ignorePaths, file.Name))
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
            if (IsInIgnorePaths(ignorePaths, file.Name))
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
            if (IsInIgnorePaths(ignorePaths, file.Name))
            {
                continue;
            }

            archive.InsertPath(file.FullName);
        }
    }
#endregion

#region Prior Backups
    private bool IsInPriorBackups(string fullName)
    {
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
                    PriorResult priorResult = DoesFileMatchPriorBackup(fileInfo, _priorBackups[i].FullPath);
                    if (priorResult == PriorResult.Matched) return true;
                    if (priorResult == PriorResult.Changed) return false;
                    //else keep looking
                }
            }
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
            throw;
        }
        return false;
    }
    /// <summary>
    /// Determines whether a given file matches a prior backup by comparing its contents
    /// with the corresponding entry in a zip archive located in the prior backup path.
    /// </summary>
    /// <param name="fileInfo">File Path</param>
    /// <param name="priorbackupPath">Prior Backup Path</param>
    /// <returns></returns>
    private static PriorResult DoesFileMatchPriorBackup(FileInfo fileInfo, string priorbackupPath)
    {
        string zipFile = Path.Combine(priorbackupPath, fileInfo.FullName[0] + ".zip");
        if (File.Exists(zipFile))
        {
            using ZipArchive zipArchive = ZipFile.OpenRead(zipFile);
            ZipArchiveEntry? zipArchiveEntry = zipArchive.GetEntry(fileInfo.FullName[3..]);
            if (zipArchiveEntry == null)
            {
                return PriorResult.NotFound;
            }

            if (zipArchiveEntry.Length != fileInfo.Length)
            {
                return PriorResult.Changed;
            }

            //Check if the two files are the same
            using Stream zipReader = zipArchiveEntry.Open();
            using SHA256 sha256 = SHA256.Create();
            byte[] hashZip = sha256.ComputeHash(zipReader);

            using Stream fileReader = fileInfo.OpenRead();
            using SHA256 sha2562 = SHA256.Create();
            byte[] hashFile = sha2562.ComputeHash(fileReader);

            if(hashFile.SequenceEqual(hashZip))
            {
                return PriorResult.Matched;
            }
            else
            {
                return PriorResult.Changed;
            }
        }
        else
        {
            return PriorResult.NotFound;
        }
    }

    private static bool IsInIgnorePaths(HashSet<string> ignorePaths, string path)
    {
        return ignorePaths.Count != 0 && ignorePaths.Contains(path);
    }

    private enum PriorResult
    {
        Matched,
        Changed,
        NotFound,
    }
#endregion
}