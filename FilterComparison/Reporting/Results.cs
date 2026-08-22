namespace FilterComparison.Reporting;

/// <summary>One strictly causal filter, scored the way it would be used on a live stream.</summary>
internal sealed record LiveFilterResult(
    string Name,
    string ShortName,
    string Kind,
    int PolyOrder,
    double Roughness,
    double LiveScore1,
    double LiveScore5,
    double OvershootMean,
    double OvershootMax,
    double? AlphaMean = null,
    double? AlphaStdDev = null)
{
    internal const string AdaptiveKind = "adaptive";
    internal const string CausalSgKind = "causal-sg";
    internal const string BaselineKind = "baseline";
}

internal sealed record DatasetResult(
    string Id,
    string Label,
    int SampleCount,
    int UsableCount,
    IReadOnlyList<LiveFilterResult> Filters);
