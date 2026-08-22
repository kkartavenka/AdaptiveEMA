using AdaptiveEMA;

namespace FilterComparison.Filters;

internal sealed class AdaptiveEmaFilter : IFilter
{
    private readonly RSquaredAdaptive _inner;

    internal AdaptiveEmaFilter(RunParameters parameters, string? name = null)
    {
        Parameters = parameters;
        WindowSize = parameters.WindowSize;
        Name = name ?? $"AdaptiveEMA [{parameters.MinScale:0.##}, {parameters.MaxScale:0.##}]";
        _inner = new RSquaredAdaptive(parameters);
    }

    internal RunParameters Parameters { get; }

    public string Name { get; }

    public int WindowSize { get; }

    public double Transform(double[] window) => _inner.Transform(window);
}
