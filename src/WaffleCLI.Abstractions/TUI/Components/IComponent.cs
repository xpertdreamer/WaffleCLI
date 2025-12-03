using WaffleCLI.Abstractions.TUI.Rendering;

namespace WaffleCLI.Abstractions.TUI.Components
{
    /// <summary>
    /// Base interface for all TUI components
    /// </summary>
    public interface IComponent : IDisposable
    {
        string Id { get; }
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        bool IsVisible { get; set; }
        bool IsEnabled { get; set; }
        IComponent? Parent { get; }
        IReadOnlyList<IComponent> Children { get; }
        int AbsoluteX { get; }
        int AbsoluteY { get; }
        
        void Render(IRenderEngine renderEngine);
        void Update();
        void AddChild(IComponent child);
        void RemoveChild(IComponent child);
    }
}