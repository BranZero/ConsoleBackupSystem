

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
namespace ConsoleBackupApp.Backup;

/// <summary>
/// Consumer of paths that are to be backed up
/// </summary>
public class BackupArchives
{
    private const CompressionLevel COMPRESSION_LEVEL = CompressionLevel.SmallestSize;
    private Queue<string> _pathsToCopy;
    private ZipArchive? _zipArchive;
    private CancellationToken _cancellationToken;
    private Semaphore _semaphore;
    private Mutex _mutex;
    private Thread? _consumer;
    public readonly char Drive;

    public BackupArchives(string folderPath, char drive)
    {
        _mutex = new();
        _semaphore = new(0,10);
        _pathsToCopy = new Queue<string>();
        Drive = drive;
    }

    private void AddFile(string filePath, string entryName)
    {
        _mutex.WaitOne();
        try
        {
            _zipArchive.CreateEntryFromFile(filePath, entryName, COMPRESSION_LEVEL);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public void Close()
    {
        _mutex.WaitOne();
        try
        {
            _zipArchive.Dispose();
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }
}