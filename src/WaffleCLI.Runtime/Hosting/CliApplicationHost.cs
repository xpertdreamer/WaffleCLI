using Microsoft.Extensions.Logging;
using WaffleCLI.Abstractions.Hosting;

namespace WaffleCLI.Runtime.Hosting;

public class CliApplicationHost : IApplicationHost
{
    private readonly IConsoleHost _consoleHost;

    public CliApplicationHost(
        IConsoleHost consoleHost)
    {
        _consoleHost = consoleHost;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _consoleHost.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApplicationException(ex.Message, ex);
        }
    }
}