using System.Globalization;

namespace FilterComparison.Data;

internal static class SeriesLoader
{
    /// <summary>
    /// Reads one numeric column. A plain Split(',') is sufficient here: none of the data
    /// files contain quoted fields, and every line of every file has a uniform field count.
    /// </summary>
    internal static double[] Load(DatasetSpec spec)
    {
        var path = RepoPaths.Resolve(spec.RelativePath);
        var lines = File.ReadAllLines(path);

        var values = new List<double>(lines.Length);
        var start = spec.HasHeader ? 1 : 0;

        for (var i = start; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var fields = line.Split(',');
            if (fields.Length <= spec.ValueColumn)
            {
                throw new InvalidDataException(
                    $"{spec.RelativePath} line {i + 1}: expected at least {spec.ValueColumn + 1} fields, found {fields.Length}");
            }

            var raw = fields[spec.ValueColumn].Trim().Trim('"');
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new InvalidDataException($"{spec.RelativePath} line {i + 1}: cannot parse '{raw}' as a number");
            }

            values.Add(value);
        }

        var result = values.ToArray();
        if (spec.ReverseFileOrder)
        {
            Array.Reverse(result);
        }

        return result;
    }
}
