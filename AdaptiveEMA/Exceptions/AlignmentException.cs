using System;

namespace AdaptiveEMA.Exceptions
{
    internal class AlignmentException : Exception
    {
        private AlignmentException(string message) : base(message)
        {
        }

        internal static AlignmentException Create(int comparisonLength, int trainSetLength)
        {
            var message =
                $"Comparison and Train sets should have same length. Comparison length: {comparisonLength}, train set length: {trainSetLength}";

            return new AlignmentException(message);
        }
    }
}