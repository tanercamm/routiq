namespace Routiq.Api.Services;

/// <summary>
/// MCP Atom #3: Time Overlap / Travel Friction
/// Stateless service. Input (multi-member flight times) → Output (friction score, max difference).
/// Lower friction = fairer for all group members.
/// </summary>
public class TimeOverlapService
{
    public class TimeOverlapResult
    {
        public int MaxDifferenceMinutes { get; set; }
        public int MinFlightMinutes { get; set; }
        public int MaxFlightMinutes { get; set; }
        public double AvgFlightMinutes { get; set; }
        public string AvgFlightFormatted { get; set; } = string.Empty;
        /// <summary>Standard deviation of flight times. Lower = fairer.</summary>
        public double FrictionScore { get; set; }
        /// <summary>Normalized 0-100 score. Higher = better (lower friction).</summary>
        public double NormalizedScore { get; set; }
    }

    public TimeOverlapResult Analyse(List<int> memberFlightTimesMinutes)
    {
        if (memberFlightTimesMinutes.Count == 0)
            return new TimeOverlapResult();

        var min = memberFlightTimesMinutes.Min();
        var max = memberFlightTimesMinutes.Max();
        var avg = memberFlightTimesMinutes.Average();

        // Standard deviation
        var variance = memberFlightTimesMinutes
            .Select(t => Math.Pow(t - avg, 2))
            .Average();
        var stdDev = Math.Sqrt(variance);

        // Normalize: A stdDev of 0 = perfect 100, stdDev of 600+ (10 hours) = 0
        var normalizedScore = Math.Max(0, Math.Min(100, 100 - (stdDev / 6.0)));

        var avgMinutes = (int)Math.Round(avg);

        return new TimeOverlapResult
        {
            MaxDifferenceMinutes = max - min,
            MinFlightMinutes = min,
            MaxFlightMinutes = max,
            AvgFlightMinutes = Math.Round(avg, 1),
            AvgFlightFormatted = $"{avgMinutes / 60}h {avgMinutes % 60:D2}m",
            FrictionScore = Math.Round(stdDev, 1),
            NormalizedScore = Math.Round(normalizedScore, 1)
        };
    }
}
