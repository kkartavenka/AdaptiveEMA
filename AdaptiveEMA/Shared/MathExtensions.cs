using System;
using System.Linq;
using MathNet.Numerics;

namespace AdaptiveEMA.Shared
{
    internal static class MathExtensions
    {
        internal static double WeightedMean(this double[] values, double[] weights)
        {
            if (values.Length != weights.Length)
            {
                throw new ArgumentException($"{nameof(values)} and {nameof(weights)} have different length", nameof(weights));
            }
            
            var sumProd = 0d;
            var sumWeights = 0d;
            for (var i = 0; i < values.Length; i++)
            {                
                sumProd += values[i] * weights[i];
                sumWeights += weights[i];
            }

            return sumProd / sumWeights;
        }

        internal static double RSquared(this double[] coef, double[] independentVar, double[] observedValues)
        {
            var meanObserved = observedValues.Average();
            var ssTotal = observedValues.Select(y => Math.Pow(y - meanObserved, 2)).Sum();
            var ssResidual = observedValues.Zip(independentVar, (y, x) => Math.Pow(y - Polynomial.Evaluate(x, coef), 2))
                .Sum();

            return 1 - ssResidual / ssTotal;
        }

        internal static double RSquared(this double[] observed, double[] expected)
        {
            var meanObserved = observed.Average();

            double sst = 0;
            double sse = 0;
            for (var i = 0; i < observed.Length; i++)
            {
                sst += Math.Pow(observed[i] - meanObserved, 2);
                sse += Math.Pow(observed[i] - expected[i], 2);
            }

            return 1 - sse / sst;
        }
    }
}