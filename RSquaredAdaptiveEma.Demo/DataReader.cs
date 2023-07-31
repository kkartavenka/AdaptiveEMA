using CsvHelper;
using System.Globalization;

namespace RSquaredAdaptiveEma.Demo;

internal class DataReader
{
    private const int DateTimeColumnId = 0;

    private readonly string[] _dateTimeFormats = new string[] { @"MMM d, yyyy", @"d-MMM-yy", @"M/d/yyyy" };
    
    internal DataReader(string file, int fieldId)
    {
        using var sr = new StreamReader(file);
        using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

        csvReader.Read();

        int id = 0;
        var dataRaw = new List<DataModel>();

        while (csvReader.Read())
        {
            string dateTimeUnparced = csvReader.GetField<string>(DateTimeColumnId).Replace("\"", "");
            DateTime? trueDateTime = null;

            foreach (string dtFormat in _dateTimeFormats)
                if (DateTime.TryParseExact(dateTimeUnparced, dtFormat, null, DateTimeStyles.None, out DateTime v1))
                {
                    trueDateTime = v1;
                    break;
                }
            dataRaw.Add(new(id++, csvReader.GetField<double>(fieldId), trueDateTime));
        }
        
        Data = dataRaw.OrderByDescending(m => m.Id).ToList();
    }

    internal List<DataModel> Data { get; }
}
