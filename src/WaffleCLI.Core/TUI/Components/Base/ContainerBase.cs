using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Base class for containers with layout capabilities
    /// </summary>
    public abstract class ContainerBase : ComponentBase, IContainer
    {
        private bool _needsLayout = true;

        protected ContainerBase(string id) : base(id)
        {
        }

        /// <summary>
        /// Marks container as needing layout update
        /// </summary>
        public void InvalidateLayout()
        {
            _needsLayout = true;
        }

        public abstract void DoLayout();

        public override void Render(IRenderEngine renderEngine)
        {
            if (_needsLayout)
            {
                DoLayout();
                _needsLayout = false;
            }
            
            base.Render(renderEngine);
        }

        public override void AddChild(IComponent child)
        {
            base.AddChild(child);
            InvalidateLayout();
        }

        public override void RemoveChild(IComponent child)
        {
            base.RemoveChild(child);
            InvalidateLayout();
        }

        /// <summary>
        /// Updates children positions to fit within container
        /// </summary>
        protected void FitChildrenToContainer()
        {
            foreach (var child in Children.OfType<ComponentBase>())
            {
                child.ClampToParentBounds();
            }
        }
    }
}