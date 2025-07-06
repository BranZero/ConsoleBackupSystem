
namespace ConsoleBackupApp;

public readonly struct Result(ResultType outcome, string? msg = null)
{
    public readonly ResultType ResultType = outcome;
    public readonly string? Message = msg;

    public readonly string GetMessage()
    {
        return $"{ResultType.ToString().Replace('_', ' ')}" + Message != null ? $": {Message}" : "";
    }
}

public enum ResultType
{
    Success,
    No_Change,
    Info,
    Warning,
    Error,

    //Path Results
    Path_Not_Found,
    SubPath_Or_SamePath,
    Path_Invalid,
    Exists,

    //DPF Results
    Not_Found,
    Empty,

    //Argument Issues
    Too_Few_Arguments,
    Too_Many_Arguments,

    //Options
    Invalid_Option,
    Duplicate_Option,
    Valid_Option,
    No_Options,
}