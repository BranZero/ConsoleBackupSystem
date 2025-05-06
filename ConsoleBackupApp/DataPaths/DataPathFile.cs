

using System.Threading.Tasks;

namespace ConsoleBackupApp.DataPaths;

public class DataPathFile
{
    public const string DATA_PATH_FILE = @"Data.dpf";
    public const string DATA_PATH_FILE_TEMP = @"Data.dpf.tmp";
    public static readonly string HEADER_ID = "DPF" + (char)1;
    public const ushort VERSION = 1;
    public const ushort HEADER_SIZE = 12;

    /// <summary>
    /// Try to add a path to the list of paths to be backedup
    /// </summary>
    /// <param name="dataPath"></param>
    /// <returns>Does it already exist in data file</returns>
    public static bool TryAddDataPath(DataPath dataPath)
    {
        DataPath[] dataPaths = GetDataPaths(ReadDataFile());
        string sourcePath = dataPath.GetSourcePath();
        //Check if exists
        foreach (var item in dataPaths)
        {
            string fullPath = item.GetSourcePath();
            if (fullPath.StartsWith(sourcePath) || sourcePath.StartsWith(fullPath))
            {
                return false;
            }
        }
        DataPath[] dataPathsNew = new DataPath[dataPaths.Length + 1];
        dataPaths.CopyTo(dataPathsNew, 0);
        dataPathsNew[dataPaths.Length] = dataPath;
        byte[] data = CreateDpfFile(dataPathsNew);
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
        List<DataPath> dataPaths = new(GetDataPaths(ReadDataFile()));
        for (int i = 0; i < dataPaths.Count; i++)
        {
            string sourcePath = dataPaths[i].GetSourcePath();
            if(sourcePath.Length == s.Length || sourcePath.Length == s.Length+1){
                if(s[0] == dataPaths[i].Drive && sourcePath.StartsWith(s)){
                    //Found
                    dataPaths.RemoveAt(i);
                    byte[] data = CreateDpfFile(dataPaths.ToArray());
                    WriteDataFile(data);
                    return true;
                }
            }
        }
        return false;
    }

    public static DataPath[] GetDataPaths()
    {
        byte[] data = ReadDataFile();
        return GetDataPaths(data);
    }

    public static DataPath[] GetDataPaths(byte[] buffer)
    {
        using MemoryStream ms = new MemoryStream(buffer);
        using BinaryReader reader = new BinaryReader(ms);

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
        reader.ReadBytes(4);//reserved bytes

        //Rows
        DataPath[] dataPaths = new DataPath[rows];
        for (int i = 0; i < rows; i++)
        {
            dataPaths[i] = new DataPath(reader);
        }
        return dataPaths;
    }
    /// <summary>
    /// Creates the file data for the .dpf file format<br/>
    /// Header 4bytes id, 2bytes version, 2bytes count, 4Bytes reserved<br/>
    /// Data for each DataPath<br/>
    /// format is 1byte Drive, 1Byte Type, 1Byte IgnorePath Count, 2Bytes n Length, nBytes Path, repeating Ignore Paths(Length 2Bytes m Length, mBytes Path Length)
    /// </summary>
    /// <param name="dataPaths"></param>
    /// <returns></returns>
    public static byte[] CreateDpfFile(DataPath[] dataPaths)
    {
        //Size of Header + length of all rows
        int size = HEADER_SIZE;
        foreach (DataPath dataPath in dataPaths)
        {
            size += dataPath.ToDataRowSize();
        }

        byte[] buffer = new byte[size];
        using MemoryStream ms = new MemoryStream(buffer);
        using BinaryWriter writer = new BinaryWriter(ms);
        //Header
        writer.Write(System.Text.Encoding.UTF8.GetBytes(HEADER_ID));
        writer.Write(VERSION);
        writer.Write((ushort)dataPaths.Length);
        writer.Write(uint.MinValue);//reserved bytes

        //Rows
        foreach (var item in dataPaths)
        {
            item.ToData(writer);
        }

        //Clean Up
        writer.Flush();
        writer.Close();
        writer.Dispose();

        ms.Close();
        ms.Dispose();

        return buffer;
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
        catch (Exception)
        {
            throw;//log errors
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
        catch (Exception)
        {
            throw;//log errors
        }
    }
}