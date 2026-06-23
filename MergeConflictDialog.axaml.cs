using Avalonia.Controls;
using Avalonia.Interactivity;
using GitDesk.ViewModels;
using System;
using System.Threading.Tasks;

namespace GitDesk;

public partial class MergeConflictDialog : Window
{
    private Func<string, Task<bool>> _saveAsync = _ => Task.FromResult(false);
    private Func<Task<bool>> _useOursAsync = () => Task.FromResult(false);
    private Func<Task<bool>> _useTheirsAsync = () => Task.FromResult(false);
    private Func<Task<bool>> _markResolvedAsync = () => Task.FromResult(false);

    public MergeConflictDialog()
    {
        InitializeComponent();
    }

    public MergeConflictDialog(
        Func<string, Task<bool>> saveAsync,
        Func<Task<bool>> useOursAsync,
        Func<Task<bool>> useTheirsAsync,
        Func<Task<bool>> markResolvedAsync)
    {
        _saveAsync = saveAsync;
        _useOursAsync = useOursAsync;
        _useTheirsAsync = useTheirsAsync;
        _markResolvedAsync = markResolvedAsync;
        InitializeComponent();
    }

    private MergeConflictDialogViewModel ViewModel => (MergeConflictDialogViewModel)DataContext!;

    private async void OnUseOursClicked(object? sender, RoutedEventArgs e)
    {
        if (await _useOursAsync())
        {
            Close(true);
        }
    }

    private async void OnUseTheirsClicked(object? sender, RoutedEventArgs e)
    {
        if (await _useTheirsAsync())
        {
            Close(true);
        }
    }

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        await _saveAsync(ViewModel.WorkingContent);
    }

    private async void OnMarkResolvedClicked(object? sender, RoutedEventArgs e)
    {
        if (!await _saveAsync(ViewModel.WorkingContent))
        {
            return;
        }

        if (await _markResolvedAsync())
        {
            Close(true);
        }
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
