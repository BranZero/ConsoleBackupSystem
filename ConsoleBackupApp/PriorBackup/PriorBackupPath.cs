namespace ConsoleBackupApp.PriorBackup;

public readonly struct PriorBackupPath : IComparable<PriorBackupPath>
{
    public readonly DateTime LastModified;
    public readonly string FullPath;

    public PriorBackupPath(DateTime createdDate, string fullPath)
    {
        LastModified = createdDate;
        if (fullPath[^1] != Path.DirectorySeparatorChar)
        {
            fullPath += Path.DirectorySeparatorChar;
        }
        FullPath = fullPath;
    }

    public static bool TryGetPriorBackup(string fullPath, out PriorBackupPath priorBackupPath)
    {
        priorBackupPath = default;
        if (!Directory.Exists(fullPath)) return false;

        try
        {
            DirectoryInfo directoryInfo = new(fullPath);

            string[] parts = directoryInfo.Name.Split('_');
            if (parts.Length != 4 && parts.Length != 5) return false;
            if (parts[0] != "Backup") return false;

            DateTime dateTime = directoryInfo.CreationTimeUtc;
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                if (fileInfo.LastWriteTimeUtc > dateTime)
                {
                    dateTime = fileInfo.LastWriteTimeUtc;
                }
            }
            priorBackupPath = new(dateTime, directoryInfo.FullName);
            return true;
        }
        catch (Exception)
        {
            //TODO: Log Error
            throw;
        }
    }

    /// <summary>
    /// Decending Order for comparing DataTime so that most recent is at the top
    /// </summary>
    public readonly int CompareTo(PriorBackupPath other)
    {
        return other.LastModified.CompareTo(LastModified);
    }
}
