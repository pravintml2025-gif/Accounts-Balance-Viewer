namespace Adra.Application.DTOs;

public class UploadBalanceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ProcessedRecords { get; set; }
    public int SkippedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class BalanceUploadRecord
{
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
