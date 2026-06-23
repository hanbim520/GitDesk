using GitDesk.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace GitDesk.ViewModels;

public sealed class CompareDialogViewModel
{
    private static readonly Regex HunkHeaderRegex = new(
        @"@@ -(?<old>\d+)(?:,\d+)? \+(?<new>\d+)(?:,\d+)? @@",
        RegexOptions.Compiled);

    public CompareDialogViewModel(CommitChangeDiff diff)
    {
        Diff = diff;
        Lines = new ObservableCollection<CompareLine>(ParseUnifiedDiff(diff.DiffText));
    }

    public CommitChangeDiff Diff { get; }

    public ObservableCollection<CompareLine> Lines { get; }

    public string HeaderText => $"{Diff.Details} ({Diff.Status})";

    public string StatusModeText => Diff.Status switch
    {
        "A" => "Added (A)",
        "D" => "Deleted (D)",
        "M" => "Modified (M)",
        "R" => "Renamed (R)",
        "C" => "Copied (C)",
        _ => $"{Diff.Details} ({Diff.Status})",
    };

    public string CommitText => $"Commit: {Diff.ShortRevision}";

    public string PathText => $"Path: {Diff.Path}";

    public string LeftHeaderText => string.IsNullOrWhiteSpace(Diff.LeftPath)
        ? "(empty)"
        : $"{Diff.LeftPath}@parent";

    public string RightHeaderText => string.IsNullOrWhiteSpace(Diff.RightPath)
        ? "(empty)"
        : $"{Diff.RightPath}@{Diff.ShortRevision}";

    public string SummaryText => $"{DifferenceCount} diffs | Tab spacing: 4 | Encoding: UTF-8";

    public int DifferenceCount
    {
        get
        {
            var blocks = new HashSet<int>();
            foreach (var line in Lines)
            {
                if (line.DifferenceBlockIndex is { } blockIndex)
                {
                    blocks.Add(blockIndex);
                }
            }

            return blocks.Count;
        }
    }

    public int FindDifferenceIndex(int currentIndex, bool forward)
    {
        if (Lines.Count == 0)
        {
            return -1;
        }

        if (forward)
        {
            for (var i = currentIndex + 1; i < Lines.Count; i++)
            {
                if (Lines[i].IsDifference)
                {
                    return i;
                }
            }

            for (var i = 0; i <= Math.Max(currentIndex, 0) && i < Lines.Count; i++)
            {
                if (Lines[i].IsDifference)
                {
                    return i;
                }
            }
        }
        else
        {
            var start = currentIndex < 0 ? Lines.Count - 1 : currentIndex - 1;
            for (var i = start; i >= 0; i--)
            {
                if (Lines[i].IsDifference)
                {
                    return i;
                }
            }

            for (var i = Lines.Count - 1; i >= Math.Max(currentIndex, 0); i--)
            {
                if (Lines[i].IsDifference)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static IReadOnlyList<CompareLine> ParseUnifiedDiff(string diffText)
    {
        var lines = new List<CompareLine>();
        var pendingDeletes = new List<(int lineNumber, string text)>();
        var pendingAdds = new List<(int lineNumber, string text)>();
        var oldLine = 0;
        var newLine = 0;
        var inHunk = false;
        var differenceBlockIndex = 0;

        foreach (var rawLine in diffText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            if (rawLine.StartsWith("@@ ", StringComparison.Ordinal))
            {
                FlushPending(lines, pendingDeletes, pendingAdds, ref differenceBlockIndex);
                var match = HunkHeaderRegex.Match(rawLine);
                if (match.Success)
                {
                    oldLine = int.Parse(match.Groups["old"].Value);
                    newLine = int.Parse(match.Groups["new"].Value);
                    inHunk = true;
                }

                continue;
            }

            if (!inHunk)
            {
                continue;
            }

            if (rawLine.StartsWith("\\ No newline", StringComparison.Ordinal))
            {
                continue;
            }

            if (rawLine.StartsWith("-", StringComparison.Ordinal))
            {
                pendingDeletes.Add((oldLine, rawLine.Length > 1 ? rawLine[1..] : string.Empty));
                oldLine++;
                continue;
            }

            if (rawLine.StartsWith("+", StringComparison.Ordinal))
            {
                pendingAdds.Add((newLine, rawLine.Length > 1 ? rawLine[1..] : string.Empty));
                newLine++;
                continue;
            }

            if (rawLine.StartsWith(" ", StringComparison.Ordinal))
            {
                FlushPending(lines, pendingDeletes, pendingAdds, ref differenceBlockIndex);
                var text = rawLine.Length > 1 ? rawLine[1..] : string.Empty;
                lines.Add(new CompareLine(oldLine, text, newLine, text, "Equal"));
                oldLine++;
                newLine++;
            }
        }

        FlushPending(lines, pendingDeletes, pendingAdds, ref differenceBlockIndex);

        if (lines.Count == 0)
        {
            foreach (var rawLine in diffText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                lines.Add(new CompareLine(null, string.Empty, null, rawLine, "Modified", 1));
            }
        }

        return lines;
    }

    private static void FlushPending(
        ICollection<CompareLine> lines,
        IList<(int lineNumber, string text)> pendingDeletes,
        IList<(int lineNumber, string text)> pendingAdds,
        ref int differenceBlockIndex)
    {
        var count = Math.Max(pendingDeletes.Count, pendingAdds.Count);
        if (count == 0)
        {
            return;
        }

        differenceBlockIndex++;
        for (var i = 0; i < count; i++)
        {
            var hasDelete = i < pendingDeletes.Count;
            var hasAdd = i < pendingAdds.Count;

            if (hasDelete && hasAdd)
            {
                lines.Add(new CompareLine(
                    pendingDeletes[i].lineNumber,
                    pendingDeletes[i].text,
                    pendingAdds[i].lineNumber,
                    pendingAdds[i].text,
                    "Modified",
                    differenceBlockIndex));
            }
            else if (hasDelete)
            {
                lines.Add(new CompareLine(
                    pendingDeletes[i].lineNumber,
                    pendingDeletes[i].text,
                    null,
                    string.Empty,
                    "Deleted",
                    differenceBlockIndex));
            }
            else
            {
                lines.Add(new CompareLine(
                    null,
                    string.Empty,
                    pendingAdds[i].lineNumber,
                    pendingAdds[i].text,
                    "Added",
                    differenceBlockIndex));
            }
        }

        pendingDeletes.Clear();
        pendingAdds.Clear();
    }
}
