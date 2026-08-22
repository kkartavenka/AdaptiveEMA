namespace FilterComparison.Charting;

internal static class Charts
{
    internal const int SliceLength = 400;

    /// <summary>
    /// Picks the most active window in the series, so the overlay shows the filters resolving a
    /// real transient rather than a flat stretch. Deterministic: fixed stride, first max wins.
    /// </summary>
    internal static (int Start, int End) PickSlice(double[] raw, int i0, int i1, int length = SliceLength)
    {
        var available = i1 - i0 + 1;
        if (available <= length) return (i0, i1);

        var stride = Math.Max(1, length / 8);
        var bestStart = i0;
        var bestScore = double.NegativeInfinity;

        for (var s = i0; s + length - 1 <= i1; s += stride)
        {
            double sum = 0, sumSq = 0;
            for (var i = s; i < s + length; i++)
            {
                sum += raw[i];
                sumSq += raw[i] * raw[i];
            }

            var mean = sum / length;
            var variance = sumSq / length - mean * mean;
            if (variance > bestScore)
            {
                bestScore = variance;
                bestStart = s;
            }
        }

        return (bestStart, bestStart + length - 1);
    }

    internal static string Overlay(
        string title,
        string subtitle,
        double[] raw,
        IReadOnlyList<(string Label, double[] Values, SeriesStyle Style)> series,
        int start,
        int end)
    {
        double yMin = double.MaxValue, yMax = double.MinValue;

        void Track(double[] values)
        {
            for (var i = start; i <= end; i++)
            {
                var v = values[i];
                if (double.IsNaN(v)) continue;
                if (v < yMin) yMin = v;
                if (v > yMax) yMax = v;
            }
        }

        Track(raw);
        foreach (var s in series) Track(s.Values);

        var pad = (yMax - yMin) * 0.08;
        var canvas = new SvgCanvas();
        canvas.Begin(title, subtitle, start, end, yMin - pad, yMax + pad);
        canvas.Axes("sample index", "value");

        canvas.Polyline(Points(raw, start, end), Palette.Raw);
        foreach (var s in series)
        {
            canvas.Polyline(Points(s.Values, start, end), s.Style);
        }

        var legend = new List<(string, SeriesStyle)> { ("Raw", Palette.Raw) };
        legend.AddRange(series.Select(s => (s.Label, s.Style)));
        canvas.Legend(legend);

        return canvas.End();
    }

    /// <summary>
    /// Grouped bars of the live prediction score, one group per dataset. The 1.0 rule is the
    /// whole point of the chart: it is the score of doing nothing, so a bar crossing it means the
    /// filter is worse than passing the raw signal through untouched.
    /// </summary>
    internal static string LiveScores(
        string title,
        string subtitle,
        IReadOnlyList<string> datasets,
        IReadOnlyList<(string Label, string Color)> series,
        IReadOnlyList<IReadOnlyList<double>> scores)
    {
        var canvas = new SvgCanvas(960, 430, 68, 18, 54, 84);

        var top = 0d;
        foreach (var row in scores)
        {
            foreach (var v in row)
            {
                if (!double.IsNaN(v) && v > top) top = v;
            }
        }

        top = Math.Max(1.05, top) * 1.06;

        canvas.Begin(title, subtitle, 0, 1, 0, top);
        canvas.Axes("", "live score  (1.0 = same as no filtering)", 0);

        var left = canvas.PlotLeftPx;
        var right = canvas.PlotRightPx;
        var groupWidth = (right - left) / datasets.Count;
        var barWidth = groupWidth * 0.78 / series.Count;

        for (var d = 0; d < datasets.Count; d++)
        {
            var groupLeft = left + d * groupWidth + groupWidth * 0.11;

            for (var f = 0; f < series.Count; f++)
            {
                var value = scores[d][f];
                if (double.IsNaN(value)) continue;

                var x = groupLeft + f * barWidth;
                var y = canvas.MapY(value);
                canvas.RectPx(x, y, barWidth * 0.86, canvas.PlotBottomPx - y, series[f].Color);
            }

            canvas.TextPx(groupLeft + groupWidth * 0.39, canvas.PlotBottomPx + 16, datasets[d], "#656d76", 10.5, "middle");
        }

        // The do-nothing line, drawn over the bars so it stays readable.
        var oneY = canvas.MapY(1);
        canvas.LinePx(left, oneY, right, oneY, "#1f2328", 1.4, "6 3");
        canvas.TextPx(right - 2, oneY - 6, "no better than raw", "#1f2328", 10.5, "end", true);

        canvas.Legend(series.Select(x => (x.Label, new SeriesStyle(x.Color, 2.6))).ToList());
        return canvas.End();
    }

    internal static string AlphaTrace(string title, string subtitle, double[] alphas, int start, int sliceStart, int sliceEnd)
    {
        var canvas = new SvgCanvas();
        canvas.Begin(title, subtitle, sliceStart, sliceEnd, 0, 1);
        canvas.Axes("sample index", "realized alpha");

        canvas.Polyline(
            Enumerable.Range(sliceStart, sliceEnd - sliceStart + 1).Select(i => ((double)i, alphas[i - start])),
            Palette.Adaptive);

        canvas.Legend(new List<(string, SeriesStyle)> { ("realized alpha (= R² of the local fit, rescaled)", Palette.Adaptive) });
        return canvas.End();
    }

    private static IEnumerable<(double, double)> Points(double[] values, int start, int end)
    {
        for (var i = start; i <= end; i++)
        {
            yield return (i, values[i]);
        }
    }
}
