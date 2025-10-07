using System.IO.Compression;
using ConsoleBackupApp.Logging;

public class MergeArchives
{
    private const CompressionLevel COMPRESSION_LEVEL = CompressionLevel.SmallestSize;
    private ZipArchive? _zipArchive;
    private Mutex _writeMutex;
    private readonly string _zipFilePath;

    public MergeArchives(char drive, string folderPath)
    {
        _writeMutex = new();
        _zipFilePath = folderPath + drive + ".zip";
    }
    public void Start()
    {
        _writeMutex.WaitOne();
        try
        {
            //Create the zip file
            _zipArchive = ZipFile.Open(_zipFilePath, ZipArchiveMode.Update);
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
            return;
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
    }
    public void AddFile(ZipArchiveEntry entry)
    {
        _writeMutex.WaitOne();
        try
        {
            if (_zipArchive is null)
            {
                Logger.Instance.Log(LogLevel.Error, "Attempted to start archive process before running start.");
                return;
            }
            // Copy entry contents and metadata
            var outEntry = _zipArchive.CreateEntry(entry.FullName, COMPRESSION_LEVEL);
            using (var inStream = entry.Open())
            using (var outStream = outEntry.Open())
            {
                inStream.CopyTo(outStream);
            }
            outEntry.LastWriteTime = entry.LastWriteTime;
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
    }

    public List<string>? GetArchivePaths()
    {
        _writeMutex.WaitOne();
        if (_zipArchive is null)
        {
            Logger.Instance.Log(LogLevel.Error, "Attempted to access archive before running start.");
            return null;
        }
        List<string> list = [];
        try
        {
            list = _zipArchive.Entries.Select(entry => entry.FullName).ToList();
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
        return list;
    }

    public void Close()
    {
        try
        {
            _writeMutex.WaitOne();
            _zipArchive?.Dispose();
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
        }
        finally
        {
            _writeMutex.ReleaseMutex();
        }
        _writeMutex.Close();
    }
}