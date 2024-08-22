using System;
using MathNet.Numerics;

namespace AdaptiveEMA.Shared
{
    internal class PolynomialRegression
    {
        private readonly double[] _coef;
        private Polynomial _poly;
        private PolynomialRegression(double[] coefficients)
        {
            _coef = coefficients;
        }
        
        public double RSquared(double[] independentValues, double[] observedValues)
        {
            return _coef.RSquared(independentValues, observedValues);
        }

        public double Transform(double x)
        {
            if (_poly is null)
            {
                _poly = new Polynomial(_coef);
            }

            return _poly.Evaluate(x);
        }

        public static PolynomialRegression Get(double[] y, int polyOrder)
        {
            var x = new double[y.Length];
            for (var i = 0; i != x.Length; i++)
            {
                x[i] = i;
            }

            return Get(x, y, polyOrder);
        }
        public static PolynomialRegression Get(double[] x, double[] y, int polyOrder)
        {
            if (polyOrder < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(polyOrder), polyOrder, "Must be greater or equal 1");
            }

            if (polyOrder + 1 >= y.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(polyOrder), polyOrder, "Should be less then value length");
            }

            return new PolynomialRegression(Fit.Polynomial(x, y, polyOrder));
        }
    }
}