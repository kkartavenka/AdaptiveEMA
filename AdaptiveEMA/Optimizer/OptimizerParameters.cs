using System;

namespace AdaptiveEMA.Optimizer
{
    public class OptimizerParameters
    {
        public OptimizerParameters(
            double[] comparisonSet, 
            double[] trainSet, 
            Func<double[], double[], double> scoreEvaluationFunc, 
            int polyOrder, 
            int windowSize, 
            int skipLeft, 
            int skipRight,
            int maxIterations,
            double convergenceTolerance)
        {
            SkipLeft = skipLeft;
            SkipRight = skipRight;
            ComparisonSet = comparisonSet;
            TrainSet = trainSet;
            ScoreEvaluationFunc = scoreEvaluationFunc;
            PolyOrder = polyOrder;
            WindowSize = windowSize;
            ConvergenceTolerance = convergenceTolerance;
            MaxIterations = maxIterations;
        }
    
        public double[] ComparisonSet { get; }
        internal double[] TrainSet { get; }
        internal Func<double[], double[], double> ScoreEvaluationFunc { get; }
        internal int PolyOrder { get; }
        internal int WindowSize { get; }
        internal int SkipLeft { get; }
        internal int SkipRight { get; }
        internal double ConvergenceTolerance { get; private set; }
        internal int MaxIterations { get; private set; }
    }
}