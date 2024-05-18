using EmbyExodus;
using EmbyExodus.Interfaces;

internal class Program
{
    public static Dictionary<string, List<MediaSyncItem>> mediaSyncItems = new Dictionary<string, List<MediaSyncItem>>();

    private static MigrateUsers migrateUsers;
    private static MigrateWatchedStatus migrateWatchedStatus;
    private static SyncWatchedStatus syncWatchedStatus;

    private static IMediaServer CreateServer(ServerConfig config)
    {
        return config.Type == MediaServerType.Emby
            ? new Emby(config.UrlBase, config.ApiKey)
            : new Jellyfin(config.UrlBase, config.ApiKey);
    }

    private static async Task Main(string[] args)
    {
        Console.WriteLine("===== Welcome to Emby Exodus!=====");
        //Load config from file
        Config config = new Config("settings.ini");

        IMediaServer server1 = CreateServer(config.server1);
        IMediaServer server2 = CreateServer(config.server2);

        migrateUsers = new MigrateUsers(server1, server2);
        migrateWatchedStatus = new MigrateWatchedStatus(server1, server2);
        syncWatchedStatus = new SyncWatchedStatus(server1, server2);

        mediaSyncItems = new Dictionary<string, List<MediaSyncItem>>();

        //Select What to do
        await ShowMenu(server1, server2);
    }

    private static async Task ShowMenu(IMediaServer server1, IMediaServer server2)
    {
        Console.WriteLine($"Server 1:{server1.ServerType} => Server 2:{server2.ServerType}");
        Console.WriteLine("What would you like to do?");
        Console.WriteLine("1. Migrate Users: Migrate users from one server to another");
        Console.WriteLine("2. Migrate Watched Status: Migrate watched status from one server to another");
        Console.WriteLine("3. Sync Watched Status: Sync watched status between servers");
        Console.WriteLine("4. Exit");
        Console.WriteLine("Enter the number of the action you would like to perform");
        var action = Console.ReadLine();
        switch (action)
        {
            case "1":
                await MigrateUsers();
                await ShowMenu(server1, server2);
                break;
            case "2":
                await MigrateWatchedStatus();
                await ShowMenu(server1, server2);
                break;
            case "3":
                await SyncWatchedStatus();
                await ShowMenu(server1, server2);
                break;
            case "4":
                Console.WriteLine("Exiting...");
                return;
            default:
                Console.WriteLine("Invalid action, exiting...");
                return;
        }
    }

    private static async Task MigrateUsers()
    {
        migrateUsers.Migrate();
    }

    private static async Task MigrateWatchedStatus()
    {
        migrateWatchedStatus.Migrate();
    }

    private static async Task SyncWatchedStatus()
    {
        syncWatchedStatus.Sync();
    }

    private static async Task DeleteUsers()
    {
        migrateUsers.Delete();
    }

    public static string? FindItemInLibrary(List<MediaItem> library, MediaSyncItem item)
    {
        foreach (var libraryItem in library)
        {
            if (item.Type != libraryItem.Type)
            {
                continue;
            }
            foreach (var providerId in item.ProviderIds)
            {
                if (libraryItem.ProviderIds.ContainsKey(providerId.Key) && libraryItem.ProviderIds[providerId.Key] == providerId.Value)
                {
                    return libraryItem.Id;
                }
            }
        }
        return null;
    }
}

public class MediaSyncItem
{
    public string Type { get; set; }
    public string Server1Id { get; set; }
    public string? Server2Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> ProviderIds { get; set; }
}
