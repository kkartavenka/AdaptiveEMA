using Accord.IO;
using CsvHelper;
using RSquaredAdaptiveEma.Demo;
using System.Globalization;
using XPlot.Plotly;

var file = @"./../../../../Data/AMD Historical Data.csv";

using var sr = new StreamReader(file);
using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

csvReader.Read();

int id = 0; 
var dataRaw = new List<DataModel>();

while (csvReader.Read())
    dataRaw.Add(new(id: id++, csvReader.GetField<double>(1)));

var filter20 = new AdaptiveEMA.RSquaredAdaptiveEma(0, 0.5, 20);
var filter202 = new AdaptiveEMA.RSquaredAdaptiveEma(0, 0.25, 20);
var filter50 = new AdaptiveEMA.RSquaredAdaptiveEma(0, 0.5, 50);

var data20 = dataRaw.Clone();
var data50 = dataRaw.Clone();
var data202 = dataRaw.Clone();
for (int i = 0; i < dataRaw.Count; i++)
{
    var selected = dataRaw.Take(i + 1).Select(m => m.Raw).ToArray();
    data20[i].Transformed = filter20.GetLastValue(selected);
    data50[i].Transformed = filter50.GetLastValue(selected);
    data202[i].Transformed = filter202.GetLastValue(selected);
}

var traces = new List<Scattergl>
{
    new Scattergl()
    {
        x = dataRaw.Select(m => (double)m.Id).ToArray(),
        y = dataRaw.Select(m => m.Raw).ToArray(),
        name = "Raw",
        mode = "lines+markers"
    },
    new Scattergl()
    {
        x = data20.Select(m => (double)m.Id).ToArray(),
        y = data20.Select(m => m.Transformed).ToArray(),
        name = "Transformed [0, 0.5, 20]",
        mode = "lines+markers"
    },
    new Scattergl()
    {
        x = data202.Select(m => (double)m.Id).ToArray(),
        y = data202.Select(m => m.Transformed).ToArray(),
        name = "Transformed [0, 0.25, 20]",
        mode = "lines+markers"
    },
    new Scattergl()
    {
        x = data50.Select(m => (double)m.Id).ToArray(),
        y = data50.Select(m => m.Transformed).ToArray(),
        name = "Transformed [0, 0.5, 50]",
        mode = "lines+markers"
    }
};

Chart.Plot(traces).Show();