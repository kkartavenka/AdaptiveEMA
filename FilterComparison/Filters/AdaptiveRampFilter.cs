using MathNet.Numerics;

namespace FilterComparison.Filters;

/// <summary>
/// The library's R²-driven adaptation, but alpha selects the span of a truncated linear ramp
/// instead of an exponential decay rate. Weights stay non-negative, so the convex-combination
/// bound — and therefore the zero-overshoot guarantee — is preserved.
/// </summary>
internal sealed class AdaptiveRampFilter : IFilter
{
    private readonly double[] _x;
    private readonly int _polyOrder;

    internal AdaptiveRampFilter(int windowSize, int polyOrder)
    {
        WindowSize = windowSize;
        _polyOrder = polyOrder;

        _x = new double[windowSize];
        for (var i = 0; i < windowSize; i++)
        {
            _x[i] = i;
        }
    }

    public string Name => $"AdaptiveRamp (order {_polyOrder})";

    public int WindowSize { get; }

    public double Transform(double[] window)
    {
        var coef = Fit.Polynomial(_x, window, _polyOrder);

        var mean = 0d;
        foreach (var v in window)
        {
            mean += v;
        }

        mean /= window.Length;

        double ssTotal = 0, ssResidual = 0;
        for (var i = 0; i < window.Length; i++)
        {
            var d = window[i] - mean;
            ssTotal += d * d;
            var r = window[i] - Polynomial.Evaluate(_x[i], coef);
            ssResidual += r * r;
        }

        var rSquared = ssTotal <= 0 ? 0 : Math.Clamp(1 - ssResidual / ssTotal, 0, 1);

        // alpha = 1 (perfect local fit) collapses the ramp onto the newest sample; alpha = 0
        // opens it to the full window, the maximally smoothed end.
        var span = 1 + (1 - rSquared) * (WindowSize - 1);

        double sum = 0, weightSum = 0;
        for (var i = 0; i < window.Length; i++)
        {
            var age = window.Length - 1 - i;
            var w = Math.Max(0, span - age);
            sum += window[i] * w;
            weightSum += w;
        }

        return sum / weightSum;
    }
}
