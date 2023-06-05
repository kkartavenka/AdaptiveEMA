using CsvHelper;
using System.Globalization;

namespace RSquaredAdaptiveEma.Demo;

internal class DataReader
{
    internal DataReader(string file, int fieldId)
    {
        using var sr = new StreamReader(file);
        using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

        csvReader.Read();

        int id = 0;
        var dataRaw = new List<DataModel>();

        while (csvReader.Read())
            dataRaw.Add(new(id: id++, csvReader.GetField<double>(fieldId)));

        
        
        Data = dataRaw.OrderByDescending(m => m.Id).ToList();
    }

    internal List<DataModel> Data { get; }
}
