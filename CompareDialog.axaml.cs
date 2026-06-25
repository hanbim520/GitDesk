using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitDesk.Models;
using GitDesk.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDesk;

public partial class CompareDialog : Window
{
    private ScrollViewer? _leftScrollViewer;
    private ScrollViewer? _rightScrollViewer;
    private bool _isSyncingScroll;
    private int _currentDifferenceIndex = -1;
    private int _currentSearchIndex = -1;
    private const double CompareRowHeight = 22;

    public CompareDialog()
    {
        InitializeComponent();
        UpdateCompareThemeResources();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ActualThemeVariantProperty)
        {
            UpdateCompareThemeResources();
        }
    }

    private void UpdateCompareThemeResources()
    {
        var isDark = ActualThemeVariant.Key == "Dark";
        Resources["CompareContentBackgroundBrush"] = Brush.Parse(isDark ? "#1E1E1E" : "#FFFFFF");
        Resources["CompareLineNumberBackgroundBrush"] = Brush.Parse(isDark ? "#252525" : "#FAFAFA");
        Resources["CompareAddedBackgroundBrush"] = Brush.Parse(isDark ? "#183A24" : "#DDF4DD");
        Resources["CompareDeletedBackgroundBrush"] = Brush.Parse(isDark ? "#4A2020" : "#FFE1E1");
        Resources["CompareModifiedLeftBackgroundBrush"] = Brush.Parse(isDark ? "#463D1D" : "#FFF4C2");
        Resources["CompareModifiedRightBackgroundBrush"] = Brush.Parse(isDark ? "#243A1F" : "#E0F5D7");
        Resources["CompareAddedLineNumberBackgroundBrush"] = Brush.Parse(isDark ? "#14321F" : "#D3ECD3");
        Resources["CompareDeletedLineNumberBackgroundBrush"] = Brush.Parse(isDark ? "#3E1B1B" : "#F4D4D4");
        Resources["CompareModifiedLeftLineNumberBackgroundBrush"] = Brush.Parse(isDark ? "#3A3218" : "#F0E6AF");
        Resources["CompareModifiedRightLineNumberBackgroundBrush"] = Brush.Parse(isDark ? "#1F321B" : "#D3EACB");
        Resources["CompareAddedMarkerBrush"] = Brush.Parse(isDark ? "#4A9B5D" : "#83C783");
        Resources["CompareDeletedMarkerBrush"] = Brush.Parse(isDark ? "#B45C5C" : "#D99595");
        Resources["CompareModifiedMarkerBrush"] = Brush.Parse(isDark ? "#9C8F3F" : "#C8BE65");
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        await AttachScrollSyncAsync();
        ScrollToDifference(forward: true);
    }

    protected override void OnClosed(EventArgs e)
    {
        DetachScrollSync();
        base.OnClosed(e);
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnRefreshClicked(object? sender, RoutedEventArgs e)
    {
        await AttachScrollSyncAsync();
        if (_currentDifferenceIndex >= 0)
        {
            SelectAndScrollToLine(_currentDifferenceIndex);
        }
        else
        {
            ScrollToDifference(forward: true);
        }
    }

    private async void OnSavePatchClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CompareDialogViewModel viewModel)
        {
            return;
        }

        var topLevel = GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                Title = "Save Diff",
                SuggestedFileName = BuildPatchFileName(viewModel.Diff),
            });

        if (file is null)
        {
            return;
        }

        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        await writer.WriteAsync(viewModel.Diff.DiffText);
    }

    private async void OnCopyLeftContentClicked(object? sender, RoutedEventArgs e)
    {
        await CopyTextAsync(BuildSideContent(leftSide: true));
    }

    private async void OnCopyRightContentClicked(object? sender, RoutedEventArgs e)
    {
        await CopyTextAsync(BuildSideContent(leftSide: false));
    }

    private async void OnCopyAllContentClicked(object? sender, RoutedEventArgs e)
    {
        await CopyTextAsync(BuildAllCompareContent());
    }

    private void OnFirstDifferenceClicked(object? sender, RoutedEventArgs e)
    {
        _currentDifferenceIndex = -1;
        ScrollToDifference(forward: true);
    }

    private void OnPreviousDifferenceClicked(object? sender, RoutedEventArgs e)
    {
        ScrollToDifference(forward: false);
    }

    private void OnNextDifferenceClicked(object? sender, RoutedEventArgs e)
    {
        ScrollToDifference(forward: true);
    }

    private void OnLastDifferenceClicked(object? sender, RoutedEventArgs e)
    {
        _currentDifferenceIndex = -1;
        ScrollToDifference(forward: false);
    }

    private void OnFindPreviousClicked(object? sender, RoutedEventArgs e)
    {
        FindText(forward: false);
    }

    private void OnFindNextClicked(object? sender, RoutedEventArgs e)
    {
        FindText(forward: true);
    }

    private void OnFindTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        FindText(forward: !e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        e.Handled = true;
    }

    private async Task AttachScrollSyncAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(AttachScrollSync, DispatcherPriority.Loaded);
    }

    private void AttachScrollSync()
    {
        DetachScrollSync();

        _leftScrollViewer = LeftCompareScrollViewer;
        _rightScrollViewer = RightCompareScrollViewer;

        if (_leftScrollViewer is not null)
        {
            _leftScrollViewer.ScrollChanged += OnLeftScrollChanged;
        }

        if (_rightScrollViewer is not null)
        {
            _rightScrollViewer.ScrollChanged += OnRightScrollChanged;
        }
    }

    private void DetachScrollSync()
    {
        if (_leftScrollViewer is not null)
        {
            _leftScrollViewer.ScrollChanged -= OnLeftScrollChanged;
        }

        if (_rightScrollViewer is not null)
        {
            _rightScrollViewer.ScrollChanged -= OnRightScrollChanged;
        }

        _leftScrollViewer = null;
        _rightScrollViewer = null;
    }

    private void OnLeftScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        SyncScroll(_leftScrollViewer, _rightScrollViewer);
    }

    private void OnRightScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        SyncScroll(_rightScrollViewer, _leftScrollViewer);
    }

    private void SyncScroll(ScrollViewer? source, ScrollViewer? target)
    {
        if (_isSyncingScroll || source is null || target is null)
        {
            return;
        }

        var offset = source.Offset;
        if (target.Offset == offset)
        {
            return;
        }

        try
        {
            _isSyncingScroll = true;
            target.Offset = offset;
        }
        finally
        {
            _isSyncingScroll = false;
        }
    }

    private void ScrollToDifference(bool forward)
    {
        if (DataContext is not CompareDialogViewModel viewModel)
        {
            return;
        }

        var nextIndex = viewModel.FindDifferenceIndex(_currentDifferenceIndex, forward);
        if (nextIndex < 0)
        {
            return;
        }

        _currentDifferenceIndex = nextIndex;
        SelectAndScrollToLine(nextIndex);
    }

    private void FindText(bool forward)
    {
        if (DataContext is not CompareDialogViewModel viewModel)
        {
            return;
        }

        var query = FindTextBox.Text;
        if (string.IsNullOrWhiteSpace(query) || viewModel.Lines.Count == 0)
        {
            return;
        }

        var startIndex = _currentSearchIndex;
        for (var step = 1; step <= viewModel.Lines.Count; step++)
        {
            var index = forward
                ? (startIndex + step + viewModel.Lines.Count) % viewModel.Lines.Count
                : (startIndex - step + viewModel.Lines.Count) % viewModel.Lines.Count;

            var line = viewModel.Lines[index];
            if (line.LeftText.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                line.RightText.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                _currentSearchIndex = index;
                SelectAndScrollToLine(index);
                return;
            }
        }
    }

    private void SelectAndScrollToLine(int index)
    {
        if (DataContext is not CompareDialogViewModel viewModel ||
            index < 0 ||
            index >= viewModel.Lines.Count)
        {
            return;
        }

        var targetOffset = new Vector(
            _leftScrollViewer?.Offset.X ?? _rightScrollViewer?.Offset.X ?? 0,
            Math.Max(0, index * CompareRowHeight - CompareRowHeight));

        Dispatcher.UIThread.Post(() =>
        {
            if (_leftScrollViewer is not null && _rightScrollViewer is not null)
            {
                try
                {
                    _isSyncingScroll = true;
                    _leftScrollViewer.Offset = targetOffset;
                    _rightScrollViewer.Offset = targetOffset;
                }
                finally
                {
                    _isSyncingScroll = false;
                }
            }
        }, DispatcherPriority.Background);
    }

    private async Task CopyTextAsync(string text)
    {
        var clipboard = GetTopLevel(this)?.Clipboard;
        if (clipboard is null || string.IsNullOrEmpty(text))
        {
            return;
        }

        await clipboard.SetTextAsync(text);
    }

    private string BuildSideContent(bool leftSide)
    {
        if (DataContext is not CompareDialogViewModel viewModel)
        {
            return string.Empty;
        }

        var lines = viewModel.Lines
            .Where(line => leftSide ? line.LeftLineNumber.Length > 0 : line.RightLineNumber.Length > 0)
            .Select(line => leftSide ? line.LeftText : line.RightText);

        return string.Join(Environment.NewLine, lines);
    }

    private string BuildAllCompareContent()
    {
        if (DataContext is not CompareDialogViewModel viewModel)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Commit: {viewModel.Diff.CommitRevision}");
        builder.AppendLine($"Path: {viewModel.Diff.Path}");
        builder.AppendLine($"Status: {viewModel.Diff.Details} ({viewModel.Diff.Status})");
        builder.AppendLine();
        builder.AppendLine("LeftLine\tLeftText\tRightLine\tRightText");

        foreach (var line in viewModel.Lines)
        {
            builder.Append(line.LeftLineNumber);
            builder.Append('\t');
            builder.Append(line.LeftText);
            builder.Append('\t');
            builder.Append(line.RightLineNumber);
            builder.Append('\t');
            builder.AppendLine(line.RightText);
        }

        return builder.ToString();
    }

    private static string BuildPatchFileName(CommitChangeDiff diff)
    {
        var name = Path.GetFileName(diff.Path);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "compare";
        }

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(invalid, '_');
        }

        return $"{name}-{diff.ShortRevision}.patch";
    }
}
