# AdaptiveEMA

THe aim was to reduce the lag of exponential moving average via assesing r squared of polynomial fit.

Usage:

```csharp
double[] someArray = new doublep[] {...}; 
var filter = new AdaptiveEMA.RSquaredAdaptiveEma(smoothingFactorMin: 0, smoothingFactorMax: 0.5, windowSize: 20);
var weightedValue = filter.GetLastValue(someArray);
```


```csharp
double[] someArray = new doublep[] {...}; 
var filter = new AdaptiveEMA.RSquaredAdaptiveEma(smoothingFactorMin: 0, smoothingFactorMax: 0.5, windowSize: 20);
filter.UseConfirmationWindowSize(confirmationWindowSize: 10, significanceRatio: 2);

var weightedValue = filter.GetLastValue(someArray);
```

## Results

![image](https://user-images.githubusercontent.com/45607880/226188061-abbad08b-fa35-44c1-9817-c86673df937f.png)

![image](https://user-images.githubusercontent.com/45607880/226188318-b3113160-726b-41c2-ad1c-f0d9d2a32904.png)

## License

FastDtw.CSharp is licensed under the [MIT license](https://github.com/kkartavenka/FastDtw.CSharp/blob/master/LICENSE.txt).
