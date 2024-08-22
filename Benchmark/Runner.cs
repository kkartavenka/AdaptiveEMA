using AdaptiveEMA;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Shared;
using Shared.Filters;

namespace Benchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class Runner
{
    private List<DataModel> _data;

    [GlobalSetup]
    public void Setup()
    {
        _data = new DataReader("Data/ECG.csv", 0).Data.OrderBy(x => x.Id)
            .ToList();
    }
    
    [Params(0, 10, 500)]
    public int WindowSize { get; set; }

    [Benchmark]
    public double DoubleVersion()
    {
        var filter = new RSquaredAdaptiveEma(new RunParameters(WindowSize));
        return filter.Transform(GetTestData());
    }

    private double[] GetTestData() => _data.Take(WindowSize)
        .Select(x => x.Raw)
        .ToArray();
}