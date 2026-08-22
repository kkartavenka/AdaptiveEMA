namespace FilterComparison.Filters;

/// <summary>Causal filter with an arbitrary fixed weight vector, index 0 oldest. Used to compare weight shapes.</summary>
internal sealed class FixedWeightFilter : IFilter
{
    private readonly double[] _weights;

    internal FixedWeightFilter(double[] weights, string name)
    {
        var sum = weights.Sum();
        _weights = weights.Select(w => w / sum).ToArray();
        Name = name;
        WindowSize = weights.Length;
    }

    public string Name { get; }

    public int WindowSize { get; }

    public double Transform(double[] window)
    {
        var sum = 0d;
        for (var i = 0; i < window.Length; i++)
        {
            sum += window[i] * _weights[i];
        }

        return sum;
    }

    /// <summary>Exponential decay — the shape <c>WeightsProvider</c> builds. w ∝ (1-alpha)^age.</summary>
    internal static FixedWeightFilter Exponential(int windowSize, double alpha)
    {
        var w = new double[windowSize];
        for (var i = 0; i < windowSize; i++)
        {
            w[i] = Math.Pow(1 - alpha, windowSize - 1 - i);
        }

        return new FixedWeightFilter(w, $"exponential alpha={alpha:0.###}");
    }

    /// <summary>
    /// Truncated linear ramp: w ∝ max(0, span - age).
    ///
    /// This is the minimiser of DC group delay among non-negative weightings at a fixed noise
    /// gain. Stationarity of  sum(w_i*d_i) + lambda*sum(w_i) + mu*sum(w_i^2)  gives
    /// w_i = -(d_i + lambda)/(2*mu), i.e. weights linear in age, clipped at zero.
    /// </summary>
    internal static FixedWeightFilter Ramp(int windowSize, double span)
    {
        var w = new double[windowSize];
        for (var i = 0; i < windowSize; i++)
        {
            var age = windowSize - 1 - i;
            w[i] = Math.Max(0, span - age);
        }

        if (w.Sum() <= 0)
        {
            w[windowSize - 1] = 1;
        }

        return new FixedWeightFilter(w, $"ramp span={span:0.##}");
    }
}
