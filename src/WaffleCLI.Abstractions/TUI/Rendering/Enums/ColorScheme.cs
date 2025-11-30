namespace WaffleCLI.Abstractions.TUI.Rendering.Enums
{
    /// <summary>
    /// Represents a color scheme for rendering
    /// </summary>
    public struct ColorScheme : IEquatable<ColorScheme>
    {
        public ConsoleColor Foreground { get; set; }
        public ConsoleColor Background { get; set; }

        public ColorScheme(ConsoleColor foreground, ConsoleColor background)
        {
            Foreground = foreground;
            Background = background;
        }

        public static ColorScheme Default => new ColorScheme(ConsoleColor.White, ConsoleColor.Black);
        
        public static ColorScheme Primary => new ColorScheme(ConsoleColor.White, ConsoleColor.DarkBlue);
        public static ColorScheme Secondary => new ColorScheme(ConsoleColor.Gray, ConsoleColor.Black);
        public static ColorScheme Success => new ColorScheme(ConsoleColor.Green, ConsoleColor.Black);
        public static ColorScheme Warning => new ColorScheme(ConsoleColor.Yellow, ConsoleColor.Black);
        public static ColorScheme Error => new ColorScheme(ConsoleColor.Red, ConsoleColor.Black);
        public static ColorScheme Focus => new ColorScheme(ConsoleColor.Black, ConsoleColor.White);

        public override bool Equals(object? obj)
        {
            return obj is ColorScheme scheme && Equals(scheme);
        }

        public bool Equals(ColorScheme other)
        {
            return Foreground == other.Foreground && Background == other.Background;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Foreground, Background);
        }

        public static bool operator ==(ColorScheme left, ColorScheme right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ColorScheme left, ColorScheme right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{Foreground}:{Background}";
        }
    }
}