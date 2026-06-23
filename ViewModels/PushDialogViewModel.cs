using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GitDesk.Models;

namespace GitDesk.ViewModels;

public sealed class PushDialogViewModel : ObservableObject
{
    private string? _selectedBranch;
    private string _errorText = string.Empty;

    public PushDialogViewModel(IEnumerable<string> branches, string? currentBranch)
    {
        Branches = new ObservableCollection<string>(branches);
        SelectedBranch = !string.IsNullOrWhiteSpace(currentBranch) && Branches.Contains(currentBranch)
            ? currentBranch
            : Branches.FirstOrDefault();
    }

    public ObservableCollection<string> Branches { get; }

    public string? SelectedBranch
    {
        get => _selectedBranch;
        set => SetProperty(ref _selectedBranch, value);
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

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(SelectedBranch))
        {
            ErrorText = "Select a branch to push.";
            return false;
        }

        ErrorText = string.Empty;
        return true;
    }
}
