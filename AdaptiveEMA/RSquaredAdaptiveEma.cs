using Accord.Statistics;
using AdaptiveEMA.Shared;

namespace AdaptiveEMA;

public class RSquaredAdaptiveEma
{
    private const double DEFAULT_SMOOTHING_FACTOR_MIN = 0;
    private const double DEFAULT_SMOOTHING_FACTOR_MAX = 1;

    private const int DEFAULT_POLY_ORDER = 2;

    private readonly int _windowSize = -1;
    private int _lowerPolyOrder, _polyOrder;

    private readonly double _smoothingFactorMin, _smoothingFactorMax;

    private WeightsProvider _weightProvider;

    public RSquaredAdaptiveEma()
    {
        (_smoothingFactorMax, _smoothingFactorMin) = (DEFAULT_SMOOTHING_FACTOR_MAX, DEFAULT_SMOOTHING_FACTOR_MIN);
        Initialize();
    }

    public RSquaredAdaptiveEma(double smoothingFactorMin, double smoothingFactorMax, int windowSize)
    {
        (_smoothingFactorMin, _smoothingFactorMax) = (smoothingFactorMin, smoothingFactorMax);
        ValidateSmoothingFactor(smoothingFactorMin, smoothingFactorMax);
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
            } else if (_weightProvider.GetWindowSize() < selectedRange.Length)
            {
                _weightProvider.UpdateWindowSize(_windowSize);
                selectedRange = selectedRange[(selectedRange.Length - _windowSize)..];
            }

        var rsquaredAdjustment = RSquaredAdjustment(sequence: selectedRange)
            .ScaleTo(0, 1, _smoothingFactorMin, _smoothingFactorMax);

        var weights = _weightProvider.GetDecayWeights(rsquaredAdjustment);
        var weightedMean = Measures.WeightedMean(selectedRange, weights);
        return weightedMean;
    }

    private static void ValidateSmoothingFactor(double smoothingFactorMin, double smoothingFactorMax)
    {
        if (smoothingFactorMin < 0 || smoothingFactorMin > 1)
            throw new ArgumentOutOfRangeException(nameof(smoothingFactorMin), "The value of {1} is out of range [0, 1]");

        if (smoothingFactorMax < 0 || smoothingFactorMax > 1)
            throw new ArgumentOutOfRangeException(nameof(smoothingFactorMax), "The value of {1} is out of range [0, 1]");

        if (smoothingFactorMin >= smoothingFactorMax)
            throw new Exception($"{nameof(smoothingFactorMax)} should be more than {nameof(smoothingFactorMin)}");
    }

    private double RSquaredAdjustment(double[] sequence)
    {
        if (sequence.Length < _polyOrder + 2)
            return 0;

        return sequence.GetRSquared(polyOrder: _polyOrder);
    }

    public void SetPolyOrder(int value)
    {
        if (value < 1)
            throw new ArgumentOutOfRangeException(nameof(value), "The value should be greater then 0");

        _polyOrder = value;
    }
}