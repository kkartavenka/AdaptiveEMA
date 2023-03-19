using CsvHelper;
using System.Globalization;

namespace RSquaredAdaptiveEma.Demo;

internal class DataReader
{
    private const int FIELD_ID = 1;

    internal DataReader(string file)
    {
        using var sr = new StreamReader(file);
        using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

        csvReader.Read();

        int id = 0;
        var dataRaw = new List<DataModel>();

        while (csvReader.Read())
            dataRaw.Add(new(id: id++, csvReader.GetField<double>(FIELD_ID)));

        Data = dataRaw;
    }

    internal List<DataModel> Data { get; }
}
