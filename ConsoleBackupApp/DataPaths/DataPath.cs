

namespace ConsoleBackupApp.DataPaths;
public struct DataPath : IComparable<DataPath>
{
    public char Drive;
    public char Type;
    public string Path;
    public string[] IgnorePaths;
    public DataPath(BinaryReader reader)
    {
        Drive = reader.ReadChar();
        Type = reader.ReadChar();
        int ignorePathCount = reader.ReadByte();
        IgnorePaths = new string[ignorePathCount];
        int length = reader.ReadUInt16();
        char[] path = reader.ReadChars(length);
        Path = new(path);


        for (int i = 0; i < ignorePathCount; i++)
        {
            length = reader.ReadUInt16();
            char[] ignorePath = reader.ReadChars(length);
            IgnorePaths[i] = new(ignorePath);
        }
    }
    public readonly int CompareTo(DataPath other)
    {
        if(Drive - other.Drive != 0)
        {
            return Drive - other.Drive;
        }
        return Path.CompareTo(other.Path);
    }

    /// <summary>
    /// Format is 1byte Drive, 1Byte Type, 1Byte IgnorePath Count, 2Bytes n Length, nBytes Path, repeating Ignore Paths(Length 2Bytes m Length, mBytes Path Length)
    /// </summary>
    /// <returns></returns>
    public readonly void ToData(BinaryWriter writer)
    {
        writer.Write(Drive);
        writer.Write(Type);
        writer.Write((byte)IgnorePaths.Length);
        writer.Write((ushort)Path.Length);
        writer.Write(System.Text.Encoding.UTF8.GetBytes(Path));

        foreach (var item in IgnorePaths)
        {
            writer.Write((ushort)item.Length);
            writer.Write(System.Text.Encoding.UTF8.GetBytes(item));
        }
    }
}