using Accord.Math.Optimization.Losses;
using Accord.Statistics.Models.Regression.Linear;
using System.Runtime.CompilerServices;

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

    internal static double GetRSquared(this double[] y, PolynomialRegression polyRegression)
    {
        var x = new double[y.Length];
        for (var i = 0; i != x.Length; i++)
            x[i] = i;
        return new RSquaredLoss(numberOfInputs: 1, expected: y).Loss(polyRegression.Transform(x));
    }

    internal static PolynomialRegression? GetRegression(this double[] y, int polyOrder, double[]? weights = null)
    {
        if (!y.Any())
            throw new ArgumentException("Argument must have values", nameof(y));

        if (polyOrder + 1 >= y.Length)
            return null;

        var x = new double[y.Length];
        for (var i = 0; i != x.Length; i++)
            x[i] = i;

        var ls = new PolynomialLeastSquares()
        {
            Degree = polyOrder
        };

        if (weights == null)
            return ls.Learn(x, y);

        return ls.Learn(x, y, weights);

    }
}
