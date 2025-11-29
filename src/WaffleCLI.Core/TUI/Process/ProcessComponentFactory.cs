using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Process;

public static class ProcessComponentFactory
{
    public static IProcessComponent CreateCommandProcess(string command, string arguments = "")
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        var processInfo = new ProcessInfo(
            FileName: isWindows ? "cmd.exe" : "/bin/bash",
            Arguments: isWindows ? $"/c {command} {arguments}" : $"-c \"{command} {arguments}\"",
            WorkingDirectory: Environment.CurrentDirectory
        );

        return new ProcessRunnerComponent(processInfo);
    }

    public static IProcessComponent CreateInteractiveShell()
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        
        var processInfo = new ProcessInfo(
            FileName: isWindows ? "cmd.exe" : "/bin/bash",
            Arguments: isWindows ? "/K" : "-i",
            WorkingDirectory: Environment.CurrentDirectory,
            UseShellExecute: false
        );
        
        return new ProcessRunnerComponent(processInfo);
    }
    
    public static IProcessComponent CreateCommandProcess(string command)
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        
        var processInfo = new ProcessInfo(
            FileName: isWindows ? "cmd.exe" : "/bin/bash",
            Arguments: isWindows ? $"/C {command}" : $"-c \"{command}\"",
            WorkingDirectory: Environment.CurrentDirectory,
            UseShellExecute: false
        );

        return new ProcessRunnerComponent(processInfo);
    }

    public static IProcessComponent CreateDotnetProcess(string projectPath, string arguments = "")
    {
        var processInfo = new ProcessInfo(
            FileName: "dotnet",
            Arguments: $"run --project {projectPath} {arguments}",
            WorkingDirectory: Path.GetDirectoryName(projectPath)
        );

        return new ProcessRunnerComponent(processInfo);
    }
    
    public static IProcessComponent CreateCustomProcess(string fileName, string arguments = "", string workingDir = "")
    {
        var processInfo = new ProcessInfo(
            FileName: fileName,
            Arguments: arguments,
            WorkingDirectory: string.IsNullOrEmpty(workingDir) ? Environment.CurrentDirectory : workingDir
        );

        return new ProcessRunnerComponent(processInfo);
    }
    
    public static IProcessComponent CreateTestProcess()
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        var testCommand = isWindows ? "dir" : "ls -la";
        
        return CreateCommandProcess(testCommand);
    }
}