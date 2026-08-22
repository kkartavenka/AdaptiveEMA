namespace FilterComparison.Charting;

/// <summary>
/// Colourblind-safe on white, and each series also carries a distinct dash pattern so the
/// charts stay readable in greyscale.
/// </summary>
internal static class Palette
{
    internal static readonly SeriesStyle Raw = new("#8c959f", 1.0);
    internal static readonly SeriesStyle Adaptive = new("#0969da", 1.9);
    internal static readonly SeriesStyle AdaptiveAlt = new("#8250df", 1.7, "7 2 2 2");
    internal static readonly SeriesStyle FixedEma = new("#cf222e", 1.7, "6 3");
    internal static readonly SeriesStyle CausalSg = new("#bc4c00", 1.7, "4 2");
    internal static readonly SeriesStyle CausalSgAlt = new("#953800", 1.5, "1 2");
    internal static readonly SeriesStyle SavitzkyGolay = new("#1a7f37", 1.6, "2 3");
    internal static readonly SeriesStyle Family = new("#cf222e", 1.6);

    internal const string AdaptiveColor = "#0969da";
    internal const string FixedEmaColor = "#cf222e";
    internal const string SavitzkyGolayColor = "#1a7f37";
    internal const string AdaptiveAltColor = "#8250df";
    internal const string CausalSgColor = "#bc4c00";
    internal const string CausalSgAltColor = "#953800";
}
