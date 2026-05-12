using System.Text.Json;
using API.Models;
using API.Services.Interfaces;

namespace API.Services;

public class StackOverflowHttpService : IStackOverflowHttpService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StackOverflowHttpService> _logger;

    public StackOverflowHttpService(IHttpClientFactory httpClientFactory, ILogger<StackOverflowHttpService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<TagDto>> GetTagsAsync(int page = 1, int pageSize = 100)
    {
        var httpClient = _httpClientFactory.CreateClient("StackExchangeClient");
        var response = await httpClient.GetAsync($"tags?page={page}&pagesize={pageSize}&order=desc&sort=popular&site=stackoverflow");
        response.EnsureSuccessStatusCode();

        using (var contentStream = await response.Content.ReadAsStreamAsync())
        {
            var stackExchangeResponse = await JsonSerializer.DeserializeAsync<StackExchangeResponse<TagDto>>(contentStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return stackExchangeResponse?.Items ?? Enumerable.Empty<TagDto>();
        }
    }
}
