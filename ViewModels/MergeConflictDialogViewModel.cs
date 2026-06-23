using GitDesk.Models;

namespace GitDesk.ViewModels;

public sealed class MergeConflictDialogViewModel : ObservableObject
{
    private string _workingContent;

    public MergeConflictDialogViewModel(MergeConflictFile file)
    {
        File = file;
        _workingContent = file.WorkingContent;
    }

    public MergeConflictFile File { get; }

    public string TitleText => $"Merge Conflict - {File.Path}";

    public string SummaryText => $"{File.Details} ({File.Status})";

    public string BaseHeaderText => $"Base: {File.Path}";

    public string OursHeaderText => $"Ours: {File.Path}";

    public string TheirsHeaderText => $"Theirs: {File.Path}";

    public string WorkingHeaderText => $"Working File: {File.Path}";

    public string BaseContent => File.BaseContent;

    public string OursContent => File.OursContent;

    public string TheirsContent => File.TheirsContent;

    public string WorkingContent
    {
        get => _workingContent;
        set => SetProperty(ref _workingContent, value);
    }
}
