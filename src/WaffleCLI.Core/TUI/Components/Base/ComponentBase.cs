using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Base implementation of IComponent with boundary checking
    /// </summary>
    public abstract class ComponentBase : IComponent
    {
        private readonly List<IComponent> _children = new();
        private bool _disposed = false;
        private IComponent? _parent;

        public string Id { get; }
        public virtual int X { get; set; }
        public virtual int Y { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual bool IsVisible { get; set; } = true;
        public virtual bool IsEnabled { get; set; } = true;
        public IComponent? Parent => _parent;
        public IReadOnlyList<IComponent> Children => _children.AsReadOnly();

        /// <summary>
        /// Gets absolute X position on screen
        /// </summary>
        public int AbsoluteX => X + (Parent is ComponentBase parentBase ? parentBase.AbsoluteX : 0);

        /// <summary>
        /// Gets absolute Y position on screen
        /// </summary>
        public int AbsoluteY => Y + (Parent is ComponentBase parentBase ? parentBase.AbsoluteY : 0);

        /// <summary>
        /// Minimum allowed width for this component
        /// </summary>
        protected virtual int MinWidth => 1;

        /// <summary>
        /// Minimum allowed height for this component
        /// </summary>
        protected virtual int MinHeight => 1;

        protected ComponentBase(string id)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        /// <summary>
        /// Validates if component fits within parent bounds
        /// </summary>
        public virtual bool ValidateBounds()
        {
            if (Parent is ComponentBase parent)
            {
                if (X < 0 || Y < 0 || X + Width > parent.Width || Y + Height > parent.Height)
                {
                    Infrastructure.Logging.TuiLogger.LogWarning(
                        $"Component {Id} exceeds parent bounds: " +
                        $"X={X}, Y={Y}, W={Width}, H={Height} " +
                        $"Parent: W={parent.Width}, H={parent.Height}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Ensures component stays within parent bounds
        /// </summary>
        public virtual void ClampToParentBounds()
        {
            if (Parent is ComponentBase parent)
            {
                X = Math.Max(0, Math.Min(X, parent.Width - MinWidth));
                Y = Math.Max(0, Math.Min(Y, parent.Height - MinHeight));
                Width = Math.Max(MinWidth, Math.Min(Width, parent.Width - X));
                Height = Math.Max(MinHeight, Math.Min(Height, parent.Height - Y));
            }
        }

        public virtual void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;
            
            // Render children
            foreach (var child in _children.Where(c => c.IsVisible))
            {
                child.Render(renderEngine);
            }
        }

        public virtual void Update()
        {
            foreach (var child in _children)
            {
                child.Update();
            }
        }

        public virtual void AddChild(IComponent child)
        {
            if (child.Parent != null)
            {
                throw new ComponentException(Id, $"Cannot add child {child.Id} - it already has a parent");
            }
            
            if (child is ComponentBase childBase)
            {
                childBase.SetParent(this);
                childBase.ClampToParentBounds();
            }
            else
            {
                throw new ComponentException(Id, $"Child component must inherit from ComponentBase");
            }
            
            _children.Add(child);
        }

        public virtual void RemoveChild(IComponent child)
        {
            if (_children.Remove(child) && child is ComponentBase childBase)
            {
                childBase.SetParent(null);
            }
        }

        internal void SetParent(IComponent? parent)
        {
            _parent = parent;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var child in _children)
                    {
                        child.Dispose();
                    }
                    _children.Clear();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~ComponentBase()
        {
            Dispose(false);
        }
    }
}