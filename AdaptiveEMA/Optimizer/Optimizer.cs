using System;
using AdaptiveEMA.Exceptions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace AdaptiveEMA.Optimizer
{
    internal class Optimizer
    {
        private static Func<double[], double[], double> _scoreTransformer;
        private static int _skipLeft;
        private static int _skipRight;
        private static int _windowSize;
        private static int _polyOrder;
        private static double[] _values;
        private static double[] _comparisonValues;
        private readonly NelderMeadSimplex _optimizer;

        private readonly Func<Vector<double>, double> _theFunc = parameters =>
        {
            var runParams = new RunParameters(
                _windowSize,
                _polyOrder,
                parameters[0],
                parameters[1],
                false);

            var filter = new RSquaredAdaptive(runParams);
            var transformed = new double[_values.Length - _windowSize];

            for (var i = _windowSize; i < _values.Length; i++)
            {
                var window = new double[_windowSize];
                var startIdx = i - _windowSize + 1;
                Array.Copy(_values, startIdx, window, 0, _windowSize);
                transformed[i - _windowSize] = filter.Transform(window);
            }

            var observedValueLength = transformed.Length - _skipLeft - _skipRight;
            var observedValues = new double[observedValueLength];
            Array.Copy(transformed, _skipLeft, observedValues, 0, observedValueLength);

            var eval = 1 - _scoreTransformer.Invoke(_comparisonValues, observedValues);
            BoundaryCorrection(parameters[0], ref eval);
            BoundaryCorrection(parameters[1], ref eval);
            ProximityCorrection(parameters[0], parameters[1], ref eval);
            return eval;
        };

        internal Optimizer(OptimizerParameters parameters)
        {
            _optimizer = new NelderMeadSimplex(parameters.ConvergenceTolerance, parameters.MaxIterations);
            _scoreTransformer = parameters.ScoreEvaluationFunc;
            _skipLeft = parameters.SkipLeft - parameters.WindowSize;
            _skipRight = parameters.SkipRight;
            _windowSize = parameters.WindowSize;
            _polyOrder = parameters.PolyOrder;
            _values = parameters.TrainSet;

            var copyLength = parameters.ComparisonSet.Length - parameters.SkipLeft - parameters.SkipRight;
            _comparisonValues = new double[copyLength];
            Array.Copy(parameters.ComparisonSet, parameters.SkipLeft, _comparisonValues, 0, copyLength);
        }

        internal double[] FindMinimum()
        {
            var initialGuess = Vector<double>.Build.DenseOfArray(new[] { 0.25, 0.75 });
            var initPerturbation = Vector<double>.Build.DenseOfArray(new[] { 0.1, 0.1 });
            try
            {
                var result = _optimizer.FindMinimum(ObjectiveFunction.Value(_theFunc), initialGuess, initPerturbation);
                return result.MinimizingPoint.ToArray();
            }
            catch (MaximumIterationsException e)
            {
                throw new OptimizerException(
                    $"Maximum iterations reached. Either increase MaxIteration count or ConvergenceTolerance", e);
            }
        }

        private static void BoundaryCorrection(double value, ref double score)
        {
            if (value >= 0 && value <= 1)
            {
                return;
            }

            var multiplier = value < 0 ? Math.Abs(value) + 1 : value;
            var modifier = Math.Exp(multiplier * Math.Abs(value));
            if (modifier > double.MaxValue / 100)
            {
                modifier = double.MaxValue / 100;
            }

            score += modifier;
        }

        private static void ProximityCorrection(double minScale, double maxScale, ref double score)
        {
            var proximity = maxScale - minScale;

            if (proximity <= 0)
            {
                score += Math.Exp(10 * Math.Abs(proximity)) - 1;
            }
        }
    }
}