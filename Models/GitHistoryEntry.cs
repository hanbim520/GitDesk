using System;
using System.Collections.Generic;

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
        IReadOnlyList<GitChange>? changes = null)
    {
        Revision = revision;
        Author = author;
        Date = date;
        Subject = subject;
        PublishState = publishState;
        ChangeListState = changeListState;
        Changes = changes ?? Array.Empty<GitChange>();
    }

    public string Revision { get; }

    public string ShortRevision => Revision.Length > 12 ? Revision[..12] : Revision;

    public string Author { get; }

    public string Date { get; }

    public string Subject { get; }

    public string PublishState { get; }

    public bool IsLocal => PublishState == "Local";

    public string ChangeListState { get; }

    public IReadOnlyList<GitChange> Changes { get; }

    public bool IsCommitEntry => Changes.Count == 0;

    public bool IsChangeEntry => Changes.Count > 0;

    public GitHistoryEntry WithPublishState(string publishState)
    {
        return new GitHistoryEntry(Revision, Author, Date, Subject, publishState, ChangeListState, Changes);
    }

    public static GitHistoryEntry FromChanges(IReadOnlyList<GitChange> changes)
    {
        return new GitHistoryEntry(
            string.Empty,
            string.Empty,
            string.Empty,
            $"Staged Changes ({changes.Count})",
            "Local",
            "Staged",
            changes);
    }
}
