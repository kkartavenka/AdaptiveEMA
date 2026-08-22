using MathNet.Numerics.LinearAlgebra;

namespace FilterComparison.Filters;

/// <summary>
/// Savitzky-Golay used causally: the polynomial is least-squares fitted to the trailing window and
/// evaluated at its newest sample rather than its centre. No future samples are read, so unlike the
/// centered form this one is usable on a live stream.
///
/// This is not the library's <see cref="AdaptiveEMA.Optimizer.SavitzkyGolayFilter"/>, which is
/// centered and offline. It exists so the "live" comparison has a Savitzky-Golay entrant that plays
/// by the same rules as the EMAs.
///
/// Evaluating a fit at the edge of its own support is extrapolation-like, so this trades noise
/// amplification for very low lag — the opposite bias to an EMA.
/// </summary>
internal sealed class CausalSavitzkyGolay : IFilter
{
    private readonly double[] _weights;

    internal CausalSavitzkyGolay(int windowSize, int polyOrder)
    {
        if (polyOrder + 1 >= windowSize)
        {
            throw new ArgumentOutOfRangeException(nameof(polyOrder), polyOrder, "Must be less than windowSize - 1");
        }

        WindowSize = windowSize;
        PolyOrder = polyOrder;

        // x is centred on the window purely for conditioning: raw powers up to (W-1)^polyOrder
        // make the normal-equation matrix needlessly ill-conditioned.
        var half = (windowSize - 1) / 2.0;

        var design = Matrix<double>.Build.Dense(windowSize, polyOrder + 1, (i, j) => Math.Pow(i - half, j));
        var transpose = design.Transpose();
        var pseudoInverse = (transpose * design).Inverse() * transpose;

        // Evaluate at the newest sample, i = windowSize - 1, i.e. x = +half.
        var endpoint = Vector<double>.Build.Dense(polyOrder + 1, j => Math.Pow(half, j));

        _weights = pseudoInverse.LeftMultiply(endpoint).ToArray();
    }

    internal int PolyOrder { get; }

    public string Name => $"Causal Savitzky-Golay (order {PolyOrder})";

    public int WindowSize { get; }

    /// <summary>The weights reproduce a constant exactly, so they already sum to 1.</summary>
    public double Transform(double[] window)
    {
        var sum = 0d;
        for (var i = 0; i < window.Length; i++)
        {
            sum += window[i] * _weights[i];
        }

        return sum;
    }
}
