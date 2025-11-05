using System.Text;

namespace WaffleCLI.Core.Parsers;

/// <summary>
/// Provides functionality for parsing command line arguments with support for quoted parameters
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Parses a command line string into an array of arguments, handling quoted parameters
    /// </summary>
    /// <param name="commandLine">The command line string to parse</param>
    /// <returns>Array of parsed arguments</returns>
    public static string[] Parse(string commandLine)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in commandLine)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length <= 0) continue;
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        
        if (current.Length > 0)
            result.Add(current.ToString());
        
        return result.ToArray();
    }
}