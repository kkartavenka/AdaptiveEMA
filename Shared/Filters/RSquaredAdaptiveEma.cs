using AdaptiveEMA;

namespace Shared.Filters;

public class RSquaredAdaptiveEma : IFilter
{
    private readonly RSquaredAdaptive _filter;

    public RSquaredAdaptiveEma(RunParameters runParams)
    {
        WindowSize = runParams.WindowSize;
        _filter = new RSquaredAdaptive(runParams);
    }
    
    public double Transform(double[] values)
    {
        return _filter.Transform(values);
    }

    public int WindowSize { get; }
}