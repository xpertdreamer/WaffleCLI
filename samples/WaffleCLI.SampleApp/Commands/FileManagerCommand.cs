using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;

namespace WaffleCLI.SampleApp.Commands;

/// <summary>
/// Command for file system operations including directory listing and file information
/// </summary>
[Command("files", "File management operations")]
public class FileManagerCommand : ICommand
{
    private readonly IConsoleOutput _output;

    /// <summary>
    /// Initializes a new instance of the FileManagerCommand class
    /// </summary>
    /// <param name="output">Console output service</param>
    public FileManagerCommand(IConsoleOutput output)
    {
        _output = output;
    }

    /// <summary>
    /// Gets the command name
    /// </summary>
    public string Name => "files";

    /// <summary>
    /// Gets the command description
    /// </summary>
    public string Description => "File management operations";

    /// <summary>
    /// Executes file management operations based on subcommands
    /// </summary>
    /// <param name="args">Subcommands: list, info, or empty for current directory</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (args.Length == 0)
        {
            ShowCurrentDirectory();
            return Task.CompletedTask;
        }

        switch (args[0].ToLower())
        {
            case "list":
                ListFiles(args.Length > 1 ? args[1] : ".");
                break;
            case "info":
                if (args.Length > 1)
                    ShowFileInfo(args[1]);
                else
                    _output.WriteError("Please specify a filename");
                break;
            default:
                _output.WriteError("Unknown subcommand. Use: list, info");
                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Displays the current working directory
    /// </summary>
    private void ShowCurrentDirectory()
    {
        var currentDir = Directory.GetCurrentDirectory();
        _output.WriteInfo($"Current directory: {currentDir}");
    }

    /// <summary>
    /// Lists files and directories in the specified path
    /// </summary>
    /// <param name="path">Directory path to list</param>
    private void ListFiles(string path)
    {
        try
        {
            var directory = Path.GetFullPath(path);
            
            if (!Directory.Exists(directory))
            {
                _output.WriteError($"Directory not found: {directory}");
                return;
            }

            var files = Directory.GetFiles(directory);
            var directories = Directory.GetDirectories(directory);

            _output.WriteLine($"Contents of: {directory}", ConsoleColor.Cyan);
            _output.WriteLine(new string('-', directory.Length + 12), ConsoleColor.Cyan);
            _output.WriteLine();

            // Show directories
            foreach (var dir in directories.Take(20))
            {
                var dirName = Path.GetFileName(dir);
                _output.WriteLine($"  [DIR]  {dirName}", ConsoleColor.Blue);
            }

            // Show files
            foreach (var file in files.Take(20))
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                _output.WriteLine($"  [FILE] {fileName} ({fileInfo.Length:N0} bytes)");
            }

            if (directories.Length > 20 || files.Length > 20)
            {
                _output.WriteWarning("Output truncated to 20 items each");
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays detailed information about a specific file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    private void ShowFileInfo(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            
            if (!fileInfo.Exists)
            {
                _output.WriteError($"File not found: {filePath}");
                return;
            }

            _output.WriteLine("File Information:", ConsoleColor.Cyan);
            _output.WriteLine("=================", ConsoleColor.Cyan);
            _output.WriteLine($"Name: {fileInfo.Name}");
            _output.WriteLine($"Full Path: {fileInfo.FullName}");
            _output.WriteLine($"Size: {fileInfo.Length:N0} bytes");
            _output.WriteLine($"Created: {fileInfo.CreationTime:g}");
            _output.WriteLine($"Modified: {fileInfo.LastWriteTime:g}");
            _output.WriteLine($"Attributes: {fileInfo.Attributes}");
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error: {ex.Message}");
        }
    }
}