namespace FilterComparison.Filters;

/// <summary>
/// Reimplementation of AdaptiveEMA.Shared.WeightsProvider and MathExtensions.WeightedMean,
/// which are internal with no InternalsVisibleTo.
///
/// Keeping the baseline EMA on this exact functional form is deliberate: the only difference
/// between the adaptive filter and the fixed-alpha baseline is then whether alpha varies,
/// so the comparison isolates the adaptation and nothing else.
/// </summary>
internal static class Weighting
{
    internal static double[] DecayWeights(int windowSize, double alpha)
    {
        if (windowSize == 1)
        {
            return new double[] { 1 };
        }

        if (alpha < 0)
        {
            alpha = 0;
        }

        if (alpha == 0)
        {
            // Flat weights: a plain unweighted mean over the window.
            var flat = new double[windowSize];
            Array.Fill(flat, 1d);
            return flat;
        }

        if (alpha == 1)
        {
            // Degenerates to the identity filter: all weight on the newest sample.
            var spike = new double[windowSize];
            spike[windowSize - 1] = 1;
            return spike;
        }

        var decay = 1 - alpha;
        var weights = new double[windowSize];

        double row = 1;
        for (var i = windowSize - 1; i >= 0; i--)
        {
            weights[i] = row;
            row *= decay;
        }

        return weights;
    }

    internal static double WeightedMean(double[] values, double[] weights)
    {
        if (values.Length != weights.Length)
        {
            throw new ArgumentException("values and weights have different length", nameof(weights));
        }

        var sumProd = 0d;
        var sumWeights = 0d;
        for (var i = 0; i < values.Length; i++)
        {
            sumProd += values[i] * weights[i];
            sumWeights += weights[i];
        }

        return sumProd / sumWeights;
    }
}
