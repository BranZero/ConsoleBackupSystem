
using System.Text;

namespace ConsoleBackupApp.DataPaths;
public class DPF
{
    public static readonly string HEADER_ID = "DPF" + (char)1;
    public const ushort VERSION = 1;
    public const ushort HEADER_SIZE = 12;
    public const int ROW_HEADER_SIZE = 5;
    public const int IGNORE_HEADER_SIZE = 2;

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
            dataPaths[i] = DpfToDataPath(reader);
        }
        return dataPaths;
    }
    /// <summary>
    /// Creates the file data for the .dpf file format<br/>
    /// Header 4bytes id, 2bytes version, 2bytes count, 4Bytes reserved<br/>
    /// Data for each DataPath<br/>
    /// format is 1Byte FileType, 1Byte CopyMode, 1Byte IgnorePath Count, 2Bytes n Length, nBytes Path, repeating Ignore Paths(Length 2Bytes m Length, mBytes Path Length)
    /// </summary>
    /// <param name="dataPaths"></param>
    /// <returns></returns>
    public static byte[] CreateFile(DataPath[] dataPaths)
    {
        //Size of Header + length of all rows
        int size = HEADER_SIZE;
        foreach (DataPath dataPath in dataPaths)
        {
            size += ToDataRowSize(dataPath);
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
        foreach (DataPath dataPath in dataPaths)
        {
            DataPathToDpf(writer, dataPath);
        }

        //Clean Up
        writer.Flush();
        writer.Close();
        writer.Dispose();

        ms.Close();
        ms.Dispose();

        return buffer;
    }

    /// <summary>
    /// Format is 1Byte FileType, 1Byte CopyMode, 1Byte IgnorePath Count, 2Bytes n Length, nBytes Path, repeating Ignore Paths(Length 2Bytes m Length, mBytes Path Length)
    /// </summary>
    /// <returns></returns>
    public static void DataPathToDpf(BinaryWriter writer, DataPath dataPath)
    {
        //header
        writer.Write((byte)dataPath.Type);
        writer.Write((byte)dataPath.FileCopyMode);
        writer.Write((byte)(dataPath.IgnorePaths?.Length ?? 0));

        //n part
        byte[] sourceBytes = Encoding.Unicode.GetBytes(dataPath.SourcePath);
        writer.Write((ushort)sourceBytes.Length);
        writer.Write(sourceBytes);

        //m part
        if (dataPath.IgnorePaths == null) return;
        foreach (var item in dataPath.IgnorePaths)
        {
            byte[] ignoreBytes = Encoding.Unicode.GetBytes(item);
            writer.Write((ushort)ignoreBytes.Length);
            writer.Write(ignoreBytes);
        }
    }

    public static DataPath DpfToDataPath(BinaryReader reader)
    {
        //header
        PathType pathType = PathTypeExtensions.FromChar(reader.ReadChar());
        CopyMode copyMode = CopyModeExtensions.FromByte(reader.ReadByte());
        int ignorePathCount = reader.ReadByte();

        //n part
        int length = reader.ReadUInt16();
        byte[] pathBytes = reader.ReadBytes(length);
        string sourcePath = Encoding.Unicode.GetString(pathBytes);

        //m part
        if (ignorePathCount == 0)
        {
            return new(pathType, copyMode, sourcePath, null);
        }

        string[] ignorePaths = new string[ignorePathCount];
        for (int i = 0; i < ignorePathCount; i++)
        {
            length = reader.ReadUInt16();
            byte[] ignoreBytes = reader.ReadBytes(length);
            ignorePaths[i] = Encoding.Unicode.GetString(ignoreBytes);
        }
        return new(pathType, copyMode, sourcePath, ignorePaths);
    }

    public static int ToDataRowSize(DataPath dataPath)
    {
        int size = ROW_HEADER_SIZE + dataPath.SourcePath.Length * sizeof(char);
        if (dataPath.IgnorePaths == null) return size;
        foreach (string ignorePath in dataPath.IgnorePaths)
        {
            size += IGNORE_HEADER_SIZE + ignorePath.Length * sizeof(char);
        }
        return size;
    }
}