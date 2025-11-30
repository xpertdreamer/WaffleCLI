using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;
using WaffleCLI.Abstractions.TUI.Exceptions;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Base implementation for all components
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
        public IComponent? Parent => _parent; // Internal set through method
        public IReadOnlyList<IComponent> Children => _children.AsReadOnly();

        protected ComponentBase(string id)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        }

        public virtual void Render(IRenderEngine renderEngine)
        {
            if (!IsVisible) return;
        
            // CRITICAL FIX: Ensure children are rendered with proper coordinate context
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

        // Internal method to set parent - not exposed in interface
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