using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Abstractions.TUI.Application
{
    /// <summary>
    /// Main TUI application coordinator
    /// </summary>
    public interface ITuiApplication : IDisposable
    {
        IComponent RootComponent { get; }
        bool IsRunning { get; }
        
        void Run();
        void Stop();
        void Refresh();
    }
}