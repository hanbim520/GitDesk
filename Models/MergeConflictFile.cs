namespace GitDesk.Models;

public sealed class MergeConflictFile
{
    public MergeConflictFile(
        string path,
        string status,
        string details,
        string baseContent,
        string oursContent,
        string theirsContent,
        string workingContent)
    {
        Path = path;
        Status = status;
        Details = details;
        BaseContent = baseContent;
        OursContent = oursContent;
        TheirsContent = theirsContent;
        WorkingContent = workingContent;
    }

    public string Path { get; }

    public string Status { get; }

    public string Details { get; }

    public string BaseContent { get; }

    public string OursContent { get; }

    public string TheirsContent { get; }

    public string WorkingContent { get; }
}
