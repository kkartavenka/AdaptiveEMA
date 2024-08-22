using System;
using MathNet.Numerics.Optimization;

namespace AdaptiveEMA.Exceptions
{
    public class OptimizerException : Exception
    {
        internal OptimizerException(string message, MaximumIterationsException innerException) : base(message,
            innerException)
        {
        }
    }
}