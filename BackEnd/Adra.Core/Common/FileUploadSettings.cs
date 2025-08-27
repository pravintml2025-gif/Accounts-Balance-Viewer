namespace Adra.Core.Common;

public class FileUploadSettings
{
    public const string SectionName = "FileUpload";
    
    public long MaxFileSizeInBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; set; } = { ".csv", ".tsv", ".txt", ".xlsx", ".xls" };
    public string UploadPath { get; set; } = "uploads";
    public int MaxRecordsPerFile { get; set; } = 10000;
    
    public bool IsValidExtension(string extension)
    {
        return AllowedExtensions.Contains(extension.ToLowerInvariant());
    }
    
    public bool IsValidFileSize(long fileSize)
    {
        return fileSize > 0 && fileSize <= MaxFileSizeInBytes;
    }
}
