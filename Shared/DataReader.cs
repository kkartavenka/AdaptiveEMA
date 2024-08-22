using System.Globalization;
using CsvHelper;

namespace Shared;

public class DataReader
{
    private const int DateTimeColumnId = 0;

    private readonly string[] _dateTimeFormats = { @"MMM d, yyyy", @"d-MMM-yy", @"M/d/yyyy" };

    public DataReader(string file, int fieldId)
    {
        using var sr = new StreamReader(file);
        using var csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);

        csvReader.Read();

        var id = 0;
        var dataRaw = new List<DataModel>();

        while (csvReader.Read())
        {
            var dateTimeUnparced = csvReader.GetField<string>(DateTimeColumnId)
                .Replace("\"", "");
            DateTime? trueDateTime = null;

            foreach (var dtFormat in _dateTimeFormats)
            {
                if (DateTime.TryParseExact(dateTimeUnparced, dtFormat, null, DateTimeStyles.None, out var v1))
                {
                    trueDateTime = v1;
                    break;
                }
            }

            dataRaw.Add(new DataModel(id++, csvReader.GetField<double>(fieldId), trueDateTime));
        }

        Data = dataRaw.OrderByDescending(m => m.Id)
            .ToList();
    }

    public List<DataModel> Data { get; }
}