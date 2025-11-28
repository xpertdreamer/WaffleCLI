namespace WaffleCLI.Abstractions.TUI;

public interface ITuiComponent : ITuiElement
{
    string Id {get;}
    ComponentState State {get;}

    Task OnCreateAsync();
    Task OnRendeAsync();
    Task OnDestroyAsync();
    Task OnResizeAsync(int width, int height);
}

public enum ComponentState
{
    Created,
    Rendering,
    Destroyed
}