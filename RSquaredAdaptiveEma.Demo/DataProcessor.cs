using XPlot.Plotly;

namespace RSquaredAdaptiveEma.Demo;

internal class DataProcessor
{
    internal DataProcessor(string filename, int fieldId)
    {
        var dataRaw = new DataReader(filename, fieldId).Data;

        var results = new ComparisonScenarioEma(dataRaw).Results;
        results.AddRange(new ComparisonScenario(dataRaw).Results);

        var traces = new List<Scattergl>
        {
            new Scattergl
            {
                x = dataRaw.Select(m => (double)m.Id).ToArray(),
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
            x = row.data.Select(m => (double)m.Id).ToArray(),
            y = row.data.Select(m => m.Transformed).ToArray(),
            name = row.title,
            mode = "lines+markers",
            marker = new Marker
            {
                symbol = "x-thin"
            }
        }));

        Chart.Plot(traces).Show();
    }
}