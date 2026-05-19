namespace SchoolGrowth.Blazor;

public static class DensityDisplay
{
    public static int SortKey(string density)
    {
        return density.Trim().ToUpperInvariant() switch
        {
            "RL" or "LOW" => 10,
            "RM" or "MEDIUM" => 20,
            "RMH" or "MEDIUM-HIGH" => 30,
            "RH" or "HIGH" => 40,
            "LOW/MEDIUM" => 50,
            "MEDIUM-HIGH/HIGH" => 60,
            "EXISTING" or "OTHER" or "OTHER/EXISTING" => 90,
            _ => 80
        };
    }

    public static string Label(string density)
    {
        return density switch
        {
            "RL" => "Low",
            "RM" => "Medium",
            "RMH" => "Medium-high",
            "RH" => "High",
            _ => density
        };
    }
}
