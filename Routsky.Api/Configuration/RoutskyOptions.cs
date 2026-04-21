namespace Routsky.Api.Configuration;

public class BudgetDefaults
{
    public const string SectionName = "Routsky:BudgetDefaults";
    public int DefaultBudgetUsd { get; set; } = 1500;
}

public class CurrencyRates
{
    public const string SectionName = "Routsky:CurrencyRates";
    public double UsdToEur { get; set; } = 0.95;
    public double UsdToTry { get; set; } = 36.5;
    public double UsdToAud { get; set; } = 1.5;

    public int Convert(int usdAmount, string targetCurrency) => targetCurrency switch
    {
        "EUR" => (int)(usdAmount * UsdToEur),
        "TRY" => (int)(usdAmount * UsdToTry),
        "AUD" => (int)(usdAmount * UsdToAud),
        _ => usdAmount
    };
}

public class FlightDefaults
{
    public const string SectionName = "Routsky:FlightDefaults";
    public int FallbackFlightMinutes { get; set; } = 180;
    public int FallbackCostUsd { get; set; } = 500;
    public int BatchFallbackCostUsd { get; set; } = 250;
    public int SingleFallbackCostUsd { get; set; } = 300;
    public int GeminiBatchChunkSize { get; set; } = 15;
    public int GeminiBatchDelayMs { get; set; } = 2000;
}

public class GeminiSettings
{
    public const string SectionName = "Routsky:GeminiSettings";
    public double Temperature { get; set; } = 0.1;
    public double TopP { get; set; } = 0.9;
}

public class CarbonEstimation
{
    public const string SectionName = "Routsky:CarbonEstimation";
    public double KgCo2PerRoute { get; set; } = 230.0;
}

public class AvatarSettings
{
    public const string SectionName = "Routsky:AvatarSettings";
    public long MaxFileSizeBytes { get; set; } = 10_485_760;
    public int ResizePx { get; set; } = 512;
    public int WebpQuality { get; set; } = 80;
}

public class PrestigeMapping
{
    public const string SectionName = "Routsky:PrestigeMapping";
    public double Multiplier { get; set; } = 50;
    public int MinScore { get; set; } = 10;
    public int MaxScore { get; set; } = 100;
}

public class DiscoverDefaults
{
    public const string SectionName = "Routsky:DiscoverDefaults";
    public double DailyCostBaseUsd { get; set; } = 100;
    public int MaxBudgetCap { get; set; } = 10000;
    public double DefaultBudgetPercentUsed { get; set; } = 50;
}

public class InviteCodeSettings
{
    public const string SectionName = "Routsky:InviteCode";
    public string Prefix { get; set; } = "RTQ-";
    public int CodeLength { get; set; } = 4;
}
