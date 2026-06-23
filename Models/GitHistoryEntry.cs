namespace GitDesk.Models;

public sealed class GitHistoryEntry
{
    public GitHistoryEntry(
        string revision,
        string author,
        string date,
        string subject,
        string publishState = "Remote")
    {
        Revision = revision;
        Author = author;
        Date = date;
        Subject = subject;
        PublishState = publishState;
    }

    public string Revision { get; }

    public string ShortRevision => Revision.Length > 12 ? Revision[..12] : Revision;

    public string Author { get; }

    public string Date { get; }

    public string Subject { get; }

    public string PublishState { get; }

    public bool IsLocal => PublishState == "Local";

    public GitHistoryEntry WithPublishState(string publishState)
    {
        return new GitHistoryEntry(Revision, Author, Date, Subject, publishState);
    }
}
