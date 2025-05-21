namespace ConsoleBackupApp.Backup;

public struct BackupStat
{
    public long Size;
    public int FileCalls;
    public BackupStat(long fileSize = 0, int fileCalls = 0)
    {
        Size = fileSize;
        FileCalls = fileCalls;
    }
}