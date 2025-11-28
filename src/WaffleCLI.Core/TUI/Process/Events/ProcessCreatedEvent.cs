using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Process.Events;

public record ProcessCreatedEvent(IProcessComponent Process);