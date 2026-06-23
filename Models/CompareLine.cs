using Avalonia.Media;

namespace GitDesk.Models;

public sealed class CompareLine
{
    private static readonly IBrush ClearBrush = Brushes.Transparent;
    private static readonly IBrush EqualLineNumberBrush = Brush.Parse("#FAFAFA");
    private static readonly IBrush AddedBrush = Brush.Parse("#DDF4DD");
    private static readonly IBrush DeletedBrush = Brush.Parse("#FFE1E1");
    private static readonly IBrush ModifiedLeftBrush = Brush.Parse("#FFF4C2");
    private static readonly IBrush ModifiedRightBrush = Brush.Parse("#E0F5D7");
    private static readonly IBrush AddedMarkerBrush = Brush.Parse("#83C783");
    private static readonly IBrush DeletedMarkerBrush = Brush.Parse("#D99595");
    private static readonly IBrush ModifiedMarkerBrush = Brush.Parse("#C8BE65");

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

    public IBrush LeftBackground => ChangeKind switch
    {
        "Deleted" => DeletedBrush,
        "Modified" => ModifiedLeftBrush,
        _ => ClearBrush,
    };

    public IBrush RightBackground => ChangeKind switch
    {
        "Added" => AddedBrush,
        "Modified" => ModifiedRightBrush,
        _ => ClearBrush,
    };

    public IBrush LeftLineNumberBackground => ChangeKind switch
    {
        "Deleted" => DeletedBrush,
        "Modified" => ModifiedLeftBrush,
        _ => EqualLineNumberBrush,
    };

    public IBrush RightLineNumberBackground => ChangeKind switch
    {
        "Added" => AddedBrush,
        "Modified" => ModifiedRightBrush,
        _ => EqualLineNumberBrush,
    };

    public IBrush LeftMarkerBackground => ChangeKind switch
    {
        "Deleted" => DeletedMarkerBrush,
        "Modified" => ModifiedMarkerBrush,
        _ => ClearBrush,
    };

    public IBrush RightMarkerBackground => ChangeKind switch
    {
        "Added" => AddedMarkerBrush,
        "Modified" => AddedMarkerBrush,
        _ => ClearBrush,
    };
}
