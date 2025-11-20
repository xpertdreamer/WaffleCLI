namespace WaffleCLI.Abstractions.Hosting;

public interface IApplicationHost
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}