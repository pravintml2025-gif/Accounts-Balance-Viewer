namespace Adra.Core.Common;

/// <summary>
/// CORS configuration settings
/// </summary>
public class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>
    /// List of allowed origins for CORS requests
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to allow credentials in CORS requests
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// Whether to allow all localhost origins (useful for development)
    /// </summary>
    public bool AllowAllLocalhost { get; set; } = false;
}
