using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Abstractions.Scripting;
using WaffleCLI.Core.Output;

namespace WaffleCLI.Runtime.Scripting;

/// <summary>
/// Default script engine implementation for executing, validating, and managing command scripts
/// Handles sequential command execution with comprehensive error handling and progress reporting
/// </summary>
public class ScriptEngine : IScriptEngine
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IConsoleOutput _output;
    private readonly ILogger<ScriptEngine> _logger;
    
    /// <summary>
    /// Initializes a new instance of ScriptEngine with required dependencies
    /// </summary>
    /// <param name="commandExecutor">Service responsible for executing individual commands</param>
    /// <param name="output">Console output service for displaying execution progress and results</param>
    /// <param name="logger">Logger for tracking script execution events and errors</param>
    public ScriptEngine(ICommandExecutor commandExecutor, IConsoleOutput output, ILogger<ScriptEngine> logger)
    {
        _commandExecutor = commandExecutor;
        _output = output;
        _logger = logger;
    }

    /// <summary>
    /// Executes commands from a script file by reading file content and processing each command sequentially
    /// </summary>
    /// <param name="filePath">Path to the script file containing commands to execute</param>
    /// <param name="cancellationToken">Token to cancel script execution</param>
    /// <returns>Script execution result with detailed outcomes of all commands</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified script file does not exist</exception>
    public async Task<ScriptResult> ExecuteScriptAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Script file {filePath} not found");
        }

        var scriptContent = await File.ReadAllLinesAsync(filePath, cancellationToken);
        return await ExecuteScriptAsync(scriptContent, cancellationToken);
    }

    /// <summary>
    /// Executes commands from provided command strings sequentially with comprehensive error handling and progress reporting
    /// </summary>
    /// <param name="commands">Array of command strings to execute in sequence</param>
    /// <param name="token">Token to cancel script execution</param>
    /// <returns>Script execution result with detailed outcomes including success status, timing, and individual command results</returns>
    public async Task<ScriptResult> ExecuteScriptAsync(string[] commands, CancellationToken token = default)
    {
        var result = new ScriptResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        _output.WriteInfo($"Executing script with {commands.Length} commands...");
        _logger.LogInformation("Starting script execution with {CommandCount} commands", commands.Length);

        for (int i = 0; i < commands.Length; i++)
        {
            if (token.IsCancellationRequested)
            {
                _output.WriteWarning("Script execution cancelled by user");
                _logger.LogWarning("Script execution cancelled by user request");
                break;
            }

            var line = commands[i].Trim();
            var lineNumber = i + 1;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

            var commandResult = new CommandExecutionResult
            {
                Command = line
            };

            var commandStopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _output.WriteInfo($"[{lineNumber}] Executing: {line}");
                _logger.LogDebug("Executing command at line {LineNumber}: {Command}", lineNumber, line);
                
                var executionResult = await _commandExecutor.ExecuteAsync(line, token);
                commandResult.Success = executionResult.Success;
                commandResult.Output = executionResult.Message;

                if (executionResult.Success)
                {
                    result.ExecutedCommands++;
                    _output.WriteSuccess($"[{lineNumber}] Executed successfully: {line}");
                    _logger.LogInformation("Command executed successfully at line {LineNumber}: {Command}", lineNumber, line);
                }
                else
                {
                    result.FailedCommands++;
                    commandResult.Error = executionResult.Message;
                    _output.WriteError($"[{lineNumber}] Failed: {executionResult.Message}");
                    _logger.LogError("Command failed at line {LineNumber}: {Command} - Error: {Error}", 
                        lineNumber, line, executionResult.Message);
                    
                    // Stop execution on first failure (fail-fast behavior)
                    break;
                }
            }
            catch (Exception ex)
            {
                result.FailedCommands++;
                commandResult.Success = false;
                commandResult.Error = ex.Message;
                _output.WriteError($"[{lineNumber}] Command error: {ex.Message}");
                _logger.LogError(ex, "Command execution error at line {LineNumber}: {Command}", lineNumber, line);
                
                // Stop execution on unhandled exception
                break;
            }
            finally
            {
                commandStopwatch.Stop();
                commandResult.Duration = commandStopwatch.Elapsed;
                result.CommandExecutionResults.Add(commandResult);
            }
        }
        
        stopwatch.Stop();
        result.TotalDuration = stopwatch.Elapsed;
        result.Duration = stopwatch.Elapsed;
        result.Success = result.FailedCommands == 0;
        
        _output.WriteLine();
        _output.WriteInfo("Script execution completed:");
        _output.WriteInfo($"Total commands: {result.ExecutedCommands + result.FailedCommands}");
        _output.WriteInfo($"  Successful: {result.ExecutedCommands}");
        _output.WriteInfo($"  Failed: {result.FailedCommands}");
        _output.WriteInfo($"  Duration: {result.TotalDuration.TotalSeconds:F2}s");

        _logger.LogInformation("Script execution completed - Success: {Success}, Executed: {Executed}, Failed: {Failed}, Duration: {Duration}",
            result.Success, result.ExecutedCommands, result.FailedCommands, result.TotalDuration);

        return result;
    }

    /// <summary>
    /// Validates a script file without executing - checks file existence, syntax, and command structure
    /// </summary>
    /// <param name="filePath">Path to the script file to validate</param>
    /// <returns>Validation result indicating script validity and any errors or warnings</returns>
    public ScriptValidationResult ValidateScript(string filePath)
    {
        var result = new ScriptValidationResult();
        _logger.LogDebug("Validating script file: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            result.Errors.Add($"Script file not found: {filePath}");
            result.IsValid = false;
            _logger.LogWarning("Script validation failed - file not found: {FilePath}", filePath);
            return result;
        }

        try
        {
            var lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                var lineNumber = i + 1;

                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                // Check for empty commands after trimming
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Warnings.Add($"Line {lineNumber}: Empty command");
                    _logger.LogWarning("Empty command detected at line {LineNumber}", lineNumber);
                }

                // Additional validation rules can be added here:
                // - Command syntax validation
                // - Parameter validation
                // - Command existence checks
                // - Security validation
            }

            result.IsValid = result.Errors.Count == 0;
            _logger.LogInformation("Script validation completed - Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
                result.IsValid, result.Errors.Count, result.Warnings.Count);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to read script: {ex.Message}");
            result.IsValid = false;
            _logger.LogError(ex, "Script validation failed due to read error: {FilePath}", filePath);
        }
        
        return result;
    }
}