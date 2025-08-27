namespace Adra.Core.Common;

public class JwtSettings
{
    public const string SectionName = "Jwt";
    
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int TokenLifetimeInMinutes { get; set; } = 120;
    
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               !string.IsNullOrWhiteSpace(Key) &&
               Key.Length >= 32 &&
               TokenLifetimeInMinutes > 0;
    }
}
