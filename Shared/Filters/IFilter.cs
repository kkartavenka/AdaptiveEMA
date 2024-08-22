namespace Shared.Filters;

public interface IFilter
{
    public double Transform(double[] values);
    public int WindowSize { get; }
}