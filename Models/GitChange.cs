namespace GitDesk.Models;

public sealed class GitChange
{
    public GitChange(string status, string path, string details)
    {
        Status = status;
        Path = path;
        Details = details;
    }

    public string Status { get; }

    public string Path { get; }

    public string Details { get; }
}
