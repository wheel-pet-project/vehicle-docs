namespace Infrastructure.Options;

public class S3Options
{
    public string[] StsBuckets { get; set; } = null!;
    public string[] PtsBuckets { get; set; } = null!;
    public string[] OsagoBuckets { get; set; } = null!;
}