using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace GitDesk.Services;

/// <summary>
/// Writes a Windows minidump (.dmp) of the current process when the app is about to
/// die from an unhandled exception. The dump captures every thread's stack and full
/// process memory, so it can be opened in WinDbg/cdb (or analyzed with the
/// dump-stack-analysis tooling) to recover the managed call stack and locals.
/// </summary>
public static class CrashDump
{
    // MINIDUMP_TYPE flags (dbghelp.h). Full memory + handle/thread info gives the most
    // useful managed-debugging dump at the cost of size.
    [Flags]
    private enum MiniDumpType : uint
    {
        WithFullMemory = 0x00000002,
        WithHandleData = 0x00000004,
        WithProcessThreadData = 0x00000100,
        WithFullMemoryInfo = 0x00000800,
        WithThreadInfo = 0x00001000,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct MiniDumpExceptionInformation
    {
        public uint ThreadId;
        public IntPtr ExceptionPointers;
        [MarshalAs(UnmanagedType.Bool)]
        public bool ClientPointers;
    }

    [DllImport("dbghelp.dll", SetLastError = true)]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        SafeHandle hFile,
        MiniDumpType dumpType,
        ref MiniDumpExceptionInformation exceptionParam,
        IntPtr userStreamParam,
        IntPtr callbackParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    /// <summary>
    /// Attempts to write a minidump next to the executable under "dumps/". Returns the
    /// dump path on success, or null if dumping is unsupported or failed. Never throws.
    /// </summary>
    public static string? TryWrite(string reason)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            var dumpDir = Path.Combine(AppContext.BaseDirectory, "dumps");
            Directory.CreateDirectory(dumpDir);

            // Avalonia is not available here and Math.Random/DateTime are fine in this
            // context; a sortable timestamp keeps dumps grouped and unique enough.
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var safeReason = string.Concat(reason.Split(Path.GetInvalidFileNameChars()));
            var dumpPath = Path.Combine(dumpDir, $"GitDesk_{stamp}_{safeReason}.dmp");

            using var process = Process.GetCurrentProcess();
            using var fileStream = new FileStream(dumpPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

            // Marshal.GetExceptionPointers() returns the EXCEPTION_POINTERS for the
            // exception currently in flight; valid while the unhandled-exception event
            // is being dispatched. ClientPointers=false: the pointers belong to us.
            var exceptionInfo = new MiniDumpExceptionInformation
            {
                ThreadId = GetCurrentThreadId(),
                ExceptionPointers = Marshal.GetExceptionPointers(),
                ClientPointers = false,
            };

            const MiniDumpType dumpType =
                MiniDumpType.WithFullMemory |
                MiniDumpType.WithHandleData |
                MiniDumpType.WithProcessThreadData |
                MiniDumpType.WithFullMemoryInfo |
                MiniDumpType.WithThreadInfo;

            var ok = MiniDumpWriteDump(
                process.Handle,
                (uint)process.Id,
                fileStream.SafeFileHandle,
                dumpType,
                ref exceptionInfo,
                IntPtr.Zero,
                IntPtr.Zero);

            return ok ? dumpPath : null;
        }
        catch
        {
            // A crash dumper must never itself throw.
            return null;
        }
    }
}
