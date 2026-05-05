namespace API.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public long TotalRecords { get; set; }
}
