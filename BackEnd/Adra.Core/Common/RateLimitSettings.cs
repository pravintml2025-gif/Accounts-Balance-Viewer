namespace Adra.Core.Common;

public class RateLimitSettings
{
    public const string SectionName = "RateLimit";
    
    public int PermitLimit { get; set; } = 100;
    public int WindowInMinutes { get; set; } = 1;
    public int QueueLimit { get; set; } = 10;
    
    public int UploadPermitLimit { get; set; } = 10;
    public int UploadWindowInMinutes { get; set; } = 5;
}
