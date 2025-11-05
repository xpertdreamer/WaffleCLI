using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;

namespace WaffleCLI.SampleApp.Commands;

[SubCommand("file", "list")]
public class FileListCommand : ICommand
{
    public string Name => "list";
    public string Description => "List files in directory";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        var path = args.Length > 0 ? args[0] : ".";
        var fullPath = Path.GetFullPath(path);
        
        Console.WriteLine($"📁 Files in: {fullPath}");
        Console.WriteLine(new string('═', fullPath.Length + 12));
        
        try
        {
            var files = Directory.GetFiles(fullPath);
            var directories = Directory.GetDirectories(fullPath);

            foreach (var dir in directories.Take(10))
            {
                var dirName = Path.GetFileName(dir);
                Console.WriteLine($"  📂 {dirName}");
            }

            foreach (var file in files.Take(10))
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                Console.WriteLine($"  📄 {fileName} ({fileInfo.Length:N0} bytes)");
            }

            if (directories.Length > 10 || files.Length > 10)
            {
                Console.WriteLine("Output truncated to 10 items each");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

[SubCommand("file", "info")]
public class FileInfoCommand : ICommand
{
    public string Name => "info";
    public string Description => "Show file information";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please specify a file path");
            return Task.CompletedTask;
        }

        try
        {
            var fileInfo = new FileInfo(args[0]);
            
            if (!fileInfo.Exists)
            {
                Console.WriteLine($"File not found: {args[0]}");
                return Task.CompletedTask;
            }

            Console.WriteLine("📄 File Information");
            Console.WriteLine("═══════════════════");
            Console.WriteLine($"Name: {fileInfo.Name}");
            Console.WriteLine($"Full Path: {fileInfo.FullName}");
            Console.WriteLine($"Size: {fileInfo.Length:N0} bytes");
            Console.WriteLine($"Created: {fileInfo.CreationTime:g}");
            Console.WriteLine($"Modified: {fileInfo.LastWriteTime:g}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

[SubCommand("file", "copy")]
public class FileCopyCommand : ICommand
{
    public string Name => "copy";
    public string Description => "Copy files (not implemented)";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        Console.WriteLine("File copy functionality is not implemented yet");
        Console.WriteLine("Usage: file copy <source> <destination>");
        return Task.CompletedTask;
    }
}

[SubCommand("file", "delete")]
public class FileDeleteCommand : ICommand
{
    public string Name => "delete";
    public string Description => "Delete files (not implemented)";

    public Task ExecuteAsync(string[] args, CancellationToken token = default)
    {
        Console.WriteLine("File delete functionality is not implemented yet");
        Console.WriteLine("Usage: file delete <filepath>");
        return Task.CompletedTask;
    }
}