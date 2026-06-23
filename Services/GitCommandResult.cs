namespace GitDesk.Services;

public sealed class GitCommandResult
{
    public GitCommandResult(int exitCode, string standardOutput, string standardError)
    {
        ExitCode = exitCode;
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    public int ExitCode { get; }

    public string StandardOutput { get; }

    public string StandardError { get; }

    public bool IsSuccess => ExitCode == 0;
}
