namespace RSquaredAdaptiveEma.Demo;

internal class ComparisonScenario : Result
{

    internal ComparisonScenario(List<DataModel> rawData) : base(rawData)
    {
        CaseNoConfirmation(20, 0, 0.5);
        CaseWithConfirmation(100, 0, 0.5, 20, 2);
    }

    internal void CaseNoConfirmation (int windowSize, double smoothingFactorMin, double smoothingFactorMax)
    {
        var title = $"aEMA [{windowSize}, {smoothingFactorMin}, {smoothingFactorMax}]";
        var copiedData = (List<DataModel>)_rawData.Clone();
        var filter = new AdaptiveEMA.RSquaredAdaptiveEma(smoothingFactorMin, smoothingFactorMax, windowSize);

        for (int i = 0; i < _rawData.Count; i++)
        {
            var selected = _rawData.Take(i + 1).Select(m => m.Raw).ToArray();
            copiedData[i].Transformed = filter.GetLastValue(selected);
        }

        Results.Add((title, copiedData));
    }

    internal void CaseWithConfirmation(int windowSize, double smoothingFactorMin, double smoothingFactorMax, int confirmationWindowSize, double confirmationSignificance)
    {
        var title = $"aEMA [{windowSize}, {smoothingFactorMin}, {smoothingFactorMax}, ({confirmationWindowSize}, {confirmationSignificance})]";

        var copiedData = (List<DataModel>)_rawData.Clone();
        var filter = new AdaptiveEMA.RSquaredAdaptiveEma(smoothingFactorMin, smoothingFactorMax, windowSize);
        filter.UseConfirmationWindowSize(confirmationWindowSize, confirmationSignificance);

        for (int i = 0; i < _rawData.Count; i++)
        {
            var selected = _rawData.Take(i + 1).Select(m => m.Raw).ToArray();
            copiedData[i].Transformed = filter.GetLastValue(selected);
        }

        Results.Add((title, copiedData));
    }
}
