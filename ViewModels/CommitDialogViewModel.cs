using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitDesk.Models;

namespace GitDesk.ViewModels;

public sealed class CommitDialogViewModel : ObservableObject
{
    private string _commitMessage;
    private string _errorText = string.Empty;
    private bool _allSelected = true;

    public CommitDialogViewModel(string commitMessage, IEnumerable<GitChange> changes)
    {
        _commitMessage = commitMessage;
        Files = new ObservableCollection<CommitFileSelection>(
            changes.Select(change => new CommitFileSelection(change)));
    }

    public string CommitMessage
    {
        get => _commitMessage;
        set => SetProperty(ref _commitMessage, value);
    }

    public ObservableCollection<CommitFileSelection> Files { get; }

    public bool AllSelected
    {
        get => _allSelected;
        set
        {
            if (!SetProperty(ref _allSelected, value))
            {
                return;
            }

            foreach (var file in Files)
            {
                file.IsSelected = value;
            }
        }
    }

    public string ErrorText
    {
        get => _errorText;
        private set
        {
            if (SetProperty(ref _errorText, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorText);

    public string SummaryText => Files.Count == 0
        ? "No added or changed files."
        : $"{Files.Count} changed files";

    public IReadOnlyList<string> SelectedPathspecs => Files
        .Where(file => file.IsSelected)
        .SelectMany(file => file.Pathspecs)
        .Distinct()
        .ToArray();

    public IReadOnlyList<CommitFileSelection> SelectedFiles => Files
        .Where(file => file.IsSelected)
        .ToArray();

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(CommitMessage))
        {
            ErrorText = "Commit message is empty.";
            return false;
        }

        if (SelectedPathspecs.Count == 0)
        {
            ErrorText = "Select at least one file to commit.";
            return false;
        }

        ErrorText = string.Empty;
        return true;
    }
}
