using Accord.Statistics;
using AdaptiveEMA.Shared;

namespace AdaptiveEMA;

public class RSquaredAdaptive
{
    private const int DEFAULT_POLY_ORDER = 2;

    private readonly double _smoothingFactorMin, _smoothingFactorMax;
    private readonly int _windowSize = -1;
    private int _polyOrder;

    private WeightsProvider _weightProvider;

    public RSquaredAdaptive()
    {
        Initialize();
    }

    public RSquaredAdaptive(double smoothingFactorMin, double smoothingFactorMax, int windowSize)
    {
        (_smoothingFactorMin, _smoothingFactorMax) = (smoothingFactorMin, smoothingFactorMax);
        _windowSize = windowSize;
        Initialize();
    }

    private void Initialize()
    {
        _weightProvider = new(_windowSize);
        _polyOrder = DEFAULT_POLY_ORDER;
    }

    public double GetLastValue(double[] sequence)
    {
        var selectedRange = sequence;
        if (_windowSize != -1)
            if (_windowSize > selectedRange.Length)
            {
                _weightProvider.UpdateWindowSize(selectedRange.Length);
            }
            else if (_weightProvider.GetWindowSize() < selectedRange.Length)
            {
                _weightProvider.UpdateWindowSize(_windowSize);
                selectedRange = selectedRange[(selectedRange.Length - _windowSize)..];
            }

        if (selectedRange.Length < _polyOrder + 2)
            return selectedRange.Last();

        var unweightedRegression = selectedRange.GetRegression(_polyOrder);
        var rsquared = selectedRange.GetRSquared(unweightedRegression)
            .ScaleTo(0, 1, _smoothingFactorMin, _smoothingFactorMax);

        var unweightedExpectation = unweightedRegression.Transform(selectedRange.Length - 1);
        var emaExpectation = selectedRange.WeightedMean(_weightProvider.GetDecayWeights(rsquared));

        return unweightedExpectation * (1 - rsquared) + emaExpectation * rsquared;
    }
}