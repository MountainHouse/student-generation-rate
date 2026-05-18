namespace SchoolGrowth.Blazor.Components;

public static class ChartAxis
{
    public static double NiceMaximum(double value, int tickCount = 5)
    {
        if (value <= 0) return 1;
        var step = NiceStep(value / Math.Max(1, tickCount - 1));
        return step * Math.Ceiling(value / step);
    }

    public static IReadOnlyList<(double Y, string Label)> YTicks(
        double axisMaximum,
        double top,
        double bottom,
        int tickCount = 5)
    {
        var max = NiceMaximum(axisMaximum, tickCount);
        var step = max / Math.Max(1, tickCount - 1);

        return Enumerable.Range(0, tickCount)
            .Select(index =>
            {
                var value = max - step * index;
                var y = top + ((max - value) / max * (bottom - top));
                return (y, FormatTickLabel(value, max));
            })
            .ToList();
    }

    private static string FormatTickLabel(double value, double max)
    {
        if (Math.Abs(value) < 0.0000001) return "0";

        return max switch
        {
            <= 0.1 => value.ToString("0.###"),
            <= 1 => value.ToString("0.##"),
            <= 10 => value.ToString("0.#"),
            _ => value.ToString("N0")
        };
    }

    private static double NiceStep(double rawStep)
    {
        if (rawStep <= 0) return 1;

        var exponent = Math.Floor(Math.Log10(rawStep));
        var magnitude = Math.Pow(10, exponent);
        var scaled = rawStep / magnitude;
        var niceScaled = scaled switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 2.5 => 2.5,
            <= 5 => 5,
            _ => 10
        };

        return niceScaled * magnitude;
    }
}
