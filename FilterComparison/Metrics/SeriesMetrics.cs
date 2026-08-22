namespace FilterComparison.Metrics;

internal static class SeriesMetrics
{
    /// <summary>
    /// Roughness ratio: RMS of the filtered signal's first differences over the raw signal's.
    /// Dimensionless, so it is comparable across datasets. Identity filter scores 1.0;
    /// lower means more noise suppressed. Descriptive only, and degenerate as a quality measure —
    /// a constant output scores ~0, a "perfect" result. Ranking is done by NextStepScore.
    /// </summary>
    internal static double RoughnessRatio(double[] filtered, double[] raw, int start, int end)
    {
        var num = RmsOfDifferences(filtered, start, end);
        var den = RmsOfDifferences(raw, start, end);
        return den == 0 ? double.NaN : num / den;
    }

    private static double RmsOfDifferences(double[] values, int start, int end)
    {
        double sum = 0;
        var count = 0;
        for (var i = start; i < end; i++)
        {
            var d = values[i + 1] - values[i];
            sum += d * d;
            count++;
        }

        return count == 0 ? double.NaN : Math.Sqrt(sum / count);
    }

    /// <summary>
    /// How far the filter's output escapes the range of the very samples it was computed from,
    /// as a fraction of that range. A smoothing filter is a weighted average of its window, so it
    /// can never leave it and scores 0. A filter that extrapolates — such as a least-squares fit
    /// evaluated at the edge of its own support — will exceed it on sharp transients.
    ///
    /// This is what the roughness ratio misses: RMS of first differences measures average
    /// high-frequency content, and is blind to a few large transient excursions.
    /// </summary>
    internal static (double Mean, double Max) Overshoot(double[] filtered, double[] raw, int windowSize, int start, int end)
    {
        double sum = 0;
        double max = 0;
        var count = 0;

        for (var i = start; i <= end; i++)
        {
            double lo = double.MaxValue, hi = double.MinValue;
            for (var j = i - windowSize + 1; j <= i; j++)
            {
                if (raw[j] < lo) lo = raw[j];
                if (raw[j] > hi) hi = raw[j];
            }

            var range = hi - lo;
            if (range <= 0) continue;

            var excess = Math.Max(0, Math.Max(filtered[i] - hi, lo - filtered[i])) / range;
            sum += excess;
            if (excess > max) max = excess;
            count++;
        }

        return count == 0 ? (double.NaN, double.NaN) : (sum / count, max);
    }

    /// <summary>
    /// Live tracking error: how well the filter's output at time n predicts the observation at
    /// n+horizon, scored against the same prediction made by the raw last sample. Below 1 means
    /// the filter beats "just use the latest value"; above 1 means it is worse than not filtering.
    ///
    /// This is the honest online criterion, and unlike the other metrics it is a single number
    /// that is not degenerate. Lag hurts it — a filter that trails is predicting the past. Noise
    /// hurts it too — a jittery estimate misses the next sample. And it needs no zero-phase
    /// reference, so nothing in it depends on data the filter could not have had.
    /// </summary>
    internal static double NextStepScore(double[] filtered, double[] raw, int start, int end, int horizon = 1)
    {
        double filterSum = 0;
        double baselineSum = 0;
        var count = 0;

        for (var i = start; i + horizon <= end; i++)
        {
            var target = raw[i + horizon];

            var fe = filtered[i] - target;
            filterSum += fe * fe;

            var be = raw[i] - target;
            baselineSum += be * be;

            count++;
        }

        if (count == 0 || baselineSum <= 0) return double.NaN;

        return Math.Sqrt(filterSum / count) / Math.Sqrt(baselineSum / count);
    }

    internal static double Mean(double[] values)
    {
        double sum = 0;
        foreach (var v in values)
        {
            sum += v;
        }

        return sum / values.Length;
    }

    internal static double StdDev(double[] values)
    {
        var mean = Mean(values);
        double sum = 0;
        foreach (var v in values)
        {
            sum += (v - mean) * (v - mean);
        }

        return Math.Sqrt(sum / values.Length);
    }
}
