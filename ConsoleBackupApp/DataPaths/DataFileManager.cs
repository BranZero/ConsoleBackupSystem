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
    public static bool TryAddDataPath(DataPath dataPath)
    {
        DataPath[] dataPaths = GetDataPaths();
        string sourcePath = dataPath.SourcePath;
        //Check if exists
        foreach (var item in dataPaths)
        {
            string fullPath = item.SourcePath;
            if (fullPath.StartsWith(sourcePath) || sourcePath.StartsWith(fullPath))
            {
                return false;
            }
        }
        DataPath[] dataPathsNew = new DataPath[dataPaths.Length + 1];
        dataPaths.CopyTo(dataPathsNew, 0);
        dataPathsNew[dataPaths.Length] = dataPath;
        byte[] data = DPF.CreateFile(dataPathsNew);
        WriteDataFile(data);
        return true;
    }
    /// <summary>
    /// Removes the first matching datapath from the list ignores Prior Paths
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool TryRemoveDataPath(string s)
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
                    return true;
                }
            }
        }
        return false;
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
            dp.SourcePath.Equals(sourcePath, StringComparison.Ordinal)
        ));
        if (index == -1)
        {
            return false;
        }
        //Continue working on
        DataPath old = dataPaths[index];
        CopyMode newCopyMode = copyMode;

        byte[] data = DPF.CreateFile(dataPaths);
        WriteDataFile(data);
        return true;
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