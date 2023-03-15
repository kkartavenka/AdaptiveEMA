namespace RSquaredAdaptiveEma.Demo;

internal class DataModel
{
    internal DataModel(int id, double raw) => (Id, Raw) = (id, raw);

    internal int Id { get; }
    internal double Raw { get; }
    internal double Transformed { get; set; }
}
