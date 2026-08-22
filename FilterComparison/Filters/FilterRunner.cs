namespace FilterComparison.Filters;

internal static class FilterRunner
{
    /// <summary>
    /// Slides a trailing window across the series, one filtered value per input sample.
    /// Starts at WindowSize-1, the first index with a full window; AdaptiveEMA's own Optimizer
    /// starts at WindowSize and so discards the first valid output.
    /// Indices before that are NaN and are excluded by the alignment range.
    /// </summary>
    internal static double[] RunCausal(double[] values, IFilter filter)
    {
        var w = filter.WindowSize;
        var output = new double[values.Length];
        Array.Fill(output, double.NaN);

        var window = new double[w];
        for (var i = w - 1; i < values.Length; i++)
        {
            Array.Copy(values, i - w + 1, window, 0, w);
            output[i] = filter.Transform(window);
        }

        return output;
    }
}
