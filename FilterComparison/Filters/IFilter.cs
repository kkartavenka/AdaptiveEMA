namespace FilterComparison.Filters;

/// <summary>
/// Causal, last-value contract: given a trailing window, return the filtered value for its
/// newest sample. Mirrors Shared.Filters.IFilter, with a name added for reporting.
/// </summary>
internal interface IFilter
{
    string Name { get; }
    int WindowSize { get; }
    double Transform(double[] window);
}
