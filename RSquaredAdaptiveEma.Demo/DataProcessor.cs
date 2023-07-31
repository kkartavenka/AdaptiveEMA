using XPlot.Plotly;

namespace RSquaredAdaptiveEma.Demo;

internal class DataProcessor
{
    internal DataProcessor(string filename, int fieldId)
    {
        var dataRaw = new DataReader(filename, fieldId).Data;

        var results = new ComparisonScenarioEma(dataRaw).Results;
        results.AddRange(new ComparisonScenario(dataRaw).Results);

        var titles = "raw," + string.Join(",", results.Select(m => m.title).ToArray());
        var preExport = new List<string>();
        for (var i = 0; i < results[0].data.Count; i++)
        {
            preExport.Add(string.Join(",", new double[] {
                results[0].data[i].Raw,
                results[0].data[i].Transformed,
                results[1].data[i].Transformed,
                results[2].data[i].Transformed}));
        }
        preExport.Insert(0, titles);
        File.WriteAllLines("amd.csv", preExport);

        var traces = new List<Scattergl>
        {
            new Scattergl
            {
                x = dataRaw[0].DateTime == null ? dataRaw.Select(m => m.Id).ToArray() : dataRaw.Select(m => m.DateTime).ToArray(),
                y = dataRaw.Select(m => m.Raw).ToArray(),
                name = "Raw",
                mode = "lines+markers",
                marker = new Marker
                {
                    symbol = "x-thin"
                }
            }
        };

        results.ForEach(row => traces.Add(new Scattergl
        {
            x = row.data[0].DateTime == null ? row.data.Select(m => m.Id).ToArray() : row.data.Select(m => m.DateTime).ToArray(),
            y = row.data.Select(m => m.Transformed).ToArray(),
            name = row.title,
            mode = "lines+markers",
            marker = new Marker
            {
                symbol = "x-thin"
            }
        }));

        var chart = Chart.Plot(traces);
        chart.WithWidth(1920);
        chart.WithHeight(900);

        chart.Show();
    }
}