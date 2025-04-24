

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
        CreateDataFile();
        DataPath[] dataPaths = GetDataPaths();
        //Check if exists
        foreach (var item in dataPaths)
        {
            if (item.CompareTo(dataPath) == 0)
            {
                return false;
            }
        }

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

}