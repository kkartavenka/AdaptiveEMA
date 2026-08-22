namespace FilterComparison.Filters;

/// <summary>
/// Fixed-alpha exponential moving average over a trailing window, using the same truncated
/// and renormalized weighting the adaptive filter uses. This is the baseline the adaptive
/// filter is measured against.
/// </summary>
internal sealed class WindowedEma : IFilter
{
    private readonly double[] _weights;

    internal WindowedEma(double alpha, int windowSize)
    {
        Alpha = alpha;
        WindowSize = windowSize;
        _weights = Weighting.DecayWeights(windowSize, alpha);
    }

    internal double Alpha { get; }

    public string Name => $"EMA (alpha={Alpha:0.###})";

    public int WindowSize { get; }

    public double Transform(double[] window) => Weighting.WeightedMean(window, _weights);
}
