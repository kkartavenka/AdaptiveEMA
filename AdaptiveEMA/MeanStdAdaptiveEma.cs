using Accord.Statistics;
using AdaptiveEMA.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveEMA;

internal class MeanStdAdaptiveEma
{
    private const double DEFAULT_SMOOTHING_FACTOR_MIN = 0;
    private const double DEFAULT_SMOOTHING_FACTOR_MAX = 1;

    private WeightsProvider _weightProvider;

    private int _period1, _period2, _windowSize;
    private double _scaleMin, _scaleMax;
    internal MeanStdAdaptiveEma(int windowSize, int period1, int period2, double scaleMin, double scaleMax)
    {
        if (windowSize <= Math.Max(period1, period2))
            throw new Exception($"{nameof(windowSize)} should ");

        _windowSize = windowSize;
        _weightProvider = new(_windowSize);

        (_period1, _period2, _scaleMin, _scaleMax) = (period1, period2, scaleMin, scaleMax);
        
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

}
