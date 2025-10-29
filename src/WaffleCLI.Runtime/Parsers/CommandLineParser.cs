using System.Text;

namespace WaffleCLI.Runtime.Parsers;

/// <summary>
/// Parses command line arguments
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public static string[] Parse(string commandLine)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes =  false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            var c = commandLine[i];
            if (c == '"')
                inQuotes = !inQuotes;
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
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