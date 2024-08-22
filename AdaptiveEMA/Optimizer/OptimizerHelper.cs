namespace AdaptiveEMA.Optimizer
{
    public class OptimizerHelper
    {
        private readonly OptimizerParameters _parameters;
        
        public OptimizerHelper(OptimizerParameters parameters)
        {
            _parameters = parameters;
        }

        public RunParameters FindParameters()
        {
            var optimizer = new Optimizer(_parameters);
            var result = optimizer.FindMinimum();

            return new RunParameters(
                    windowSize: _parameters.WindowSize,
                    polyOrder: _parameters.PolyOrder,
                    minScale: result[0],
                    maxScale: result[1],
                    swallowValidation: false);
        }
    }
}