namespace API.Models;

public class TagPercentageDto
{
    public required string Name { get; set; } = string.Empty;
    public required int Count { get; set; }
    public required decimal Percentage { get; set; }
}
