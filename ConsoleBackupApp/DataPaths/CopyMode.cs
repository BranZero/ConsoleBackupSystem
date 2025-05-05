namespace ConsoleBackupApp.DataPaths;
[Flags]
public enum CopyMode : byte
{
    None = 0,
    ForceCopy = 1,//Must copy everything in the DataPath
    AllOrNone = 2//Copy Everyting if any changed otherwise don't
}

public static class CopyModeExtensions
{
    public static CopyMode FromChar(char c)
    {
        return c switch
        {
            'c' => CopyMode.ForceCopy,
            'a' => CopyMode.AllOrNone,
            _ => CopyMode.None,
        };
    }
    public static CopyMode MergeCopyMode(CopyMode a, CopyMode b)
    {
        return a ^ b;
    }
}