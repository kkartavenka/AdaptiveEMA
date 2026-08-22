using AdaptiveEMA;
using MathNet.Numerics;

namespace FilterComparison.Filters;

/// <summary>
/// Recomputes the decay coefficient the adaptive filter derives internally, so the realized
/// alpha can be reported. AdaptiveEMA's helpers are internal with no InternalsVisibleTo, so
/// the derivation is duplicated here.
///
/// Duplicated logic drifts silently, so every window is cross-checked: the weighted mean
/// computed from the mirrored alpha must equal RSquaredAdaptive.Transform to 1e-9. If that
/// assert ever fails, the reported alpha is not the alpha the filter actually used.
/// </summary>
internal static class AlphaMirror
{
    internal const double Tolerance = 1e-9;

    internal static double AlphaFor(double[] window, RunParameters parameters)
    {
        var x = new double[window.Length];
        for (var i = 0; i < x.Length; i++)
        {
            x[i] = i;
        }

        var coef = Fit.Polynomial(x, window, parameters.PolyOrder);
        var rSquared = RSquared(coef, x, window);

        return ScaleTo(rSquared, 0, 1, parameters.MinScale, parameters.MaxScale);
    }

    /// <summary>Mirrors MathExtensions.RSquared(coef, independentVar, observedValues).</summary>
    private static double RSquared(double[] coef, double[] x, double[] y)
    {
        var mean = y.Average();

        double ssTotal = 0;
        double ssResidual = 0;
        for (var i = 0; i < y.Length; i++)
        {
            ssTotal += Math.Pow(y[i] - mean, 2);
            ssResidual += Math.Pow(y[i] - Polynomial.Evaluate(x[i], coef), 2);
        }

        return 1 - ssResidual / ssTotal;
    }

    /// <summary>Mirrors NumericExtension.ScaleTo. Note it does not clamp.</summary>
    private static double ScaleTo(double value, double observedMin, double observedMax, double expectedMin, double expectedMax)
    {
        var minFrom = Math.Min(observedMin, observedMax);
        var maxFrom = Math.Max(observedMin, observedMax);
        var minTo = Math.Min(expectedMax, expectedMin);
        var maxTo = Math.Max(expectedMax, expectedMin);

        return minTo + (value - minFrom) * (maxTo - minTo) / (maxFrom - minFrom);
    }

    /// <summary>
    /// Returns the realized alpha per window over [start, end], verifying each one against the
    /// filter's own output. Throws if the mirror and the filter ever disagree.
    /// </summary>
    internal static double[] Trace(double[] values, AdaptiveEmaFilter filter, int start, int end)
    {
        var parameters = filter.Parameters;
        var w = parameters.WindowSize;
        var alphas = new double[end - start + 1];

        for (var i = start; i <= end; i++)
        {
            var window = new double[w];
            Array.Copy(values, i - w + 1, window, 0, w);

            var alpha = AlphaFor(window, parameters);
            alphas[i - start] = alpha;

            var mirrored = Weighting.WeightedMean(window, Weighting.DecayWeights(w, alpha));
            var actual = filter.Transform(window);

            if (Math.Abs(mirrored - actual) > Tolerance)
            {
                throw new InvalidOperationException(
                    $"Alpha mirror disagrees with the filter at index {i}: mirrored {mirrored:R}, actual {actual:R}, alpha {alpha:R}");
            }
        }

        return alphas;
    }
}
