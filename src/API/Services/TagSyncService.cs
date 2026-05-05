using API.Data;
using API.Models;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TagSyncService : ITagSyncService
{
    private readonly IStackOverflowHttpService _stackOverflowService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TagSyncService> _logger;

    public TagSyncService(IStackOverflowHttpService stackOverflowService, IServiceScopeFactory scopeFactory, ILogger<TagSyncService> logger)
    {
        _stackOverflowService = stackOverflowService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task SyncTagsIfEmptyAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var count = await context.Tags.CountAsync();
        if (count == 0)
        {
            await PerformSyncAsync(context);
        }
        else
        {
            _logger.LogInformation("Database already has {Count} tags. Skipping sync.", count);
        }
    }

    public async Task ForceSyncAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await PerformSyncAsync(context);
    }

    private async Task PerformSyncAsync(AppDbContext context)
    {
        var allFetchedTags = await FetchTagsAsync();  
        
        await UpsertTagsAsync(context, allFetchedTags);
    }

    private async Task<List<TagDto>> FetchTagsAsync()
    {
        const int totalRequired = 1000;
        const int pageSize = 100;

        int pagesToFetch = (int)Math.Ceiling((double)totalRequired / pageSize);

        var semaphore = new SemaphoreSlim(3);

        var tasks = Enumerable.Range(1, pagesToFetch)
            .Select(async page =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _stackOverflowService.GetTagsAsync(page, pageSize);
                }
                finally
                {
                    semaphore.Release();
                }
            });

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(tags => tags)
            .ToList();
    }

    private async Task UpsertTagsAsync(
        AppDbContext context,
        List<TagDto> fetchedTags)
    {
        var totalCount = fetchedTags.Sum(t => (long)t.Count); 

        foreach (var dto in fetchedTags)
        {
            decimal percentage = totalCount > 0 ? ((decimal)dto.Count / totalCount) * 100m : 0m;

            var existing = await context.Tags.FirstOrDefaultAsync(t => t.Name == dto.Name);
            if (existing != null)
            {
                existing.Count = dto.Count;
                existing.Percentage = percentage;
            }
            else
            {
                context.Tags.Add(new Tag
                {
                    Name = dto.Name,
                    Count = dto.Count,
                    Percentage = percentage
                });
            }
        }

        await context.SaveChangesAsync();
    } 
}
