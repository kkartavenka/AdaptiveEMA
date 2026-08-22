using System.Globalization;
using System.Text;

namespace FilterComparison.Charting;

internal sealed record SeriesStyle(string Color, double Width, string? Dash = null);

/// <summary>
/// Minimal SVG writer. Deliberately dependency-free, and deliberately restricted to what
/// GitHub's markdown sanitizer preserves: presentation attributes on rect/line/polyline/
/// circle/text/g only. No &lt;style&gt;, no &lt;script&gt;, no external fonts, no xlink.
///
/// The background is painted an explicit opaque white rather than left transparent or driven
/// by prefers-color-scheme: the media query follows the OS rather than the GitHub theme, and
/// image proxying can defeat it, so a transparent chart turns unreadable on dark theme.
/// </summary>
internal sealed class SvgCanvas
{
    private const string FontFamily = "ui-sans-serif, -apple-system, Segoe UI, Helvetica, Arial, sans-serif";
    private const string Ink = "#1f2328";
    private const string Muted = "#656d76";
    private const string Grid = "#e4e8ed";

    private readonly StringBuilder _sb = new();
    private readonly double _width;
    private readonly double _height;
    private readonly double _left, _right, _top, _bottom;

    private double _xMin, _xMax, _yMin, _yMax;
    private bool _logY;

    internal SvgCanvas(double width = 960, double height = 420, double left = 68, double right = 18, double top = 54, double bottom = 74)
    {
        _width = width;
        _height = height;
        _left = left;
        _right = right;
        _top = top;
        _bottom = bottom;
    }

    private double PlotLeft => _left;
    private double PlotRight => _width - _right;
    private double PlotTop => _top;
    private double PlotBottom => _height - _bottom;

    private static string N(double v) => v.ToString("0.#", CultureInfo.InvariantCulture);

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    internal double MapX(double x) => PlotLeft + (x - _xMin) / (_xMax - _xMin) * (PlotRight - PlotLeft);

    internal double MapY(double y)
    {
        if (!_logY)
        {
            return PlotBottom - (y - _yMin) / (_yMax - _yMin) * (PlotBottom - PlotTop);
        }

        var lo = Math.Log10(_yMin);
        var hi = Math.Log10(_yMax);
        var v = Math.Log10(Math.Max(y, _yMin));
        return PlotBottom - (v - lo) / (hi - lo) * (PlotBottom - PlotTop);
    }

    internal void Begin(string title, string subtitle, double xMin, double xMax, double yMin, double yMax, bool logY = false)
    {
        _logY = logY;
        (_xMin, _xMax, _yMin, _yMax) = (xMin, xMax, yMin, yMax);

        if (_xMax - _xMin == 0) _xMax = _xMin + 1;
        if (_yMax - _yMin == 0) _yMax = _yMin + 1;

        _sb.Append(CultureInfo.InvariantCulture,
            $"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {N(_width)} {N(_height)}\" width=\"{N(_width)}\" height=\"{N(_height)}\" role=\"img\">");
        _sb.Append($"<title>{Esc(title)}</title>");
        _sb.Append($"<rect x=\"0\" y=\"0\" width=\"{N(_width)}\" height=\"{N(_height)}\" fill=\"#ffffff\"/>");
        _sb.Append($"<text x=\"{N(PlotLeft)}\" y=\"21\" font-family=\"{FontFamily}\" font-size=\"15\" font-weight=\"600\" fill=\"{Ink}\">{Esc(title)}</text>");

        // Subtitle sits on its own left-aligned line: a right-anchored subtitle risks clipping
        // in renderers that treat text-anchor loosely.
        if (subtitle.Length > 0)
        {
            _sb.Append($"<text x=\"{N(PlotLeft)}\" y=\"38\" font-family=\"{FontFamily}\" font-size=\"11.5\" fill=\"{Muted}\">{Esc(subtitle)}</text>");
        }
    }

    internal void Axes(string xLabel, string yLabel, int xTickCount = 8, int yTickCount = 6)
    {
        foreach (var t in _logY ? LogTicks(_yMin, _yMax) : NiceTicks(_yMin, _yMax, yTickCount))
        {
            var y = MapY(t);
            if (y < PlotTop - 0.5 || y > PlotBottom + 0.5) continue;

            _sb.Append($"<line x1=\"{N(PlotLeft)}\" y1=\"{N(y)}\" x2=\"{N(PlotRight)}\" y2=\"{N(y)}\" stroke=\"{Grid}\" stroke-width=\"1\"/>");
            _sb.Append($"<text x=\"{N(PlotLeft - 8)}\" y=\"{N(y + 3.5)}\" text-anchor=\"end\" font-family=\"{FontFamily}\" font-size=\"11\" fill=\"{Muted}\">{Esc(TickLabel(t))}</text>");
        }

        // xTickCount <= 0 suppresses the numeric x axis entirely, for charts whose x is
        // categorical and draws its own labels.
        foreach (var t in xTickCount <= 0 ? Array.Empty<double>() : NiceTicks(_xMin, _xMax, xTickCount))
        {
            var x = MapX(t);
            if (x < PlotLeft - 0.5 || x > PlotRight + 0.5) continue;

            _sb.Append($"<line x1=\"{N(x)}\" y1=\"{N(PlotTop)}\" x2=\"{N(x)}\" y2=\"{N(PlotBottom)}\" stroke=\"{Grid}\" stroke-width=\"1\"/>");
            _sb.Append($"<text x=\"{N(x)}\" y=\"{N(PlotBottom + 16)}\" text-anchor=\"middle\" font-family=\"{FontFamily}\" font-size=\"11\" fill=\"{Muted}\">{Esc(TickLabel(t))}</text>");
        }

        _sb.Append($"<line x1=\"{N(PlotLeft)}\" y1=\"{N(PlotBottom)}\" x2=\"{N(PlotRight)}\" y2=\"{N(PlotBottom)}\" stroke=\"{Muted}\" stroke-width=\"1\"/>");
        _sb.Append($"<line x1=\"{N(PlotLeft)}\" y1=\"{N(PlotTop)}\" x2=\"{N(PlotLeft)}\" y2=\"{N(PlotBottom)}\" stroke=\"{Muted}\" stroke-width=\"1\"/>");

        _sb.Append($"<text x=\"{N((PlotLeft + PlotRight) / 2)}\" y=\"{N(PlotBottom + 33)}\" text-anchor=\"middle\" font-family=\"{FontFamily}\" font-size=\"11.5\" fill=\"{Ink}\">{Esc(xLabel)}</text>");
        _sb.Append($"<text x=\"16\" y=\"{N((PlotTop + PlotBottom) / 2)}\" text-anchor=\"middle\" font-family=\"{FontFamily}\" font-size=\"11.5\" fill=\"{Ink}\" transform=\"rotate(-90 16 {N((PlotTop + PlotBottom) / 2)})\">{Esc(yLabel)}</text>");
    }

    internal void Polyline(IEnumerable<(double X, double Y)> points, SeriesStyle style)
    {
        var sb = new StringBuilder();
        var any = false;
        foreach (var (x, y) in points)
        {
            if (double.IsNaN(y)) continue;
            if (any) sb.Append(' ');
            sb.Append(N(MapX(x))).Append(',').Append(N(MapY(y)));
            any = true;
        }

        if (!any) return;

        _sb.Append($"<polyline fill=\"none\" stroke=\"{style.Color}\" stroke-width=\"{N(style.Width)}\" stroke-linejoin=\"round\" stroke-linecap=\"round\"");
        if (style.Dash is not null) _sb.Append($" stroke-dasharray=\"{style.Dash}\"");
        _sb.Append($" points=\"{sb}\"/>");
    }

    internal void Marker(double x, double y, string color, double radius = 5)
    {
        _sb.Append($"<circle cx=\"{N(MapX(x))}\" cy=\"{N(MapY(y))}\" r=\"{N(radius)}\" fill=\"{color}\" stroke=\"#ffffff\" stroke-width=\"1.5\"/>");
    }

    internal void Annotate(double x, double y, string text, string color, double dx = 9, double dy = -8, string anchor = "start")
    {
        _sb.Append($"<text x=\"{N(MapX(x) + dx)}\" y=\"{N(MapY(y) + dy)}\" text-anchor=\"{anchor}\" font-family=\"{FontFamily}\" font-size=\"11.5\" font-weight=\"600\" fill=\"{color}\">{Esc(text)}</text>");
    }

    /// <summary>Legend along the bottom. Entries wrap by measured-ish width to stay inside the viewBox.</summary>
    internal void Legend(IReadOnlyList<(string Label, SeriesStyle Style)> entries)
    {
        var x = PlotLeft;
        var y = _height - 16;

        foreach (var (label, style) in entries)
        {
            _sb.Append($"<line x1=\"{N(x)}\" y1=\"{N(y - 4)}\" x2=\"{N(x + 22)}\" y2=\"{N(y - 4)}\" stroke=\"{style.Color}\" stroke-width=\"{N(Math.Max(2, style.Width))}\" stroke-linecap=\"round\"");
            if (style.Dash is not null) _sb.Append($" stroke-dasharray=\"{style.Dash}\"");
            _sb.Append("/>");
            _sb.Append($"<text x=\"{N(x + 28)}\" y=\"{N(y)}\" font-family=\"{FontFamily}\" font-size=\"11.5\" fill=\"{Ink}\">{Esc(label)}</text>");

            x += 34 + label.Length * 6.2;
        }
    }

    /// <summary>Pixel-space rectangle, for chart types that lay out their own geometry.</summary>
    internal void RectPx(double x, double y, double width, double height, string fill)
    {
        _sb.Append($"<rect x=\"{N(x)}\" y=\"{N(y)}\" width=\"{N(width)}\" height=\"{N(height)}\" fill=\"{fill}\"/>");
    }

    /// <summary>Pixel-space text, for chart types that lay out their own geometry.</summary>
    internal void TextPx(double x, double y, string text, string color, double size = 11, string anchor = "start", bool bold = false)
    {
        _sb.Append($"<text x=\"{N(x)}\" y=\"{N(y)}\" text-anchor=\"{anchor}\" font-family=\"{FontFamily}\" font-size=\"{N(size)}\"" +
                   (bold ? " font-weight=\"600\"" : "") + $" fill=\"{color}\">{Esc(text)}</text>");
    }

    internal void LinePx(double x1, double y1, double x2, double y2, string color, double width = 1, string? dash = null)
    {
        _sb.Append($"<line x1=\"{N(x1)}\" y1=\"{N(y1)}\" x2=\"{N(x2)}\" y2=\"{N(y2)}\" stroke=\"{color}\" stroke-width=\"{N(width)}\"");
        if (dash is not null) _sb.Append($" stroke-dasharray=\"{dash}\"");
        _sb.Append("/>");
    }

    internal double PlotLeftPx => PlotLeft;
    internal double PlotRightPx => PlotRight;
    internal double PlotTopPx => PlotTop;
    internal double PlotBottomPx => PlotBottom;
    internal double HeightPx => _height;

    internal string End()
    {
        _sb.Append("</svg>");
        return _sb.ToString();
    }

    private static string TickLabel(double v)
    {
        var a = Math.Abs(v);
        if (a != 0 && (a < 0.001 || a >= 100000)) return v.ToString("0.##e+0", CultureInfo.InvariantCulture);
        if (a >= 100) return v.ToString("0", CultureInfo.InvariantCulture);
        if (a >= 10) return v.ToString("0.#", CultureInfo.InvariantCulture);
        return v.ToString("0.###", CultureInfo.InvariantCulture);
    }

    /// <summary>Round tick positions at 1/2/5 x 10^n, so labels stay readable and stable across runs.</summary>
    /// <summary>Ticks at 1/2/5 per decade, so a log axis stays readable across two or three decades.</summary>
    private static double[] LogTicks(double min, double max)
    {
        var ticks = new List<double>();
        var startExp = (int)Math.Floor(Math.Log10(min));
        var endExp = (int)Math.Ceiling(Math.Log10(max));

        for (var e = startExp; e <= endExp; e++)
        {
            foreach (var m in new[] { 1d, 2d, 5d })
            {
                var t = m * Math.Pow(10, e);
                if (t >= min && t <= max)
                {
                    ticks.Add(t);
                }
            }
        }

        return ticks.Count >= 2 ? ticks.ToArray() : new[] { min, max };
    }

    private static double[] NiceTicks(double min, double max, int target)
    {
        if (max <= min || target < 2) return new[] { min, max };

        var rawStep = (max - min) / target;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        var norm = rawStep / magnitude;
        var step = magnitude * (norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10);

        var first = Math.Ceiling(min / step) * step;
        var ticks = new List<double>();
        for (var t = first; t <= max + step * 1e-6; t += step)
        {
            ticks.Add(Math.Abs(t) < step * 1e-9 ? 0 : t);
        }

        return ticks.ToArray();
    }
}
