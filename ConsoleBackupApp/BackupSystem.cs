


public class BackupSystem
{
    /// <summary>
    /// Add all priorbackups in backup folderPath
    /// </summary>
    /// <param name="priorBackups"></param>
    /// <param name="folderPath"></param>
    /// <returns>true if an error occured during this process</returns>
    public static bool TryFindPriorBackupPathsInDirectory(string backupDir, out List<string> priorBackupPaths)
    {
        //Directory has already been check
        priorBackupPaths = new List<string>();
        try
        {
            DirectoryInfo directoryInfo = new(backupDir);
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            foreach (DirectoryInfo dir in directoryInfos)
            {
                if (!IsPriorBackup(dir)) continue;
                string path = dir.FullName;
                if (path[^1] != Path.DirectorySeparatorChar)
                {
                    path += Path.DirectorySeparatorChar;
                }
                priorBackupPaths.Add(path);
            }
        }
        catch (Exception)
        {
            //Log Error
            throw;
        }

        return priorBackupPaths.Count != 0;
    }

    internal static bool IsPriorBackup(DirectoryInfo dir)
    {
        if (!dir.Name.StartsWith("Backup_")) return false;
        if (!char.IsNumber(dir.Name[^1])) return false;
        try
        {
            DirectoryInfo[] directoryInfos = dir.GetDirectories();
            foreach (var subDir in directoryInfos)
            {
                if (subDir.Name.Length != 1) return false;
            }
        }
        catch (System.Exception)
        {
            //Log Error
            throw;
        }
        return true;
    }

    internal static string FindBackupPathName(string backupDir)
    {
        DateTime dateTime = DateTime.Now;
        backupDir += $"Backup_{dateTime.Month}_{dateTime.Day}_{dateTime.Year}";

        //Add number if it already exists
        string temp = "";
        ushort count = 1;
        while (Directory.Exists(backupDir + temp))
        {
            temp = $"_{count}"; //Check if exists then increment number tile works
            count++;
        }
        return backupDir + temp + Path.DirectorySeparatorChar;
    }
}