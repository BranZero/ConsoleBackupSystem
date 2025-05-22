namespace ConsoleBackupApp.Backup;

public struct BackupStat
{
    private const long MEGABYTE_TO_BYTE = 1_048_576;
    public long Size;
    public int FileCalls;
    public BackupStat(long fileSize = 0, int fileCalls = 0)
    {
        Size = fileSize;
        FileCalls = fileCalls;
    }

    public static BackupStat operator +(BackupStat a, BackupStat b)
    {
        return new BackupStat(a.Size + b.Size, a.FileCalls + b.FileCalls);
    }

    public readonly long GetSizeInMegaBytes()
    {
        return Size / MEGABYTE_TO_BYTE;
    }
}