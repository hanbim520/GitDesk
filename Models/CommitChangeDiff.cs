namespace GitDesk.Models;

public sealed class CommitChangeDiff
{
    public CommitChangeDiff(
        string commitRevision,
        string path,
        string leftPath,
        string rightPath,
        string status,
        string details,
        string diffText)
    {
        CommitRevision = commitRevision;
        Path = path;
        LeftPath = leftPath;
        RightPath = rightPath;
        Status = status;
        Details = details;
        DiffText = diffText;
    }

    public string CommitRevision { get; }

    public string ShortRevision => CommitRevision.Length > 12 ? CommitRevision[..12] : CommitRevision;

    public string Path { get; }

    public string LeftPath { get; }

    public string RightPath { get; }

    public string Status { get; }

    public string Details { get; }

    public string DiffText { get; }
}
