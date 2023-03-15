using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;

namespace AdaptiveEMA.Shared;

internal static class RegressionExtension
{
    internal static double GetRSquared(this double[] y, int polyOrder)
    {
        if (!y.Any())
            throw new ArgumentException("Argument must have values", nameof(y));

        if (polyOrder + 1 >= y.Length)
            return 1;

        var x = new double[y.Length];
        for (var i = 0; i != x.Length; i++)
            x[i] = i;

        var ls = new PolynomialLeastSquares()
        {
            Degree = polyOrder
        };
        var poly = ls.Learn(x, y);
        return new RSquaredLoss(numberOfInputs: 1, expected: y).Loss(poly.Transform(x));
    }
}
