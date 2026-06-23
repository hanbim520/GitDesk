using Avalonia.Controls;
using Avalonia.Interactivity;
using GitDesk.ViewModels;

namespace GitDesk;

public partial class PushDialog : Window
{
    public PushDialog()
    {
        InitializeComponent();
    }

    private void OnPushClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PushDialogViewModel viewModel)
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
