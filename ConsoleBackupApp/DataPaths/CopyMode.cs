namespace ConsoleBackupApp.DataPaths;
public enum CopyMode
{
    None,
    ForceCopy = 'f'
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