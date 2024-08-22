using Accord.Statistics;

namespace Shared.Filters;

public class SimpleEma : IFilter
{
    private readonly double _smoothing;

    public SimpleEma(double smoothing, int windowSize)
    {
        _smoothing = smoothing;
        WindowSize = windowSize;
    }
    
    public double Transform(double[] values)
    {
        return values.TakeLast(WindowSize)
            .ToArray()
            .ExponentialWeightedMean(_smoothing);
    }

    public int WindowSize { get; }
}