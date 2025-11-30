using WaffleCLI.Abstractions.TUI.Rendering;
using WaffleCLI.Abstractions.TUI.Components;

namespace WaffleCLI.Core.TUI.Components.Base
{
    /// <summary>
    /// Base class for container components
    /// </summary>
    public abstract class ContainerBase : ComponentBase, IContainer
    {
        protected ContainerBase(string id) : base(id)
        {
        }

        public abstract void DoLayout();

        public override void AddChild(IComponent child)
        {
            base.AddChild(child);
            DoLayout();
        }

        public override void RemoveChild(IComponent child)
        {
            base.RemoveChild(child);
            DoLayout();
        }
    }
}