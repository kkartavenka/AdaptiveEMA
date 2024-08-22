using System;
using AdaptiveEMA.Exceptions;
using AdaptiveEMA.Shared;

namespace AdaptiveEMA.Optimizer
{
    public class OptimizerBuilder
    {
        private const int DefaultMaxIterations = 1_000;
    
        private double[] _comparisonSet;
        private int _polyOrder;
        private Func<double[], double[], double> _scoreEvaluationFunc;
        private double[] _trainSet;
        private int _windowSize;

        private int _skipLeft;
        private int _skipRight;
        private double _convergenceTolerance;
        private int _maxIterations;

        private bool _useDefaultSimpleParameters;

        /// <summary>
        ///     Uses Savitzky-Golay filter
        /// </summary>
        /// <returns></returns>
        public OptimizerBuilder UseDefaultComparison(double[] rawValues, int savitzkyGolaySidePoints, int savitzkyGolayPolyOrder = 2)
        {
            var sgFilter = new SavitzkyGolayFilter(savitzkyGolaySidePoints, savitzkyGolayPolyOrder);
            _comparisonSet = sgFilter.Process(rawValues);
            _trainSet = rawValues;
            _skipLeft = savitzkyGolaySidePoints;
            _skipRight = savitzkyGolaySidePoints;

            return this;
        }

        public OptimizerBuilder WithComparisonSet(double[] comparisonValues)
        {
            _comparisonSet = comparisonValues;
            return this;
        }

        public OptimizerBuilder WithTrainSet(double[] trainSet)
        {
            _trainSet = trainSet;
            return this;
        }
        public OptimizerBuilder WithAlgoParameters(int windowSize, int polyOrder)
        {
            (_windowSize, _polyOrder) = (windowSize, polyOrder);

            return this;
        }

        public OptimizerBuilder UseDefaultScoreEvaluation()
        {
            _scoreEvaluationFunc = (expected, observed) => expected.RSquared(observed);
            return this;
        }

        public OptimizerBuilder WithScoreEvaluation(Func<double[], double[], double> func)
        {
            _scoreEvaluationFunc = func;
            return this;
        }

        public OptimizerBuilder WithSimplexParameters(double convergenceTolerance, int maxIterations = DefaultMaxIterations)
        {
            if (maxIterations <= 0)
            {
                throw new ArgumentException("Should be larger than 0", nameof(maxIterations));
            }

            _useDefaultSimpleParameters = false;
            _convergenceTolerance = convergenceTolerance;
            _maxIterations = maxIterations;

            return this;
        }

        public OptimizerBuilder UseDefaultSimplexParameters()
        {
            _useDefaultSimpleParameters = true;
            return this;
        }

        public OptimizerParameters Build()
        {
            if (_windowSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_windowSize), _windowSize, $"Use {nameof(WithAlgoParameters)} to set");
            }
        
            if (_polyOrder < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_polyOrder), _polyOrder, $"Use {nameof(WithAlgoParameters)} to set polynomial order to more or equal 2");
            }
        
            if (_comparisonSet.Length != _trainSet.Length)
            {
                throw AlignmentException.Create(_comparisonSet.Length, _trainSet.Length);
            }

            if (_useDefaultSimpleParameters)
            {
                _maxIterations = DefaultMaxIterations;
                _convergenceTolerance = 1e-6;
            }

            if (_windowSize > _skipLeft)
            {
                _skipLeft = _windowSize;
            }
        
            return new OptimizerParameters(
                comparisonSet: _comparisonSet,
                trainSet: _trainSet,
                scoreEvaluationFunc: _scoreEvaluationFunc,
                polyOrder: _polyOrder,
                windowSize: _windowSize,
                skipLeft: _skipLeft,
                skipRight: _skipRight,
                maxIterations: _maxIterations,
                convergenceTolerance: _convergenceTolerance);
        }
    }
}