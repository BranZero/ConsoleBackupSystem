namespace ConsoleBackupApp.DataPaths;
public enum PathType
{
    File = 'f',
    Directory = 'd',
    Unknown = '?',
}
public static class PathTypeExtensions{
    public static PathType FromChar(char c)
    {
        return c switch
        {
            'f' => PathType.File,
            'd' => PathType.Directory,
            _ => PathType.Unknown,
        };
    }
}