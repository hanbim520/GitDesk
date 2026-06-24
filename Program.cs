using Avalonia;
using GitDesk.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GitDesk;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogCrash("AppDomain.UnhandledException", e.ExceptionObject as Exception);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            LogCrash("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Last-resort net: an AppDomain unhandled exception still terminates the process,
    // but at least leave a trace on disk so the next crash is diagnosable without the
    // Windows event log.
    private static void LogCrash(string source, Exception? ex)
    {
        try
        {
            // Capture a full minidump first so the live process state (all thread
            // stacks + memory) is preserved before anything else runs.
            var dumpPath = CrashDump.TryWrite(source);

            var path = Path.Combine(AppContext.BaseDirectory, "crash.log");
            var dumpLine = dumpPath is null ? "(no dump written)" : $"dump: {dumpPath}";
            File.AppendAllText(path, $"[{DateTime.Now:O}] {source}{Environment.NewLine}{dumpLine}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Never let crash logging itself throw.
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
