namespace API.Services.Interfaces;

public interface ITagSyncService
{
    Task SyncTagsIfEmptyAsync();
    Task ForceSyncAsync();
}
