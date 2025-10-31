using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for listing files
/// </summary>
public class FileListCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileListCommand(IConsoleOutput output)
    {
        _output = output;
    }
    
    public string Name => "list";
    public string Description => "List files in directory";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        var path = args.Length > 0 ? args[0] : ".";
        var fullPath = Path.GetFullPath(path);
        
        _output.WriteLine($"📁 Files in: {fullPath}", ConsoleColor.Cyan);
        _output.WriteLine(new string('═', fullPath.Length + 12), ConsoleColor.Cyan);
        
        try
        {
            var files = Directory.GetFiles(fullPath);
            var directories = Directory.GetDirectories(fullPath);

            foreach (var dir in directories.Take(10))
            {
                var dirName = Path.GetFileName(dir);
                _output.WriteLine($"  📂 {dirName}", ConsoleColor.Blue);
            }

            foreach (var file in files.Take(10))
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                _output.WriteLine($"  📄 {fileName} ({fileInfo.Length:N0} bytes)");
            }

            if (directories.Length > 10 || files.Length > 10)
            {
                _output.WriteWarning("Output truncated to 10 items each");
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Command for showing file information
/// </summary>
public class FileInfoCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileInfoCommand(IConsoleOutput output)
    {
        _output = output;
    }

    public string Name => "info";
    public string Description => "Show file information";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0)
        {
            _output.WriteError("Please specify a file path");
            return Task.CompletedTask;
        }

        try
        {
            var fileInfo = new FileInfo(args[0]);
            
            if (!fileInfo.Exists)
            {
                _output.WriteError($"File not found: {args[0]}");
                return Task.CompletedTask;
            }

            _output.WriteLine("📄 File Information", ConsoleColor.Cyan);
            _output.WriteLine("═══════════════════", ConsoleColor.Cyan);
            _output.WriteLine($"Name: {fileInfo.Name}");
            _output.WriteLine($"Full Path: {fileInfo.FullName}");
            _output.WriteLine($"Size: {fileInfo.Length:N0} bytes");
            _output.WriteLine($"Created: {fileInfo.CreationTime:g}");
            _output.WriteLine($"Modified: {fileInfo.LastWriteTime:g}");
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Command for copying files (заглушка)
/// </summary>
public class FileCopyCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileCopyCommand(IConsoleOutput output)
    {
        _output = output;
    }

    public string Name => "copy";
    public string Description => "Copy files (not implemented)";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        _output.WriteWarning("File copy functionality is not implemented yet");
        _output.WriteInfo("Usage: file copy <source> <destination>");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Command for deleting files (заглушка)
/// </summary>
public class FileDeleteCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileDeleteCommand(IConsoleOutput output)
    {
        _output = output;
    }

    public string Name => "delete";
    public string Description => "Delete files (not implemented)";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        _output.WriteWarning("File delete functionality is not implemented yet");
        _output.WriteInfo("Usage: file delete <filepath>");
        return Task.CompletedTask;
    }
}