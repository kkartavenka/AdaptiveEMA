using System;
using MathNet.Numerics.LinearAlgebra;

namespace AdaptiveEMA.Optimizer
{
    public class SavitzkyGolayFilter
    {
        private readonly int _sidePoints;
        private Matrix<double> _coefficients;

        public SavitzkyGolayFilter(int sidePoints, int polynomialOrder)
        {
            _sidePoints = sidePoints;
            Design(polynomialOrder);
        }
    
        public double[] Process(double[] samples)
        {
            var length = samples.Length;
            var output = new double[length];
            var frameSize = GetFrameSize();
            var frame = new double[frameSize];

            Array.Copy(samples, frame, frameSize);

            for (var i = 0; i < _sidePoints; ++i)
            {
                output[i] = _coefficients.Column(i).DotProduct(Vector<double>.Build.DenseOfArray(frame));
            }

            for (var n = _sidePoints; n < length - _sidePoints; ++n)
            {
                Array.ConstrainedCopy(samples, n - _sidePoints, frame, 0, frameSize);
                output[n] = _coefficients.Column(_sidePoints).DotProduct(Vector<double>.Build.DenseOfArray(frame));
            }

            Array.ConstrainedCopy(samples, length - frameSize, frame, 0, frameSize);

            for (var i = 0; i < _sidePoints; ++i)
            {
                output[length - _sidePoints + i] = _coefficients.Column(_sidePoints + 1 + i).DotProduct(Vector<double>.Build.Dense(frame));
            }

            return output;
        }

        private int GetFrameSize() => (_sidePoints << 1) + 1;

        private void Design(int polynomialOrder)
        {
            var a = new double[GetFrameSize(), polynomialOrder + 1];

            for (var m = -_sidePoints; m <= _sidePoints; ++m)
            {
                for (int i = 0; i <= polynomialOrder; ++i)
                {
                    a[m + _sidePoints, i] = Math.Pow(m, i);
                }
            }

            var s = Matrix<double>.Build.DenseOfArray(a);
            _coefficients = s.Multiply(s.TransposeThisAndMultiply(s).Inverse()).Multiply(s.Transpose());
        }
    }
}