namespace ConsoleBackupApp.Backup;

public struct BackupStat
{
    private ulong Size;
    private uint FileCalls;
    public BackupStat(ulong fileSize = 0, uint fileCalls = 0)
    {
        Size = fileSize;
        FileCalls = fileCalls;
    }
}