# AdaptiveEMA

The aim was to reduce the lag of exponential moving average via assesing r squared of polynomial fit.

## Usage:

1) Construct the parameters:
   
```csharp
// At minimum, window size is required, in this case the EMA decay factor will be in the range [0, 1], the polynomial order is 2
// Additionally, the EMA decay factor range can be specified and the polynomial order
var filterRunParameter = new RunParameters(10);
```

2) Construct the filter, and use it

```csharp
var filter = new RSquaredAdaptiveEma(filterRunParameter);

var dataPoints = new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
var lastPointTransformed = filter.Transform(dataPoints);
```

## Getting the parameters:

In the AdaptiveEMA.Optimizer namespace there are `OptimizerHelper` and `OptimizerBuilder` which help to find the best decay factor range for a given data. By default, the Savitzky-Golay filter is used to smooth data points and the Nelder-Mead algorithm is used to maximize the coefficient of determination. However, the comparison data points and the evaluation function can be overridden by the builder.

```csharp
// Assuming we have some dataRaw as a double[], we take 25% of it for the train purpose
var trainSamples = dataRaw.Take((int)(dataRaw.Count * 0.25)).ToArray();

var optimizerParams = new OptimizerBuilder()
    .UseDefaultComparison(trainSamples, 10, 2) // Savitzky-Golay filter is used here with side point of 10 and polynomial order of 2
    .UseDefaultScoreEvaluation() // Indicate that R-Squared will be used
    .UseDefaultSimplexParameters() // Indicate that up to 1000 iterations will be used, convergence tolerance of 1e-6
    .WithAlgoParameters(20, 2) // Indicating that we are interested to use the AdaptiveEMA algorithm with window size of 20 and polynomial order of 2
    .Build();

// Getting optimized parameters
var optimizedParams = new OptimizerHelper(optimizerParams).FindParameters();
```

## Comparison to simple EMA

To be updated...

## License

AdaptiveEMA is licensed under the [MIT license](https://github.com/kkartavenka/FastDtw.CSharp/blob/master/LICENSE.txt).

