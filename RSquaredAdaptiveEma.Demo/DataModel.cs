namespace RSquaredAdaptiveEma.Demo;

internal class DataModel: ICloneable
{
    internal DataModel(int id, double raw, DateTime? dateTime) => (Id, Raw, DateTime) = (id, raw, dateTime);

    internal int Id { get; }
    internal double Raw { get; }
    internal double Transformed { get; set; }
    internal DateTime? DateTime { get; }

    public object Clone() => MemberwiseClone();
}
