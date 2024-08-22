using AdaptiveEMA;
using Shared;
using Shared.Filters;
using XPlot.Plotly;

var dataRaw = new DataReader("Data/ECG.csv", 0).Data.OrderBy(x => x.Id)
    .ToList();

const int windowSize = 20;
IFilter simpleEma = new SimpleEma(0.2, windowSize);

var filterRunParameter = new RunParameters(20);
IFilter adaptiveEma = new RSquaredAdaptiveEma(filterRunParameter);
var filterList = new List<IFilter>(2)
{
    simpleEma, adaptiveEma
};
var labelDictionary = new Dictionary<int, string>()
{
    { 0, "Raw" },
    { 1, $"Simple EMA [smoothing 0.2]" },
    { 2, $"Adaptive EMA [{filterRunParameter.WindowSize}, {filterRunParameter.PolyOrder}]" }
};

var results = new Dictionary<int, List<(double, double)>>();
var idx = 1;
results.Add(0, new List<(double, double)>());
foreach (var filter in filterList)
{
    results.Add(idx, new List<(double, double)>());
    idx++;
}

for (var i = windowSize; i < dataRaw.Count; i++)
{
    var selected = dataRaw.Take(i + 1)
        .TakeLast(windowSize)
        .ToList();
    
    results[0].Add((selected.Last().Id, selected.Last().Raw));

    idx = 1;
    foreach (var filter in filterList)
    {
        var transformedValue = filter.Transform(selected.Select(x => x.Raw)
            .ToArray());
        
        results[idx].Add((selected.Last().Id, transformedValue));
        idx++;
    }
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