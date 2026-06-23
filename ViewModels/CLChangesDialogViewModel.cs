using GitDesk.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace GitDesk.ViewModels;

public sealed class CLChangesDialogViewModel : ObservableObject
{
    private readonly Func<Task> _checkoutAsync;
    private readonly Func<GitHistoryEntry, GitChange, Task<CommitChangeDiff?>> _compareAsync;
    private GitChange? _selectedChange;

    public CLChangesDialogViewModel(
        GitHistoryEntry commit,
        IReadOnlyList<GitChange> changes,
        Func<Task> checkoutAsync,
        Func<GitHistoryEntry, GitChange, Task<CommitChangeDiff?>> compareAsync)
    {
        Commit = commit;
        _checkoutAsync = checkoutAsync;
        _compareAsync = compareAsync;
        Changes = new ObservableCollection<GitChange>(changes);
        CheckoutCommand = new AsyncRelayCommand(_ => CheckoutAsync());
    }

    public GitHistoryEntry Commit { get; }

    public ObservableCollection<GitChange> Changes { get; }

    public AsyncRelayCommand CheckoutCommand { get; }

    public string TitleText => $"CL Changes - {Commit.ShortRevision} ({Changes.Count})";

    public string DetailText => $"{Commit.Author} | {Commit.Date} | {Commit.Subject}";

    public GitChange? SelectedChange
    {
        get => _selectedChange;
        set => SetProperty(ref _selectedChange, value);
    }

    private async Task CheckoutAsync()
    {
        await _checkoutAsync();
    }

    public async Task<CommitChangeDiff?> GetSelectedChangeDiffAsync()
    {
        return SelectedChange is null
            ? null
            : await _compareAsync(Commit, SelectedChange);
    }
}
