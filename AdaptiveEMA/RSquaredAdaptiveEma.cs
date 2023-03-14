using Accord.Statistics;
using AdaptiveEMA.Shared;

namespace AdaptiveEMA;

public class RSquaredAdaptiveEma
{
    private const double DEFAULT_SMOOTHING_FACTOR_MIN = 0;
    private const double DEFAULT_SMOOTHING_FACTOR_MAX = 1;

    private const int DEFAULT_LOWER_POLY_ORDER = 1;
    private const int DEFAULT_HIGHER_POLY_ORDER = 2;

    private double _rsquaredMin, _rsquaredMax;
    private readonly int _windowSize = -1;
    private int _lowerPolyOrder, _higherPolyOrder;

    private double _scaleMin, _scaleMax;
    private bool _isFinalScale = false;
    private readonly double _smoothingFactorMin, _smoothingFactorMax;

    private WeightsProvider _weightProvider;

    public RSquaredAdaptiveEma()
    {
        (_smoothingFactorMax, _smoothingFactorMin) = (DEFAULT_SMOOTHING_FACTOR_MAX, DEFAULT_SMOOTHING_FACTOR_MIN);
        Initialize();
    }

    public RSquaredAdaptiveEma(double smoothingFactorMin, double smoothingFactorMax, int windowSize)
    {
        (_smoothingFactorMax, _smoothingFactorMin) = (smoothingFactorMin, smoothingFactorMax);
        ValidateSmoothingFactor(smoothingFactorMin, smoothingFactorMax);
        _windowSize = windowSize;
        Initialize();
    }

    private void Initialize()
    {
        _weightProvider = new(_windowSize);
        (_rsquaredMin, _rsquaredMax) = (0, 1 - _smoothingFactorMin);
        (_lowerPolyOrder, _higherPolyOrder) = (DEFAULT_LOWER_POLY_ORDER, DEFAULT_HIGHER_POLY_ORDER);
    }

    public double GetLastValue(double[] sequence, bool simpleEma = false)
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

        var rsquaredAdjustment = 0d;

        if (!simpleEma)
            rsquaredAdjustment = RSquaredAdjustment(sequence: selectedRange);

        var currentSmoothingFactor = _smoothingFactorMin + rsquaredAdjustment;
        if (currentSmoothingFactor > _smoothingFactorMax)
            currentSmoothingFactor = _smoothingFactorMax;

        if (_isFinalScale && !simpleEma)
            currentSmoothingFactor = currentSmoothingFactor.ScaleTo(_smoothingFactorMin, _smoothingFactorMax, _scaleMin, _scaleMax);

        var weights = _weightProvider.GetDecayWeights(currentSmoothingFactor);
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
        if (sequence.Length < _higherPolyOrder + 2)
            return 0;

        var rsquaredLinear = sequence.GetRSquared(polyOrder: _lowerPolyOrder);
        var rsquaredQuadratic = sequence.GetRSquared(polyOrder: _higherPolyOrder);

        var rsquaredDistance = rsquaredQuadratic - rsquaredLinear;

        return rsquaredDistance.ScaleTo(observedMinX: 0, observedMaxX: 1, expectedMinX: _rsquaredMin, expectedMaxX: _rsquaredMax);
    }

    public void SetRSquaredAdjustmentBoundaries(double min, double max) => (_rsquaredMin, _rsquaredMax) = (min, max);
    public void SetLowerPolyOrder(int value) => _lowerPolyOrder = value;
    public void SetHigherPolyOrder(int value) => _higherPolyOrder = value;
    public void SetScalingSmoothingFactorRange(double min, double max)
    {
        if (min < 0 || min > 1)
            throw new ArgumentOutOfRangeException(nameof(min));

        if (max < 0 || max > 1)
            throw new ArgumentOutOfRangeException(nameof(max));

        if (min >= max)
            throw new Exception($"{nameof(max)} shoud be more than {nameof(min)}");

        (_scaleMin, _scaleMax) = (min, max);
        _isFinalScale = true;
    }

}