

namespace ConsoleBackupApp.DataPaths;

public class DataPathFile
{
    /// <summary>
    /// File Format header 4bytes id, 2bytes version, 2bytes count
    /// 
    /// </summary>
    public const string DATA_PATH_FILE = @"Data.dpf";
    public const string DATA_PATH_FILE_TEMP = @"Data.dpf.tmp";
    private const string HEADER_ID = "DPF☺";
    private const ushort VERSION = 1;


    public static bool TryAddDataPath(DataPath dataPath)
    {
        DataPath[] dataPaths = GetDataPaths();
        //Check if exists
        foreach (var item in dataPaths)
        {
            if (item.CompareTo(dataPath) == 0)
            {
                return false;
            }
        }
        DataPath[] dataPathsNew = new DataPath[dataPaths.Length + 1];
        dataPaths.CopyTo(dataPathsNew, 0);
        dataPathsNew[dataPaths.Length] = dataPath;
        UpdateDataPaths(dataPathsNew);
        return true;
    }

    public static DataPath[] GetDataPaths()
    {
        using FileStream fs = new FileStream(DATA_PATH_FILE, FileMode.Open, FileAccess.Read, FileShare.Read);
        using BinaryReader reader = new BinaryReader(fs);

        //Header of File
        char[] header = reader.ReadChars(HEADER_ID.Length);
        if (!new string(header).Equals(HEADER_ID))
        {
            return []; //invalid data file
        }
        ushort version = reader.ReadUInt16();
        if (version != VERSION)
        {
            //Handle Older Versions
            return []; //invalid data file
        }
        ushort rows = reader.ReadUInt16();

        //Rows
        DataPath[] dataPaths = new DataPath[rows];
        for (int i = 0; i < rows; i++)
        {
            dataPaths[i] = new DataPath(reader);
        }
        return dataPaths;

    }
    private static void CreateDataFile()
    {
        try
        {
            File.Create(DATA_PATH_FILE_TEMP);
        }
        catch (Exception)
        {
            Console.WriteLine("Can't Create data file");
        }
    }

    private static void UpdateDataPaths(DataPath[] dataPaths)
    {
        CreateDataFile();
        using FileStream fs = new FileStream(DATA_PATH_FILE_TEMP, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using BinaryWriter writer = new BinaryWriter(fs);
        //Header
        writer.Write(System.Text.Encoding.UTF8.GetBytes(HEADER_ID));
        writer.Write(VERSION);
        writer.Write((ushort)dataPaths.Length);

        //Rows
        foreach (var item in dataPaths)
        {
            item.ToData(writer);
        }

        //Clean Up
        writer.Flush();
        writer.Close();
        writer.Dispose();

        fs.Close();
        fs.Dispose();

        RenameDataFile();
    }

    private static void RenameDataFile()
    {
        try
        {
            if (File.Exists(DATA_PATH_FILE))
                File.Delete(DATA_PATH_FILE);
            File.Move(DATA_PATH_FILE_TEMP, DATA_PATH_FILE);
        }
        catch (Exception)
        {
            throw;
        }
    }
}