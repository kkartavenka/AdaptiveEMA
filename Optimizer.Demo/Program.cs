using AdaptiveEMA.Optimizer;
using Shared;
using Shared.Filters;
using XPlot.Plotly;

var dataRaw = new DataReader("Data/AMD.csv", 1).Data.OrderBy(x => x.Id)
    .ToList();

var windowSize = 20;
var sgSidePoints = 20;
var sgPolyOrder = 2;

var samples = dataRaw.Select(x => x.Raw)
    .ToArray();
var sgFilter = new SavitzkyGolayFilter(sgSidePoints, sgPolyOrder);
var smoothedValues = sgFilter.Process(samples);

var trainSamples = dataRaw.Take((int)(dataRaw.Count * 0.25))
    .Select(x => x.Raw)
    .ToArray();

var optimizerParams = new OptimizerBuilder()
    .UseDefaultComparison(trainSamples, 10, 2)
    .UseDefaultScoreEvaluation()
    .UseDefaultSimplexParameters()
    .WithAlgoParameters(20, 2)
    .Build();

var optimizerParams2 = new OptimizerBuilder()
    .UseDefaultComparison(trainSamples, 15, 2)
    .UseDefaultScoreEvaluation()
    .UseDefaultSimplexParameters()
    .WithAlgoParameters(30, 2)
    .Build();

var optimizerParams3 = new OptimizerBuilder()
    .UseDefaultComparison(trainSamples, 20, 2)
    .UseDefaultScoreEvaluation()
    .UseDefaultSimplexParameters()
    .WithAlgoParameters(40, 2)
    .Build();

var optimizedParams = new OptimizerHelper(optimizerParams).FindParameters();
var optimizedParams2 = new OptimizerHelper(optimizerParams2).FindParameters();
var optimizedParams3 = new OptimizerHelper(optimizerParams3).FindParameters();

IFilter adaptiveEmaOptimized = new RSquaredAdaptiveEma(optimizedParams);
IFilter adaptiveEmaOptimized2 = new RSquaredAdaptiveEma(optimizedParams2);
IFilter adaptiveEmaOptimized3 = new RSquaredAdaptiveEma(optimizedParams3);

var filterList = new List<IFilter>(3)
{
    adaptiveEmaOptimized,
    adaptiveEmaOptimized2,
    adaptiveEmaOptimized3
};
var labelDictionary = new Dictionary<int, string>()
{
    { 0, "Raw" },
    { 1, $"EMA [{optimizedParams.MinScale:N3}, {optimizedParams.MaxScale:N3}], PO: {optimizedParams.PolyOrder}" },
    { 2, $"EMA [{optimizedParams2.MinScale:N3}, {optimizedParams2.MaxScale:N3}], PO: {optimizedParams2.PolyOrder}" },
    { 3, $"EMA [{optimizedParams3.MinScale:N3}, {optimizedParams3.MaxScale:N3}], PO: {optimizedParams3.PolyOrder}" },
    { 4, "Savitzky-Golay"}
};

var results = new Dictionary<int, List<(double, double)>>();
var idx = 1;
results.Add(0, []);
foreach (var filter in filterList)
{
    results.Add(idx, []);
    idx++;
}
results.Add(idx, []);

var maxWindowSize = filterList.Max(x => x.WindowSize);
for (var i = maxWindowSize; i < dataRaw.Count; i++)
{
    var selected = dataRaw.Take(i + 1)
        .TakeLast(maxWindowSize)
        .ToList();
    
    results[0].Add((selected.Last().Id, selected.Last().Raw));

    idx = 1;
    foreach (var filter in filterList)
    {
        var caseSpecificArray = selected.TakeLast(filter.WindowSize)
            .Select(x => x.Raw)
            .ToArray();
        var transformedValue = filter.Transform(caseSpecificArray);
        
        results[idx].Add((selected.Last().Id, transformedValue));
        idx++;
    }
    results[idx].Add((selected.Last().Id, smoothedValues[i]));
}

var traces = new List<Scattergl>();
foreach (var key in results)
{
    traces.Add(new()
    {
        x = key.Value.Select(x=>x.Item1).ToArray(),
        y = key.Value.Select(x=>x.Item2).ToArray(),
        name = $"Line {labelDictionary[key.Key]}",
        mode = "lines+markers",
        marker = new Marker
        {
            symbol = "x-thin"
        }
    });
}

var chart = Chart.Plot(traces);
chart.WithWidth(1600);
chart.WithHeight(800);
chart.WithTitle("ECG");

chart.Show();