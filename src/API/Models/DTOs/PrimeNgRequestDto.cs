using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class PrimeNgRequestDto
{
    [Range(0, int.MaxValue)]
    public int First { get; set; } = 0;

    [Range(1, 100)]
    public int Rows { get; set; } = 10;

    public string? SortField { get; set; }

    [Range(-1, 1)]
    public int SortOrder { get; set; } = 1;
}