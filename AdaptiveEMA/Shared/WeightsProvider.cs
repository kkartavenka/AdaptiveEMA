using System;
using System.Linq;

namespace AdaptiveEMA.Shared
{
    internal class WeightsProvider
    {
        private readonly int _windowSize;

        internal WeightsProvider(int windowSize)
        {
            _windowSize = windowSize;
        }

        private double[] GetSpecificCase(double decayFactor)
        {
            if (_windowSize == 1)
            {
                return new double[] { 1 };
            }

            if (decayFactor < 0)
            {
                decayFactor = 0;
            }

            switch (decayFactor)
            {
                case 0:
                    return Enumerable.Range(1, _windowSize)
                        .Select(m => (double)1)
                        .ToArray();
                case 1:
                {
                    var sequence = Enumerable.Range(1, _windowSize)
                        .Select(m => (double)0)
                        .ToArray();
                    sequence[sequence.Length - 1] = 1;
                    return sequence;
                }
                default:
                    return null;
            }
        }

        internal double[] GetDecayWeights(double alpha)
        {
            var specificCase = GetSpecificCase(alpha);
            if (specificCase != null)
            {
                return specificCase;
            }

            var decay = 1 - alpha;
            var decayWeights = new double[_windowSize];

            double decayRow = 1;
            for (var i = _windowSize - 1; i >= 0; i--)
            {
                decayWeights[i] = decayRow;
                decayRow *= decay;
            }

            return decayWeights;
        }
    }
}