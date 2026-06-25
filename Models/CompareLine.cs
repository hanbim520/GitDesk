namespace GitDesk.Models;

public sealed class CompareLine
{

    public CompareLine(
        int? leftLineNumber,
        string leftText,
        int? rightLineNumber,
        string rightText,
        string changeKind,
        int? differenceBlockIndex = null)
    {
        LeftLineNumber = leftLineNumber?.ToString() ?? string.Empty;
        LeftText = leftText;
        RightLineNumber = rightLineNumber?.ToString() ?? string.Empty;
        RightText = rightText;
        ChangeKind = changeKind;
        DifferenceBlockIndex = differenceBlockIndex;
    }

    public string LeftLineNumber { get; }

    public string LeftText { get; }

    public string RightLineNumber { get; }

    public string RightText { get; }

    public string ChangeKind { get; }

    public int? DifferenceBlockIndex { get; }

    public bool IsDifference => ChangeKind != "Equal";

    public bool IsAdded => ChangeKind == "Added";

    public bool IsDeleted => ChangeKind == "Deleted";

    public bool IsModified => ChangeKind == "Modified";
}
