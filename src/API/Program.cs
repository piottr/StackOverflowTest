using System.Net;
using API.Data;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:8080");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StackExchangeOptions>(builder.Configuration.GetSection("StackExchange"));

builder.Services.AddHttpClient("StackExchangeClient", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<StackExchangeOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

builder.Services.AddScoped<IStackOverflowHttpService, StackOverflowHttpService>();
builder.Services.AddScoped<ITagSyncService, TagSyncService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    var tagSyncService = services.GetRequiredService<ITagSyncService>();
    await tagSyncService.SyncTagsIfEmptyAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
