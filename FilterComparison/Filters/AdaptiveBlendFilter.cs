using MathNet.Numerics;

namespace FilterComparison.Filters;

/// <summary>
/// Candidate improvement to <c>RSquaredAdaptive</c>.
///
/// The library already fits a polynomial to every window in order to compute R², then throws the
/// fit away and uses R² only to pick a decay rate. But the fit's value at the newest sample is
/// exactly what a causal Savitzky-Golay filter returns — the thing that currently beats the library
/// on lag. It is already computed, so using it costs nothing.
///
/// R² is a natural blend weight: it is precisely "how much should I trust this local polynomial".
/// High R² leans on the fit endpoint (low lag); low R² falls back to a flat mean (heavy smoothing).
/// Unlike a decay weighting this can produce negative effective weights, so it trades away the
/// convex-combination bound that makes the current filter overshoot-free.
/// </summary>
internal sealed class AdaptiveBlendFilter : IFilter
{
    private readonly double[] _x;
    private readonly int _polyOrder;
    private readonly double _minScale;
    private readonly double _maxScale;

    internal AdaptiveBlendFilter(int windowSize, int polyOrder, double minScale = 0, double maxScale = 1)
    {
        WindowSize = windowSize;
        _polyOrder = polyOrder;
        _minScale = minScale;
        _maxScale = maxScale;

        _x = new double[windowSize];
        for (var i = 0; i < windowSize; i++)
        {
            _x[i] = i;
        }
    }

    public string Name => $"AdaptiveBlend (order {_polyOrder})";

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

        var rSquared = ssTotal <= 0 ? 0 : 1 - ssResidual / ssTotal;

        // Same [0,1] -> [minScale, maxScale] mapping the library uses, but clamped: R² goes
        // negative on a bad fit, and a negative blend weight would push the output away from
        // both candidates rather than between them.
        var weight = _minScale + Math.Clamp(rSquared, 0, 1) * (_maxScale - _minScale);

        var endpoint = Polynomial.Evaluate(_x[^1], coef);
        return weight * endpoint + (1 - weight) * mean;
    }
}
