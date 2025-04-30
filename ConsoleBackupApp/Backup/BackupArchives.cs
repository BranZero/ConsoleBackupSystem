

using System.Collections.Concurrent;
using System.IO.Compression;
namespace ConsoleBackupApp.Backup;

/// <summary>
/// Consumer of paths that are to be backed up
/// </summary>
public class BackupArchives
{
    private const CompressionLevel COMPRESSION_LEVEL = CompressionLevel.SmallestSize;
    private ConcurrentQueue<string> _pathsToCopy;
    private ZipArchive? _zipArchive;
    private CancellationToken _cancellationToken;
    private Mutex _writeMutex;
    private Thread? _consumer;
    public readonly char Drive;

    public BackupArchives(char drive)
    {
        _writeMutex = new();

        _pathsToCopy = new ConcurrentQueue<string>();
        Drive = drive;
    }

    /// <summary>
    /// Starts the internal thread and create a zip file and accepts more parts to be copied now
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="cancellationToken"></param>
    public void Start(string folderPath, CancellationToken cancellationToken)
    {
        _writeMutex.WaitOne();
        _cancellationToken = cancellationToken;

        string zipFile = folderPath + Drive + ".zip";
        try
        {
            //Create the zip file
            _zipArchive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
            _consumer = new Thread(Consumer);
            _consumer.Start();
        }
        catch (Exception)
        {
            //TODO: log error and exit
            return;
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
    }

    public void InsertPath(string fullPath)
    {
        _pathsToCopy.Enqueue(fullPath);
    }

    private void Consumer()
    {
        while (!_cancellationToken.IsCancellationRequested)
        {
            if (!_pathsToCopy.TryDequeue(out string? fullPath) || fullPath is null)
            {
                continue;
            }
            string zipPath = fullPath[3..];
            AddFile(fullPath, zipPath);
        }
    }
    /// <summary>
    /// Add files to the queue be included in the archive
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="entryName"></param>
    /// <exception cref="NullReferenceException"></exception>
    private void AddFile(string filePath, string entryName)
    {
        _writeMutex.WaitOne();
        try
        {
            if (_zipArchive is null)
            {
                //TODO: Log Error from that class not running start yet
                throw new NullReferenceException();
            }
            _zipArchive.CreateEntryFromFile(filePath, entryName, COMPRESSION_LEVEL);
        }
        catch
        {
            //TODO: Log Error
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
    }

    public void Close()
    {
        try
        {
            _consumer?.Join();

            _writeMutex.WaitOne();
            _zipArchive?.Dispose();
        }
        catch
        {
            //TODO: Log Error
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
        _writeMutex.Close();
    }
}