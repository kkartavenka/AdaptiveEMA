using System;
using AdaptiveEMA.Shared;

namespace AdaptiveEMA
{
    public class RSquaredAdaptive
    {
        private readonly RunParameters _parameters;
        private readonly WeightsProvider _weightsProvider;
        private readonly double[] _x;

        public RSquaredAdaptive(RunParameters parameters)
        {
            _parameters = parameters;
            _weightsProvider = new WeightsProvider(parameters.WindowSize);
            
            _x = new double[parameters.WindowSize];
            for (var i = 0; i < _x.Length; i++)
            {
                _x[i] = i;
            }
        }

        public double Transform(double[] values)
        {
            if (values.Length == _parameters.WindowSize)
            {
                return GetLastValue(values);
            }
            
            if (values.Length > _parameters.WindowSize)
            {
                var selectedArray = new double[_parameters.WindowSize];
                Array.Copy(values, (values.Length - _parameters.WindowSize), selectedArray, 0, _parameters.WindowSize);
                return GetLastValue(selectedArray);
            }

            // In case not sufficient data points, just return the last one
            if (_parameters.SwallowValidation)
            {
                return values[values.Length - 1];
            }

            throw new ArgumentOutOfRangeException(nameof(values), values.Length,
                $"Should be larger than window size, windowSize: {_parameters.WindowSize}, array length: {values.Length}");
        }
        
        private double GetLastValue(double[] values)
        {
            var decayCoef = GetDecayCoef(values);
            var weights = _weightsProvider.GetDecayWeights(decayCoef);
            return values.WeightedMean(weights);
        }

        private double GetDecayCoef(double[] values)
        {
            var poly = PolynomialRegression.Get(_x, values, _parameters.PolyOrder);
            var rSquared = poly.RSquared(_x, values);
            
            return rSquared.ScaleTo(0, 1, _parameters.MinScale, _parameters.MaxScale);
        }
    }
}