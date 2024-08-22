namespace Shared;

public class DataModel
{
    public DataModel(int id, double raw, DateTime? dateTime)
    {
        (Id, Raw, DateTime) = (id, raw, dateTime);
    }

    public int Id { get; }
    public double Raw { get; }
    public double Transformed { get; set; }
    public DateTime? DateTime { get; }
}