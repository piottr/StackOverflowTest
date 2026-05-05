using API.Models;

namespace API.Services.Interfaces
{
    public interface IStackOverflowHttpService
    {
        Task<IEnumerable<TagDto>> GetTagsAsync(int page, int pageSize);
    }
}