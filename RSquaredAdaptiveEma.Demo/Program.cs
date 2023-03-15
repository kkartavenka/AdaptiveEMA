using CsvHelper;
using RSquaredAdaptiveEma.Demo;
using System.Globalization;
using XPlot.Plotly;
using AdaptiveEMA;

var file = @"./../../../../Data/AMD Historical Data.csv";

using var sr = new StreamReader(file);
using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

csvReader.Read();

int id = 0; 
var data = new List<DataModel>();

while (csvReader.Read())
    data.Add(new(id: id++, csvReader.GetField<double>(1)));

var filter = new AdaptiveEMA.RSquaredAdaptiveEma(0, 0.5, 20);
for(int i =0;i<data.Count;i++)
{
    var selected = data.Take(i + 1).Select(m => m.Raw).ToArray();
    data[i].Transformed = filter.GetLastValue(selected);
}

var traces = new List<Scattergl>();
traces.Add(new Scattergl()
{
    x = data.Select(m => (double)m.Id).ToArray(),
    y = data.Select(m => m.Raw).ToArray(),
    name = "Raw",
    mode = "lines+markers"
});

traces.Add(new Scattergl()
{
    x = data.Select(m => (double)m.Id).ToArray(),
    y = data.Select(m => m.Transformed).ToArray(),
    name = "Transformed",
    mode = "lines+markers"
});

Chart.Plot(traces).Show();