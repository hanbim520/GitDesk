using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace GitDesk;

public partial class CLSearchDialog : Window
{
    public CLSearchDialog()
    {
        InitializeComponent();
    }

    public string CommitText => CLTextBox.Text?.Trim() ?? string.Empty;

    protected override void OnOpened(System.EventArgs e)
    {
        base.OnOpened(e);
        CLTextBox.Focus();
    }

    private void OnOkClicked(object? sender, RoutedEventArgs e)
    {
        Close(!string.IsNullOrWhiteSpace(CommitText));
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void OnCLTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Close(!string.IsNullOrWhiteSpace(CommitText));
            e.Handled = true;
        }
    }
}
