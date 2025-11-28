namespace DiamondX.Weather;

/// <summary>
/// Current weather conditions that affect gameplay.
/// </summary>
public record WeatherConditions
{
    /// <summary>
    /// Temperature in Fahrenheit.
    /// </summary>
    public double Temperature { get; init; } = 72.0;

    /// <summary>
    /// Humidity percentage (0-100).
    /// </summary>
    public double Humidity { get; init; } = 50.0;

    /// <summary>
    /// Wind speed in mph.
    /// </summary>
    public double WindSpeed { get; init; } = 5.0;

    /// <summary>
    /// Wind direction in degrees (0 = from center field, 180 = from home plate).
    /// </summary>
    public double WindDirection { get; init; } = 0.0;

    /// <summary>
    /// Barometric pressure in inches of mercury.
    /// </summary>
    public double Pressure { get; init; } = 29.92;

    /// <summary>
    /// Current precipitation type.
    /// </summary>
    public PrecipitationType Precipitation { get; init; } = PrecipitationType.None;

    /// <summary>
    /// Sky condition affecting visibility.
    /// </summary>
    public SkyCondition Sky { get; init; } = SkyCondition.Clear;

    /// <summary>
    /// Whether the game should be delayed due to weather.
    /// </summary>
    public bool IsPlayable => Precipitation != PrecipitationType.Heavy &&
                              Precipitation != PrecipitationType.Thunderstorm &&
                              WindSpeed < 40;

    /// <summary>
    /// Calculate home run distance modifier based on weather.
    /// Positive = ball carries further, negative = shorter.
    /// </summary>
    public double GetHomeRunModifier()
    {
        double modifier = 0;

        // Temperature: warmer air is less dense, ball carries further
        // ~3.5 feet per 10°F above/below 70°F
        modifier += (Temperature - 70) * 0.35;

        // Humidity: humid air is actually less dense (water vapor lighter than N2/O2)
        // ~1 foot per 10% humidity above 50%
        modifier += (Humidity - 50) * 0.1;

        // Altitude/Pressure: lower pressure = less air resistance
        // ~4 feet per 0.1 inHg below normal (29.92)
        modifier += (29.92 - Pressure) * 40;

        // Wind: blowing out helps, blowing in hurts
        // Direction 0 = from CF (blowing in), 180 = from HP (blowing out)
        var windFactor = Math.Cos(WindDirection * Math.PI / 180); // -1 to 1
        modifier += WindSpeed * windFactor * -1.5; // Flip sign: 0° wind blowing IN

        return modifier;
    }

    /// <summary>
    /// Get a descriptive string for the current conditions.
    /// </summary>
    public string GetDescription()
    {
        var windDir = WindDirection switch
        {
            >= 315 or < 45 => "from center field",
            >= 45 and < 135 => "from right field",
            >= 135 and < 225 => "from home plate",
            _ => "from left field"
        };

        var precip = Precipitation switch
        {
            PrecipitationType.None => "",
            PrecipitationType.Drizzle => "Light drizzle. ",
            PrecipitationType.Light => "Light rain. ",
            PrecipitationType.Moderate => "Rain. ",
            PrecipitationType.Heavy => "Heavy rain! ",
            PrecipitationType.Thunderstorm => "Thunderstorm! ",
            _ => ""
        };

        return $"{precip}{Sky}, {Temperature:F0}°F, {Humidity:F0}% humidity, " +
               $"wind {WindSpeed:F0} mph {windDir}";
    }
}

/// <summary>
/// Types of precipitation.
/// </summary>
public enum PrecipitationType
{
    None,
    Drizzle,
    Light,
    Moderate,
    Heavy,
    Thunderstorm
}

/// <summary>
/// Sky conditions.
/// </summary>
public enum SkyCondition
{
    Clear,
    PartlyCloudy,
    Cloudy,
    Overcast
}
