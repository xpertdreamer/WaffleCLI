using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Rendering.Enums;

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
    
    /// <summary>
    /// Fluent API extensions for IComponent
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Sets the component position
        /// </summary>
        public static TComponent WithPosition<TComponent>(this TComponent component, int x, int y) 
            where TComponent : IComponent
        {
            component.X = x;
            component.Y = y;
            return component;
        }
        
        /// <summary>
        /// Sets the component size
        /// </summary>
        public static TComponent WithSize<TComponent>(this TComponent component, int width, int height) 
            where TComponent : IComponent
        {
            component.Width = width;
            component.Height = height;
            return component;
        }
        
        /// <summary>
        /// Sets the component visibility
        /// </summary>
        public static TComponent WithVisibility<TComponent>(this TComponent component, bool isVisible) 
            where TComponent : IComponent
        {
            component.IsVisible = isVisible;
            return component;
        }
        
        /// <summary>
        /// Sets the component enabled state
        /// </summary>
        public static TComponent WithEnabled<TComponent>(this TComponent component, bool isEnabled) 
            where TComponent : IComponent
        {
            component.IsEnabled = isEnabled;
            return component;
        }
        
        /// <summary>
        /// Centers the component horizontally within its parent
        /// </summary>
        public static TComponent CenterHorizontally<TComponent>(this TComponent component) 
            where TComponent : IComponent
        {
            if (component.Parent != null)
            {
                component.X = Math.Max(0, (component.Parent.Width - component.Width) / 2);
            }
            return component;
        }
        
        /// <summary>
        /// Centers the component vertically within its parent
        /// </summary>
        public static TComponent CenterVertically<TComponent>(this TComponent component) 
            where TComponent : IComponent
        {
            if (component.Parent != null)
            {
                component.Y = Math.Max(0, (component.Parent.Height - component.Height) / 2);
            }
            return component;
        }
        
        /// <summary>
        /// Centers the component both horizontally and vertically within its parent
        /// </summary>
        public static TComponent Center<TComponent>(this TComponent component) 
            where TComponent : IComponent
        {
            return component.CenterHorizontally().CenterVertically();
        }
    }
}