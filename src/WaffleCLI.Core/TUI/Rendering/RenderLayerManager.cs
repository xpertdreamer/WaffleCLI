using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Rendering;

public class RenderLayerManager
{
    private readonly List<RenderLayer> _layers = [];
    private readonly IRenderEngine _renderEngine;

    public RenderLayerManager(IRenderEngine renderEngine)
    {
        _renderEngine = renderEngine;
    }

    public void AddLayer(string layerName, int layerPriority, bool isVisible = true)
    {
        var layer = new RenderLayer(layerName, layerPriority, isVisible);
        _layers.Add(layer);
        _layers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    public void RemoveLayer(string layerName)
    {
        _layers.RemoveAll(l => l.Name == layerName);
    }

    public void AddElementsToLayer(string layerName, ITuiElement element)
    {
        var layer = _layers.FirstOrDefault(l => l.Name == layerName);
        if (layer == null)
        {
            AddLayer(layerName, 1);
            AddElementsToLayer(layerName, element);
        }
        layer?.Elements.Add(element);
    }

    public void RemoveElementFromLayer(string layerName, ITuiElement element)
    {
        var layer = _layers.FirstOrDefault(l => l.Name == layerName);
        layer?.Elements.Remove(element);
    }
    
    public void RenderAllLayers()
    {
        foreach (var layer in _layers.Where(l => l.IsVisible))
        {
            RenderLayer(layer);
        }
    }

    public void RenderLayer(string layerName)
    {
        var layer = _layers.FirstOrDefault(l => l.Name == layerName);
        if (layer != null)
        {
            RenderLayer(layer);
        }
    }
    
    private void RenderLayer(RenderLayer layer)
    {
        foreach (var element in layer.Elements.Where(e => e.isVisible))
        {
            _renderEngine.RenderElement(element);
        }
    }
    
    public IEnumerable<RenderLayer> GetLayers() => _layers.AsReadOnly();
}

public class RenderLayer
{
    public string Name { get; }
    public int Priority { get; }
    public bool IsVisible { get; set; }
    public List<ITuiElement> Elements { get; } = [];
    
    public RenderLayer(string name, int priority, bool isVisible = true)
    {
        Name = name;
        Priority = priority;
        IsVisible = isVisible;
    }
}