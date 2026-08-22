namespace FilterComparison.Data;

/// <summary>
/// One input series. Ordering and column layout differ per file, so both are declared
/// explicitly rather than inferred.
/// </summary>
internal sealed record DatasetSpec(
    string Id,
    string Label,
    string RelativePath,
    int ValueColumn,
    bool HasHeader,
    bool ReverseFileOrder)
{
    /// <summary>
    /// Fixed iteration order — no dictionary enumeration anywhere in an output path.
    /// MSFT_1day_sample.txt is excluded deliberately: it holds 10 rows, which leaves
    /// nothing after the window warm-up and Savitzky-Golay edge trim.
    /// </summary>
    internal static readonly IReadOnlyList<DatasetSpec> All = new[]
    {
        new DatasetSpec("ecg", "ECG", "RSquaredAdaptiveEma.Demo/Data/ECG.csv", 0, true, false),

        // AMD.csv is stored newest-first, so it must be reversed before a causal filter
        // walks it, otherwise the filter runs backwards through time.
        new DatasetSpec("amd", "AMD close", "Optimizer.Demo/Data/AMD.csv", 1, true, true),

        new DatasetSpec("msft-1min", "MSFT 1min", "RSquaredAdaptiveEma.Demo/Data/MSFT_1min_sample.txt", 4, false, false),
        new DatasetSpec("msft-5min", "MSFT 5min", "RSquaredAdaptiveEma.Demo/Data/MSFT_5min_sample.txt", 4, false, false),
        new DatasetSpec("msft-30min", "MSFT 30min", "RSquaredAdaptiveEma.Demo/Data/MSFT_30min_sample.txt", 4, false, false),
        new DatasetSpec("msft-1hour", "MSFT 1hour", "RSquaredAdaptiveEma.Demo/Data/MSFT_1hour_sample.txt", 4, false, false),
    };
}
