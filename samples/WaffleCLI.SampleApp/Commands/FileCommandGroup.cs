using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Output;
using WaffleCLI.Runtime.Services;

namespace WaffleCLI.SampleApp.Commands;

public class FileCommandGroup : CommandGroup
{
    public FileCommandGroup(IConsoleOutput output) : base(output)
    {
        RegisterSubCommand("list", new FileListCommand(output));
        RegisterSubCommand("info", new FileInfoCommand(output));
        RegisterSubCommand("copy", new FileCopyCommand(output));
        RegisterSubCommand("delete", new FileDeleteCommand(output));
    }
        
    public override string Name => "file";
    public override string Description => "File management operations";
}

/// <summary>
/// Subcommand for listing files
/// </summary>
public class FileListCommand(IConsoleOutput output) : ICommand
{
    public string Name => "list";
    public string Description => "List files in directory";
    
    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        var path = args.Length > 0 ? args[0] : ".";
        output.WriteInfo($"Listing files in: {Path.GetFullPath(path)}");
        
        // Implementation for listing files
        return Task.CompletedTask;
    }
}

public class FileCopyCommand(IConsoleOutput output) : ICommand
{
    public string Name => "copy";
    public string Description => "Copy files into a directory";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length < 2)
        {
            output.WriteError("Source and destination paths are required");
            return Task.CompletedTask;
        }

        var sourcePath = args[0];
        var destPath = args[1];

        if (!File.Exists(sourcePath))
        {
            output.WriteError($"Source file not found: {sourcePath}");
            return Task.CompletedTask;
        }
        
        try
        {
            File.Copy(sourcePath, destPath, overwrite: true);
            output.WriteSuccess($"File copied successfully: {sourcePath} -> {destPath}");
        }
        catch (Exception ex)
        {
            output.WriteError($"Error copying file: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Subcommand for deleting files
/// </summary>
public class FileDeleteCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileDeleteCommand(IConsoleOutput output)
    {
        _output = output;
    }
    
    public string Name => "delete";
    public string Description => "Delete a file";
    
    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0)
        {
            _output.WriteError("File path is required");
            return Task.CompletedTask;
        }

        var filePath = args[0];
        
        if (!File.Exists(filePath))
        {
            _output.WriteError($"File not found: {filePath}");
            return Task.CompletedTask;
        }

        try
        {
            File.Delete(filePath);
            _output.WriteSuccess($"File deleted successfully: {filePath}");
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error deleting file: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Subcommand for getting file information
/// </summary>
public class FileInfoCommand : ICommand
{
    private readonly IConsoleOutput _output;

    public FileInfoCommand(IConsoleOutput output)
    {
        _output = output;
    }
    
    public string Name => "info";
    public string Description => "Get detailed information about a file";
    
    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0)
        {
            _output.WriteError("File path is required");
            return Task.CompletedTask;
        }

        var filePath = args[0];
        
        if (!File.Exists(filePath))
        {
            _output.WriteError($"File not found: {filePath}");
            return Task.CompletedTask;
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            _output.WriteInfo($"File: {fileInfo.Name}");
            _output.WriteInfo($"Path: {fileInfo.FullName}");
            _output.WriteInfo($"Size: {fileInfo.Length} bytes");
            _output.WriteInfo($"Created: {fileInfo.CreationTime}");
            _output.WriteInfo($"Modified: {fileInfo.LastWriteTime}");
            _output.WriteInfo($"Attributes: {fileInfo.Attributes}");
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error getting file info: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}