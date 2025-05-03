namespace ConsoleBackupApp.DataPaths;
[Flags]
public enum CopyMode : byte
{
    None,
    ForceCopy = 1
}

public static class CopyModeExtensions
{
    public static CopyMode FromChar(char c)
    {
        return c switch
        {
            'f' => CopyMode.ForceCopy,
            _ => CopyMode.None,
        };
    }
}