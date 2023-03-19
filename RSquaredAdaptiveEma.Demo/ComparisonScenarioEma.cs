using Accord.Statistics;

namespace RSquaredAdaptiveEma.Demo;

internal class ComparisonScenarioEma: Result
{
    internal ComparisonScenarioEma(List<DataModel> rawData) : base(rawData)
    {
        ComparisonConditionEMA();
    }

    internal void ComparisonConditionEMA()
    {
        var (windowSize, smoothingFactor) = (20, 0.2);

        var title = $"Simple EMA [{windowSize}, {smoothingFactor}]";
        var copiedData = (List<DataModel>)_rawData.Clone();

        for (var i = 0; i != _rawData.Count; i++)
        {
            var selected = copiedData.Take(i + 1).Select(m => m.Raw).ToArray();

            if (i < windowSize)
            {
                copiedData[i].Transformed = copiedData[i].Raw;
                continue;
            }

            copiedData[i].Transformed = selected.ExponentialWeightedMean(windowSize, smoothingFactor);
        }

        Results.Add((title, copiedData));
    }
}
