using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitDesk.Models;
using GitDesk.ViewModels;
using System.Threading.Tasks;

namespace GitDesk;

public partial class CLChangesDialog : Window
{
    public CLChangesDialog()
    {
        InitializeComponent();
    }

    private CLChangesDialogViewModel ViewModel => (CLChangesDialogViewModel)DataContext!;

    private void OnChangesGridPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(ChangesGrid).Properties.IsRightButtonPressed)
        {
            return;
        }

        var change = FindDataContextFromSource<GitChange>(e.Source);
        if (change is null)
        {
            return;
        }

        ChangesGrid.SelectedItem = change;
        ViewModel.SelectedChange = change;
    }

    private async void OnCompareClicked(object? sender, RoutedEventArgs e)
    {
        var diff = await ViewModel.GetSelectedChangeDiffAsync();
        if (diff is null)
        {
            return;
        }

        var dialog = new CompareDialog
        {
            DataContext = new CompareDialogViewModel(diff),
        };

        await dialog.ShowDialog(this);
    }

    private static T? FindDataContextFromSource<T>(object? source)
        where T : class
    {
        var visual = source as Avalonia.Visual;
        while (visual is not null)
        {
            if (visual is Control { DataContext: T item })
            {
                return item;
            }

            visual = visual.GetVisualParent();
        }

        return null;
    }
}
