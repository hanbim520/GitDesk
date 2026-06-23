using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GitDesk.Models;

public sealed class FileSystemNode : ObservableObject
{
    private bool _isExpanded;
    private bool _isLoaded;

    public FileSystemNode(string fullPath, bool isDirectory)
    {
        FullPath = fullPath;
        IsDirectory = isDirectory;
        Name = GetDisplayName(fullPath, isDirectory);
        IsRepositoryRoot = isDirectory && Directory.Exists(Path.Combine(fullPath, ".git"));

        if (isDirectory)
        {
            Children.Add(CreatePlaceholder());
        }
    }

    private FileSystemNode(string name)
    {
        Name = name;
        FullPath = string.Empty;
        IsDirectory = false;
        IsPlaceholder = true;
    }

    public string Name { get; }

    public string FullPath { get; }

    public bool IsDirectory { get; }

    public bool IsRepositoryRoot { get; }

    public bool IsPlaceholder { get; }

    public string Icon => IsPlaceholder ? string.Empty : IsDirectory ? (IsRepositoryRoot ? "R" : "D") : "F";

    public ObservableCollection<FileSystemNode> Children { get; } = new();

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value) && value)
            {
                LoadChildren();
            }
        }
    }

    public void Refresh()
    {
        _isLoaded = false;
        Children.Clear();

        if (IsDirectory)
        {
            Children.Add(CreatePlaceholder());
        }

        if (IsExpanded)
        {
            LoadChildren();
        }
    }

    private void LoadChildren()
    {
        if (!IsDirectory || _isLoaded || IsPlaceholder)
        {
            return;
        }

        _isLoaded = true;
        Children.Clear();

        try
        {
            var directories = Directory.EnumerateDirectories(FullPath)
                .Where(ShouldShowPath)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .Select(path => new FileSystemNode(path, true));

            var files = Directory.EnumerateFiles(FullPath)
                .Where(ShouldShowPath)
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .Select(path => new FileSystemNode(path, false));

            foreach (var child in directories.Concat(files))
            {
                Children.Add(child);
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (IOException)
        {
        }
    }

    private static bool ShouldShowPath(string path)
    {
        var name = Path.GetFileName(path);
        return !string.Equals(name, ".git", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDisplayName(string fullPath, bool isDirectory)
    {
        var trimmed = fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var name = Path.GetFileName(trimmed);

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return isDirectory ? trimmed : fullPath;
    }

    private static FileSystemNode CreatePlaceholder()
    {
        return new FileSystemNode("Loading...");
    }
}
