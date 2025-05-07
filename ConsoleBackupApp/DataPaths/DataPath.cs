
namespace ConsoleBackupApp.DataPaths;
public struct DataPath : IComparable<DataPath>
{
    public readonly char Drive => SourcePath[0];
    public PathType Type;
    public readonly CopyMode FileCopyMode;
    public readonly string SourcePath;
    public readonly string[]? IgnorePaths;

    public DataPath(PathType pathType, CopyMode copyMode, string sourcePath, string[]? ignorePaths = null)
    {
        Type = pathType;
        FileCopyMode = copyMode;
        SourcePath = sourcePath;
        IgnorePaths = ignorePaths;
    }

    public static bool Init(PathType pathType, CopyMode copyMode, ReadOnlySpan<string> args, out DataPath dataPath)
    {
        dataPath = default;
        if (args[0].Length < 4) return false;
        if (!char.IsLetter(args[0][0])) return false;
        string sourcePath = args[0];

        string[] ignorePaths = new string[args.Length - 1];
        for (int i = 1; i < args.Length; i++)
        {
            ignorePaths[i - 1] = args[i];
        }
        dataPath = new(pathType, copyMode, sourcePath, ignorePaths);
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

    public HashSet<string> GetIgnorePaths()
    {
        if (IgnorePaths == null || IgnorePaths.Length == 0)
        {
            return [];//empty
        }
        else
        {
            return [.. IgnorePaths];
        }
    }
}