using WaffleCLI.Abstractions.TUI;

namespace WaffleCLI.Core.TUI.Process.Events;

public record ProcessExitedEvent(IProcessComponent Process, int ExitCode);