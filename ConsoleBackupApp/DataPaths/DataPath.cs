
namespace ConsoleBackupApp.DataPaths;
public struct DataPath : IComparable<DataPath>
{
    public char Drive;
    public PathType Type;
    public string SourcePath;
    public string[]? IgnorePaths;

    public DataPath(BinaryReader reader)
    {
        Drive = reader.ReadChar();
        Type = PathTypeExtensions.FromChar(reader.ReadChar());
        int ignorePathCount = reader.ReadByte();
        IgnorePaths = new string[ignorePathCount];
        int length = reader.ReadUInt16();
        char[] path = reader.ReadChars(length);
        SourcePath = new(path);


        for (int i = 0; i < ignorePathCount; i++)
        {
            length = reader.ReadUInt16();
            char[] ignorePath = reader.ReadChars(length);
            IgnorePaths[i] = new(ignorePath);
        }
    }

    public static bool Init(PathType pathType, ReadOnlySpan<string> args, out DataPath dataPath)
    {
        dataPath = new DataPath();
        if (args[0].Length < 4) return false;
        if (!Char.IsAsciiLetterUpper(args[0][0])) return false;
        dataPath.Drive = args[0][0];
        dataPath.Type = pathType;
        dataPath.SourcePath = args[0][3..];

        dataPath.IgnorePaths = new string[args.Length - 1];
        for (int i = 1; i < args.Length; i++)
        {
            dataPath.IgnorePaths[i-1] = args[i];
        }
        return true;
    }
    public readonly int CompareTo(DataPath other)
    {
        if (Drive - other.Drive != 0)
        {
            return Drive - other.Drive;
        }
        return SourcePath.CompareTo(other.SourcePath);
    }

    /// <summary>
    /// Format is 1byte Drive, 1Byte Type, 1Byte IgnorePath Count, 2Bytes n Length, nBytes Path, repeating Ignore Paths(Length 2Bytes m Length, mBytes Path Length)
    /// </summary>
    /// <returns></returns>
    public readonly void ToData(BinaryWriter writer)
    {
        writer.Write(Drive);
        writer.Write((byte)Type);
        writer.Write((byte)(IgnorePaths?.Length ?? 0));
        writer.Write((ushort)SourcePath.Length);
        writer.Write(System.Text.Encoding.UTF8.GetBytes(SourcePath));

        if (IgnorePaths == null) return;
        foreach (var item in IgnorePaths)
        {
            writer.Write((ushort)item.Length);
            writer.Write(System.Text.Encoding.UTF8.GetBytes(item));
        }
    }

    public readonly string GetSourcePath()
    {
        string sourcePath = Drive + ":" + Path.DirectorySeparatorChar;
        sourcePath += SourcePath;
        return sourcePath;
    }

    public readonly int ToDataRowSize()
    {
        int size = 5 + SourcePath.Length;
        if (IgnorePaths == null) return size;
        foreach (string ignorePath in IgnorePaths)
        {
            size += 2 + ignorePath.Length;
        }
        return size;
    }
}