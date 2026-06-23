namespace GitDesk.Models;

public sealed class GitHistoryEntry
{
    public GitHistoryEntry(
        string revision,
        string author,
        string date,
        string subject,
        string publishState = "Remote",
        string changeListState = "Commit",
        GitChange? change = null)
    {
        Revision = revision;
        Author = author;
        Date = date;
        Subject = subject;
        PublishState = publishState;
        ChangeListState = changeListState;
        Change = change;
    }

    public string Revision { get; }

    public string ShortRevision => Revision.Length > 12 ? Revision[..12] : Revision;

    public string Author { get; }

    public string Date { get; }

    public string Subject { get; }

    public string PublishState { get; }

    public bool IsLocal => PublishState == "Local";

    public string ChangeListState { get; }

    public GitChange? Change { get; }

    public bool IsCommitEntry => Change is null;

    public bool IsChangeEntry => Change is not null;

    public GitHistoryEntry WithPublishState(string publishState)
    {
        return new GitHistoryEntry(Revision, Author, Date, Subject, publishState, ChangeListState, Change);
    }

    public static GitHistoryEntry FromChange(GitChange change, string changeListState)
    {
        return new GitHistoryEntry(
            string.Empty,
            string.Empty,
            string.Empty,
            change.Path,
            "Local",
            changeListState,
            change);
    }
}
