namespace RSquaredAdaptiveEma.Demo;

internal abstract class Result
{
    internal List<DataModel> _rawData;
    internal Result(List<DataModel> rawData) => _rawData = (List<DataModel>)rawData.Clone();

    internal List<(string title, List<DataModel> data)> Results { get; set; } = new();
}
