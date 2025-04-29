

using System.IO.Compression;
namespace ConsoleBackupApp.Backup;

/// <summary>
/// Consumer of 
/// </summary>
public class BackupArchives
{
    private ZipArchive _zipArchive;
    private Mutex _mutex;

    public BackupArchives(string folderPath, char drive)
    {
        _mutex = new();
        _mutex.WaitOne();
        string zipFile = folderPath + drive + ".zip";
        try
        {
            _zipArchive = ZipFile.Open(zipFile, ZipArchiveMode.Create);
        }
        catch (Exception)
        {
            //Log error
            throw;
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public void AddFile(string filePath, string entryName)
    {
        _mutex.WaitOne();
        try
        {
            _zipArchive.CreateEntryFromFile(filePath, entryName, CompressionLevel.SmallestSize);
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