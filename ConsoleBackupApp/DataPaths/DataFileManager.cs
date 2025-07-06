using ConsoleBackupApp.Logging;

namespace ConsoleBackupApp.DataPaths;

public class DataFileManager
{
    public const string DATA_PATH_FILE = @"Data.dpf";
    public const string DATA_PATH_FILE_TEMP = @"Data.dpf.tmp";

    /// <summary>
    /// Try to add a path to the list of paths to be backedup
    /// </summary>
    /// <param name="dataPath"></param>
    /// <returns>Does it already exist in data file</returns>
    public static Result TryAddDataPath(DataPath dataPath)
    {
        DataPath[] dataPaths = GetDataPaths();
        string sourcePath = dataPath.SourcePath;
        //Check if exists
        foreach (var item in dataPaths)
        {
            string fullPath = item.SourcePath;
            if (fullPath.StartsWith(sourcePath) || sourcePath.StartsWith(fullPath))
            {
                return new(ResultType.SubPath_Or_SamePath, $"The Path entered is part of or same as: {fullPath}");
            }
        }
        DataPath[] dataPathsNew = new DataPath[dataPaths.Length + 1];
        dataPaths.CopyTo(dataPathsNew, 0);
        dataPathsNew[dataPaths.Length] = dataPath;
        byte[] data = DPF.CreateFile(dataPathsNew);
        WriteDataFile(data);
        return new(ResultType.Success);
    }
    /// <summary>
    /// Removes the first matching datapath from the list ignores Prior Paths
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Result TryRemoveDataPath(string s)
    {
        List<DataPath> dataPaths = [.. GetDataPaths()];
        for (int i = 0; i < dataPaths.Count; i++)
        {
            string sourcePath = dataPaths[i].SourcePath;
            if (sourcePath.Length == s.Length || sourcePath.Length == s.Length + 1)
            {
                if (s[0] == dataPaths[i].Drive && sourcePath.StartsWith(s))
                {
                    //Found
                    dataPaths.RemoveAt(i);
                    byte[] data = DPF.CreateFile(dataPaths.ToArray());
                    WriteDataFile(data);
                    return new(ResultType.Success);
                }
            }
        }
        return new(ResultType.Not_Found);
    }
    /// <summary>
    /// Updates an existing DataPath by sourcePath. Allows changing CopyMode and IgnorePaths.
    /// </summary>
    /// <param name="sourcePath">The source path to update</param>
    /// <param name="copyMode">Optional new CopyMode</param>
    /// <param name="ignorePaths">Optional new ignore paths</param>
    /// <returns>True if updated, false if not found</returns>
    public static bool TryUpdateDataPathCopyMode(string sourcePath, CopyMode copyMode)
    {
        DataPath[] dataPaths = GetDataPaths();
        int index = Array.FindIndex(dataPaths, dp => (
            dp.SourcePath.Equals(sourcePath, StringComparison.OrdinalIgnoreCase)
        ));
        if (index == -1)
        {
            return false;
        }

        DataPath old = dataPaths[index];
        dataPaths[index] = new(old.Type, copyMode, old.SourcePath, old.IgnorePaths);

        byte[] data = DPF.CreateFile(dataPaths);
        WriteDataFile(data);
        return true;
    }

    /// <summary>
    /// Attempts to remove one or more ignore paths from the specified source path's ignore list.
    /// </summary>
    /// <param name="sourcePath">The source path from which to remove ignore paths.</param>
    /// <param name="ignorePaths">An array of ignore paths to remove.</param>
    /// <returns>
    /// <see cref="ResultType.Success"/> if some specified ignore paths were successfully removed;<br />
    /// <see cref="ResultType.Path_Not_Found"/> if the source path does not exist;<br />
    /// <see cref="ResultType.Empty"/> if the ignore list is empty.<br />
    /// <see cref="ResultType.Empty"/> nothing was changed.<br />
    /// </returns>
    public static Result TryRemoveIgnorePaths(string sourcePath, string[] ignorePaths)
    {
        DataPath[] dataPaths = GetDataPaths();
        int index = Array.FindIndex(dataPaths, dp => dp.SourcePath.Equals(sourcePath, StringComparison.Ordinal));
        if (index == -1)
        {
            return new(ResultType.Path_Not_Found, $"DataPath Not Found: {sourcePath}");
        }

        DataPath old = dataPaths[index];
        if (old.IgnorePaths == null)
        {
            return new(ResultType.Empty, $"DataPath IgnorePaths is empty: {sourcePath}"); ;
        }

        List<string> removedPaths = [];
        HashSet<string> ignoreSet = [.. old.IgnorePaths];
        foreach (string ignorePath in ignorePaths)
        {
            if (!ignoreSet.Remove(ignorePath))
            {
                removedPaths.Add(ignorePath);
            }
            // No effect if IgnorePath doesn't exist
        }

        if (removedPaths.Count == ignorePaths.Length)
        {
            //nothing added
            return new(ResultType.No_Change, "Nothing Removed");
        }

        dataPaths[index] = new(old.Type, old.FileCopyMode, old.SourcePath, [.. ignoreSet]);
        byte[] data = DPF.CreateFile(dataPaths);
        WriteDataFile(data);
        return new(ResultType.Success, removedPaths.Count > 0 ? $"Following IgnorePaths didn't exist: {string.Join(", ", removedPaths)}." : null);
    }

    /// <summary>
    /// Attempts to add one or more ignore paths from the specified source path's ignore list.
    /// </summary>
    /// <param name="sourcePath">The source path from which to add ignore paths.</param>
    /// <param name="ignorePaths">An array of ignore paths to add.</param>
    /// <returns>
    /// <see cref="ResultType.Success"/> if some specified ignore paths were successfully added;<br />
    /// <see cref="ResultType.Path_Not_Found"/> if the source path does not exist;<br />
    /// <see cref="ResultType.Empty"/> if the ignore list is empty.<br />
    /// <see cref="ResultType.Empty"/> nothing was changed.<br />
    /// </returns>
    public static Result TryAddIgnorePaths(string sourcePath, string[] ignorePaths)
    {
        DataPath[] dataPaths = GetDataPaths();
        int index = Array.FindIndex(dataPaths, dp => dp.SourcePath.Equals(sourcePath, StringComparison.Ordinal));
        if (index == -1)
        {
            return new(ResultType.Path_Not_Found, $"DataPath Not Found: {sourcePath}");
        }

        List<string> addPaths = [];
        DataPath old = dataPaths[index];
        HashSet<string> ignoreSet = [.. old.IgnorePaths ?? []];
        foreach (string ignorePath in ignorePaths)
        {
            if (!ignoreSet.Add(ignorePath))
            {
                addPaths.Add(ignorePath);
            }
            // No effect if IgnorePath already exists
        }

        if (addPaths.Count == ignorePaths.Length)
        {
            //nothing added
            return new(ResultType.No_Change, "Nothing Added");
        }

        dataPaths[index] = new(old.Type, old.FileCopyMode, old.SourcePath, [.. ignoreSet]);
        byte[] data = DPF.CreateFile(dataPaths);
        WriteDataFile(data);
        return new(ResultType.Success, addPaths.Count > 0 ? $"Following IgnorePaths already exist: {string.Join(", ", addPaths)}." : null);
    }

    public static DataPath[] GetDataPaths()
    {
        byte[] data = ReadDataFile();
        return DPF.GetDataPaths(data);
    }

    private static void WriteDataFile(byte[] buffer)
    {
        try
        {
            File.WriteAllBytes(DATA_PATH_FILE_TEMP, buffer);
            if (File.Exists(DATA_PATH_FILE))
            {
                File.Delete(DATA_PATH_FILE);
            }
            File.Move(DATA_PATH_FILE_TEMP, DATA_PATH_FILE);
            File.Delete(DATA_PATH_FILE_TEMP);
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
        }
    }
    internal static byte[] ReadDataFile()
    {
        try
        {
            if (File.Exists(DATA_PATH_FILE))
            {
                return File.ReadAllBytes(DATA_PATH_FILE);
            }
            else
            {
                return [];
            }
        }
        catch (Exception e)
        {
            Logger.Instance.Log(LogLevel.Error, e.Message + "\n" + e.StackTrace);
            return [];
        }
    }
}