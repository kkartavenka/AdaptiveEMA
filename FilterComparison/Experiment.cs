using AdaptiveEMA;
using FilterComparison.Data;
using FilterComparison.Filters;
using FilterComparison.Metrics;

namespace FilterComparison;

/// <summary>
/// Live evaluation: every filter here is strictly causal, and the score itself uses no
/// future-looking reference. Each filter's output at time n is scored as a prediction of the
/// observation at n+h, relative to the same prediction made by the raw last sample.
/// </summary>
internal static class Experiment
{
    internal static void Run(int windowSize)
    {
        Console.WriteLine("Live test — score = RMSE(filter output at n, raw at n+h) / RMSE(raw at n, raw at n+h).");
        Console.WriteLine("Below 1.000 beats 'just use the latest sample'. Lower is better. No zero-phase reference used.");
        Console.WriteLine();

        var horizons = new[] { 1, 5 };

        foreach (var spec in DatasetSpec.All)
        {
            var raw = SeriesLoader.Load(spec);
            var i0 = windowSize - 1;
            var i1 = raw.Length - 1;
            if (i1 - i0 < 64) continue;

            var candidates = new List<(string Name, IFilter Filter)>
            {
                ("AdaptiveEMA o2", new AdaptiveEmaFilter(new RunParameters(windowSize, 2))),
                ("AdaptiveEMA o3", new AdaptiveEmaFilter(new RunParameters(windowSize, 3))),
                ("causal S-G o2", new CausalSavitzkyGolay(windowSize, 2)),
                ("causal S-G o3", new CausalSavitzkyGolay(windowSize, 3)),
                ("AdaptiveRamp o2", new AdaptiveRampFilter(windowSize, 2)),
                ("AdaptiveRamp o3", new AdaptiveRampFilter(windowSize, 3)),
                ("AdaptiveBlend o3", new AdaptiveBlendFilter(windowSize, 3)),
            };

            // Best fixed-alpha EMA and best ramp, tuned per dataset per horizon: the strongest
            // non-adaptive baselines, so the adaptive filters are not flattered by a bad constant.
            Console.WriteLine($"=== {spec.Label}   (n = {i1 - i0 + 1})");

            foreach (var h in horizons)
            {
                var rows = new List<(string Name, double Score)>();

                foreach (var (name, filter) in candidates)
                {
                    var output = FilterRunner.RunCausal(raw, filter);
                    rows.Add((name, SeriesMetrics.NextStepScore(output, raw, i0, i1, h)));
                }

                (string, double) BestOf(string label, Func<double, IFilter> build, double lo, double hi)
                {
                    var best = double.MaxValue;
                    var bestParam = lo;
                    for (var t = 0; t <= 60; t++)
                    {
                        var p = lo + (hi - lo) * t / 60.0;
                        var score = SeriesMetrics.NextStepScore(FilterRunner.RunCausal(raw, build(p)), raw, i0, i1, h);
                        if (score < best) { best = score; bestParam = p; }
                    }

                    return ($"{label} (best={bestParam:0.##})", best);
                }

                rows.Add(BestOf("fixed-alpha EMA", a => FixedWeightFilter.Exponential(windowSize, a), 0.02, 0.99));
                rows.Add(BestOf("ramp", s => FixedWeightFilter.Ramp(windowSize, s), 1.0, windowSize * 1.5));

                Console.WriteLine($"  horizon {h}:");
                foreach (var (name, score) in rows.OrderBy(r => r.Score))
                {
                    var flag = score < 1 ? "  beats raw" : "  WORSE than raw";
                    Console.WriteLine($"    {name,-28} {score,7:0.0000}{flag}");
                }
            }

            Console.WriteLine();
        }
    }
}
