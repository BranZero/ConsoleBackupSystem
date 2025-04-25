

using System.Threading.Tasks;

namespace ConsoleBackupApp.DataPaths;

public class DataPathFile
{
    public const string DATA_PATH_FILE = @"Data.dpf";
    public const string DATA_PATH_FILE_TEMP = @"Data.dpf.tmp";
    private static readonly string HEADER_ID = "DPF" + (char)1;
    private const ushort VERSION = 1;
    private const ushort HEADER_SIZE = 8;


    public static bool TryAddDataPath(DataPath dataPath)
    {
        DataPath[] dataPaths = GetDataPaths(ReadDataFile());
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
        byte[] ms = CreateDpfFile(dataPathsNew);
        return true;
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
    /// Header 4bytes id, 2bytes version, 2bytes count<br/>
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
            File.Move(DATA_PATH_FILE_TEMP, DATA_PATH_FILE);
            File.Delete(DATA_PATH_FILE_TEMP);
        }
        catch (Exception)
        {
            throw;//log errors
        }
    }
    private static byte[] ReadDataFile()
    {
        try
        {
            return File.ReadAllBytes(DATA_PATH_FILE);
        }
        catch (Exception)
        {
            throw;//log errors
        }
    }
}