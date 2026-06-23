using Avalonia.Controls;
using Avalonia.Interactivity;
using GitDesk.ViewModels;

namespace GitDesk;

public partial class CommitDialog : Window
{
    public CommitDialog()
    {
        InitializeComponent();
    }

    private void OnCommitClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CommitDialogViewModel viewModel)
        {
            Close(false);
            return;
        }

        if (!viewModel.Validate())
        {
            return;
        }

        Close(true);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
