using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Process.Events;

public record ProcessOutputEvent(IProcessComponent Process, string Output);