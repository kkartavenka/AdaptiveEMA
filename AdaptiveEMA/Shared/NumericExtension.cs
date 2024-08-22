using System;

namespace AdaptiveEMA.Shared
{
    internal static class NumericExtension
    {
        internal static double ScaleTo(this double value, double observedMinX, double observedMaxX, double expectedMinX,
            double expectedMaxX)
        {
            var minFrom = Math.Min(observedMinX, observedMaxX);
            var maxFrom = Math.Max(observedMinX, observedMaxX);

            var minTo = Math.Min(expectedMaxX, expectedMinX);
            var maxTo = Math.Max(expectedMaxX, expectedMinX);

            return minTo + (value - minFrom) * (maxTo - minTo) / (maxFrom - minFrom);
            
            //return expectedMinX + (value - observedMinX) * (expectedMaxX - expectedMinX) / (observedMaxX - observedMinX);
        }
    }
}