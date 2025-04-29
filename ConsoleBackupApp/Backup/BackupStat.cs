

namespace ConsoleBackupApp.Backup;
public struct BackupStat
{
    private ulong _size;
    private Mutex _mutexSize;
    private uint _fileCalls;
    private Mutex _mutexFileCalls;
    public BackupStat(ulong fileSize = 0, uint fileCalls = 0) {
        _mutexFileCalls = new Mutex();
        _mutexSize = new Mutex();
        _size = fileSize;
        _fileCalls = fileCalls;
    }

    public void AddFileCall()
    {
        _mutexFileCalls.WaitOne();
        _fileCalls++;
        _mutexFileCalls.ReleaseMutex();
    }

    public readonly uint GetFileCallCount()
    {
        return _fileCalls;
    }
}