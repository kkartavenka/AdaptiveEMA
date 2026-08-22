using AdaptiveEMA;
using FilterComparison;
using FilterComparison.Charting;
using FilterComparison.Data;
using FilterComparison.Filters;
using FilterComparison.Metrics;
using FilterComparison.Reporting;

const int WindowSize = 21;

// Polynomial orders under comparison, for both the adaptive filter and causal Savitzky-Golay.
int[] polyOrders = { 2, 3 };

if (args.Contains("--experiment"))
{
    Experiment.Run(WindowSize);
    return 0;
}

if (args.Contains("--selfcheck"))
{
    return SelfCheck() ? 0 : 1;
}

var imagesDir = RepoPaths.Resolve("docs/images");
Directory.CreateDirectory(imagesDir);

var results = new List<DatasetResult>();
var skipped = new List<string>();

foreach (var spec in DatasetSpec.All)
{
    var raw = SeriesLoader.Load(spec);

    // Only the window warm-up is trimmed. Nothing here reads ahead, so there is no edge to cut
    // at the right-hand end.
    var i0 = WindowSize - 1;
    var i1 = raw.Length - 1;

    if (i1 - i0 < 64)
    {
        skipped.Add($"{spec.Label} (N={raw.Length}: too short to score after the {WindowSize}-sample warm-up)");
        Console.WriteLine($"SKIP  {spec.Label}: N={raw.Length}");
        continue;
    }

    var filters = new List<LiveFilterResult>();
    var outputs = new Dictionary<string, double[]>();
    var (sliceStart, sliceEnd) = Charts.PickSlice(raw, i0, i1);

    LiveFilterResult Evaluate(string name, string shortName, string kind, int polyOrder, double[] output, double[]? alphas)
    {
        var overshoot = SeriesMetrics.Overshoot(output, raw, WindowSize, i0, i1);
        return new LiveFilterResult(
            name, shortName, kind, polyOrder,
            SeriesMetrics.RoughnessRatio(output, raw, i0, i1),
            SeriesMetrics.NextStepScore(output, raw, i0, i1),
            SeriesMetrics.NextStepScore(output, raw, i0, i1, 5),
            overshoot.Mean, overshoot.Max,
            alphas is null ? null : SeriesMetrics.Mean(alphas),
            alphas is null ? null : SeriesMetrics.StdDev(alphas));
    }

    foreach (var polyOrder in polyOrders)
    {
        var adaptive = new AdaptiveEmaFilter(new RunParameters(WindowSize, polyOrder), $"AdaptiveEMA (order {polyOrder})");
        var output = FilterRunner.RunCausal(raw, adaptive);

        // Cross-checks the mirrored alpha against the filter's own output on every window.
        var alphas = AlphaMirror.Trace(raw, adaptive, i0, i1);

        filters.Add(Evaluate(adaptive.Name, $"AdaptiveEMA o{polyOrder}", LiveFilterResult.AdaptiveKind, polyOrder, output, alphas));
        outputs[$"adaptive-{polyOrder}"] = output;

        if (spec.Id == "ecg")
        {
            File.WriteAllText(
                Path.Combine(imagesDir, $"ecg-alpha-order{polyOrder}.svg"),
                Charts.AlphaTrace(
                    $"ECG — realized alpha, polynomial order {polyOrder}",
                    "alpha rises where the local polynomial fit is good, falls where it is not",
                    alphas, i0, sliceStart, sliceEnd));
        }
    }

    foreach (var polyOrder in polyOrders)
    {
        var causal = new CausalSavitzkyGolay(WindowSize, polyOrder);
        var output = FilterRunner.RunCausal(raw, causal);

        filters.Add(Evaluate(causal.Name, $"causal S-G o{polyOrder}", LiveFilterResult.CausalSgKind, polyOrder, output, null));
        outputs[$"causal-sg-{polyOrder}"] = output;
    }

    // Strongest non-adaptive baseline: the fixed alpha that is best at live prediction on this
    // dataset. Tuning it per dataset means the adaptive filters are never flattered by a badly
    // chosen constant — this baseline has an advantage no live user would actually have.
    var bestAlpha = double.NaN;
    var bestScore = double.MaxValue;
    for (var t = 0; t <= 96; t++)
    {
        var alpha = 0.02 + (0.99 - 0.02) * t / 96.0;
        var score = SeriesMetrics.NextStepScore(
            FilterRunner.RunCausal(raw, new WindowedEma(alpha, WindowSize)), raw, i0, i1);
        if (score < bestScore)
        {
            bestScore = score;
            bestAlpha = alpha;
        }
    }

    var bestEma = new WindowedEma(bestAlpha, WindowSize);
    var bestEmaOut = FilterRunner.RunCausal(raw, bestEma);
    filters.Add(Evaluate($"fixed-alpha EMA (alpha={bestAlpha:0.##}, tuned here)", "best fixed EMA",
        LiveFilterResult.BaselineKind, 0, bestEmaOut, null));
    outputs["best-ema"] = bestEmaOut;

    results.Add(new DatasetResult(spec.Id, spec.Label, raw.Length, i1 - i0 + 1, filters));

    var liveSeries = new List<(string, double[], SeriesStyle)>
    {
        ("AdaptiveEMA o2", outputs["adaptive-2"], Palette.Adaptive),
        ("AdaptiveEMA o3", outputs["adaptive-3"], Palette.AdaptiveAlt),
        ("causal S-G o2", outputs["causal-sg-2"], Palette.CausalSg),
        ("causal S-G o3", outputs["causal-sg-3"], Palette.CausalSgAlt),
        ($"best fixed EMA (alpha={bestAlpha:0.##})", outputs["best-ema"], Palette.FixedEma),
    };

    File.WriteAllText(
        Path.Combine(imagesDir, $"{spec.Id}-live.svg"),
        Charts.Overlay(
            $"{spec.Label} — live filtering, no filter reads a future sample",
            $"window {WindowSize}, samples {sliceStart}–{sliceEnd}",
            raw, liveSeries, sliceStart, sliceEnd));

    Console.WriteLine($"OK  {spec.Label,-11} " + string.Join("  ", filters.Select(f => $"{f.ShortName}={f.LiveScore1:0.000}")));
}

// One summary chart across every dataset.
var chartSeries = new List<(string, string)>
{
    ("AdaptiveEMA o2", Palette.AdaptiveColor),
    ("AdaptiveEMA o3", Palette.AdaptiveAltColor),
    ("causal S-G o2", Palette.CausalSgColor),
    ("causal S-G o3", Palette.CausalSgAltColor),
    ("best fixed EMA", Palette.FixedEmaColor),
};

File.WriteAllText(
    Path.Combine(imagesDir, "live-scores.svg"),
    Charts.LiveScores(
        "Live prediction score — lower is better",
        "each filter's output at n, scored as a prediction of the raw value at n+1, relative to using the raw value at n",
        results.Select(r => r.Label).ToList(),
        chartSeries,
        results.Select(r => (IReadOnlyList<double>)chartSeries
            .Select(cs => r.Filters.FirstOrDefault(f => f.ShortName == cs.Item1)?.LiveScore1 ?? double.NaN)
            .ToList()).ToList()));

File.WriteAllText(RepoPaths.Resolve("docs/comparison.md"), MarkdownReport.Build(results, skipped, WindowSize));

Console.WriteLine();
Console.WriteLine($"wrote docs/comparison.md and {Directory.GetFiles(imagesDir, "*.svg").Length} SVG charts");
return 0;

bool SelfCheck()
{
    var ok = true;

    foreach (var spec in DatasetSpec.All)
    {
        var series = SeriesLoader.Load(spec);
        var start = WindowSize - 1;
        var end = series.Length - 1;
        if (end - start < 64)
        {
            Console.WriteLine($"{spec.Label,-12} skipped (too short)");
            continue;
        }

        foreach (var polyOrder in polyOrders)
        {
            var filter = new AdaptiveEmaFilter(new RunParameters(WindowSize, polyOrder));
            var trace = AlphaMirror.Trace(series, filter, start, end);
            Console.WriteLine($"{spec.Label,-12} order {polyOrder}: alpha mirror agrees with Transform on {trace.Length} windows");
        }
    }

    Console.WriteLine($"(mirror tolerance {AlphaMirror.Tolerance:E0})");

    var raw = SeriesLoader.Load(DatasetSpec.All[0]);
    var i0 = WindowSize - 1;
    var i1 = raw.Length - 1;

    // An alpha of 1 puts all weight on the newest sample, so the filter must be the identity,
    // and the identity must score exactly 1.0 on the live test by construction.
    var identity = FilterRunner.RunCausal(raw, new WindowedEma(1.0, WindowSize));
    for (var i = i0; i <= i1; i++)
    {
        if (Math.Abs(identity[i] - raw[i]) > 1e-12)
        {
            Console.Error.WriteLine($"identity check failed at {i}");
            ok = false;
            break;
        }
    }

    var identityScore = SeriesMetrics.NextStepScore(identity, raw, i0, i1);
    Console.WriteLine($"identity filter: rho={SeriesMetrics.RoughnessRatio(identity, raw, i0, i1):0.######} (expect 1), " +
                      $"live score={identityScore:0.########} (expect 1)");
    if (Math.Abs(identityScore - 1) > 1e-9) { Console.Error.WriteLine("identity live score is not 1"); ok = false; }

    // A polynomial fit of order >= 1 reproduces a ramp exactly, at the endpoint as anywhere else.
    var ramp = Enumerable.Range(0, WindowSize).Select(i => 3.5 * i - 7).ToArray();
    foreach (var polyOrder in polyOrders)
    {
        var value = new CausalSavitzkyGolay(WindowSize, polyOrder).Transform(ramp);
        Console.WriteLine($"causal Savitzky-Golay order {polyOrder} on a ramp: {value:0.########} (expect {ramp[^1]})");
        if (Math.Abs(value - ramp[^1]) > 1e-8) { Console.Error.WriteLine("ramp reproduction failed"); ok = false; }
    }

    Console.WriteLine(ok ? "SELFCHECK PASSED" : "SELFCHECK FAILED");
    return ok;
}
