namespace AdaptiveEMA.Shared;

internal class WeightsProvider
{
    private int _windowSize;
    internal WeightsProvider(int windowSize) => _windowSize = windowSize;

    private double[]? GetSpecificCase(double decayFactor)
    {
        if (_windowSize == 1)
            return new double[] { 1 };

        if (decayFactor < 0)
            decayFactor = 0;

        if (decayFactor == 0)
            return Enumerable.Range(1, _windowSize).Select(m => (double)1).ToArray();

        if (decayFactor == 1)
        {
            var sequence = Enumerable.Range(1, _windowSize).Select(m => (double)0).ToArray();
            sequence[^1] = 1;

            return sequence;
        }

        return null;
    }

    internal double[] GetDecayWeights(double alpha)
    {
        var specificCase = GetSpecificCase(alpha);
        if (specificCase != null)
            return specificCase;

        double decay = 1 - alpha;
        double[] decayWeights = new double[_windowSize];

        double decayRow = 1;
        for (int i = _windowSize - 1; i >= 0; i--)
        {
            decayWeights[i] = decayRow;
            decayRow *= decay;
        }

        return decayWeights;
    }

    internal void UpdateWindowSize(int windowSize) => _windowSize = windowSize;

    internal int GetWindowSize() => _windowSize;
}
