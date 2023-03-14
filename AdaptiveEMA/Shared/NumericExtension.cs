namespace AdaptiveEMA.Shared;

internal static class NumericExtension
{
    internal static double ScaleTo(this double value, double observedMinX, double observedMaxX, double expectedMinX, double expectedMaxX) =>
        expectedMinX + (value - observedMinX) * (expectedMaxX - expectedMinX) / (observedMaxX - observedMinX);

}
