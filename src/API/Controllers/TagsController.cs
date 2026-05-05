using API.Data;
using API.Extensions;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITagSyncService _tagSyncService;

    public TagsController(AppDbContext context, ITagSyncService tagSyncService)
    {
        _context = context;
        _tagSyncService = tagSyncService;
    }

    [HttpGet]
    public async Task<PagedResult<TagPercentageDto>> GetTags([FromQuery] PrimeNgRequestDto request)
    {
        var query = _context.Tags.AsNoTracking();

        var totalRecords = await query.CountAsync();

        var data = await ApplySorting(query, request.SortField, request.SortOrder)
            .ApplyPaging(request.First, request.Rows > 0 ? request.Rows : 10)
            .Select(t => new TagPercentageDto
            {
                Name = t.Name,
                Count = t.Count,
                Percentage = Math.Round(t.Percentage, 4)
            })
            .ToListAsync();

        return new PagedResult<TagPercentageDto> { Data = data, TotalRecords = totalRecords };
    }

    private static IQueryable<Tag> ApplySorting(IQueryable<Tag> query, string? field, int order)
    {
        bool isDescending = order == -1;

        if (string.IsNullOrWhiteSpace(field))
            return query;

        return field.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "count" => isDescending ? query.OrderByDescending(x => x.Count) : query.OrderBy(x => x.Count),
            "percentage" => isDescending ? query.OrderByDescending(x => x.Percentage) : query.OrderBy(x => x.Percentage),
            _ => query
        };
    }

    [HttpPost("sync")]
    public async Task<IActionResult> ForceSync()
    {
        await _tagSyncService.ForceSyncAsync();
        return Ok(new { Message = "Synchronization completed successfully." });
    }
}
