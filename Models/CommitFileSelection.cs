using System;
using System.Collections.Generic;
using System.Linq;

namespace GitDesk.Models;

public sealed class CommitFileSelection : ObservableObject
{
    private bool _isSelected = true;

    public CommitFileSelection(GitChange change)
    {
        Status = change.Status;
        Path = change.Path;
        Details = change.Details;
        Pathspecs = BuildPathspecs(change.Path);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string Status { get; }

    public string DisplayStatus => Details;

    public string Path { get; }

    public string Details { get; }

    public IReadOnlyList<string> Pathspecs { get; }

    private static IReadOnlyList<string> BuildPathspecs(string path)
    {
        const string renameMarker = " -> ";
        if (!path.Contains(renameMarker, StringComparison.Ordinal))
        {
            return new[] { path };
        }

        return path
            .Split(new[] { renameMarker }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();
    }
}
