using EmbyExodus;
internal class Program
{
    public static Dictionary<string, List<MediaSyncItem>> mediaSyncItems = new Dictionary<string, List<MediaSyncItem>>();
    private static async Task Main(string[] args)
    {
        Console.WriteLine("===== Welcome to Emby Exodus!=====");
        //Load config from file
        Config config = new Config("settings.ini");
        mediaSyncItems = new Dictionary<string, List<MediaSyncItem>>();

        Emby emby = new Emby(config.embyUrlBase, config.embyApiKey);
        Jellyfin jellyfin = new Jellyfin(config.jellyfinUrlBase, config.jellyfinApiKey);

        List<EmbyUser>? embyUsers = await emby.GetEmbyUsers();

        List<JellyfinUser>? jellyfinUsers = await jellyfin.GetJellyfinUsers();

        //check if any users are missing from Jellyfin
        bool missingUsers = false;
        foreach (var user in embyUsers)
        {
            if (jellyfinUsers.Find(x => x.Name.ToLower().Replace(" ", "") == user.Name.ToLower().Replace(" ", "")) == null)
            {
                missingUsers = true;
                break;
            }
        }

        if (!missingUsers)
        {
            Console.WriteLine("All users are already in Jellyfin, nothing to do\n");
        }
        else
        {
            Console.WriteLine("==== Adding Users to Jellyfin =====");
            foreach (var user in embyUsers)
            {
                //check if user exists in Jellyfin by comparing case-insensitive names and removing spaces
                JellyfinUser? jellyfinUser = jellyfinUsers.Find(x => x.Name.ToLower().Replace(" ", "") == user.Name.ToLower().Replace(" ", ""));
                if (jellyfinUser == null)
                {
                    Console.WriteLine($"User {user.Name} not found in Jellyfin, add {user.Name}?");
                    Console.WriteLine($"Press Y to add {user.Name}, any other key to skip");
                    var key = Console.ReadKey();
                    Console.WriteLine();

                    if (key.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine($"create a password for {user.Name}");
                        var password = Console.ReadLine();
                        var newUser = await jellyfin.AddJellyFinUser(user.Name, password);
                        jellyfinUsers.Add(newUser);
                    }
                    else
                    {
                        Console.WriteLine($"Skipping {user.Name}\n");
                        continue;
                    }
                    Console.WriteLine();
                }
                else
                {
                    jellyfinUser.Name = user.Name;
                }
            }
        }


        Console.WriteLine("===== Adding Sync Watched Status to Jellyfin =====");
        foreach (var jellyfinUser in jellyfinUsers)
        {
            var user = embyUsers.Find(x => x.Name.ToLower().Replace(" ", "") == jellyfinUser.Name.ToLower().Replace(" ", ""));
            Console.WriteLine($"Sync watched status for User: {jellyfinUser.Name}?");
            Console.WriteLine($"Press Y to sync {user.Watched.Items.Count} watched items, any other key to skip");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key != ConsoleKey.Y)
            {
                Console.WriteLine($"Skipping {jellyfinUser.Name}\n");
                continue;
            }

            mediaSyncItems.Add(jellyfinUser.Name, new List<MediaSyncItem>());
            int progress = 0;
            Console.WriteLine();
            foreach (var item in user.Watched.Items)
            {
                progress++;
                MediaSyncItem media = new MediaSyncItem
                {
                    Type = item.Type,
                    EmbyId = item.Id,
                    Name = item.Name,
                    ProviderIds = item.ProviderIds
                };
                Console.Write($"Getting items {progress}/{user.Watched.Items.Count}\r");
                media.ProviderIds.Remove("sonarr");
                mediaSyncItems[jellyfinUser.Name].Add(media);
            }
            Console.WriteLine();

            await jellyfin.UpdateUserLibrary(jellyfinUser);

            Console.WriteLine($"Finding Jellyfin IDs for {jellyfinUser.Name}");
            progress = 0;
            foreach (MediaSyncItem item in mediaSyncItems[jellyfinUser.Name])
            {
                progress++;
                Console.Write($"{progress}/{mediaSyncItems[jellyfinUser.Name].Count}\r");
                item.JellyfinId = FindItemInLibrary(jellyfinUsers.Find(x => x.Name.ToLower().Replace(" ", "") == user.Name.ToLower().Replace(" ", ""))?.Library, item);
            }

            Console.WriteLine($"Updating watched status for {jellyfinUser.Name}");
            await jellyfin.UpdateWatchedStatus(jellyfinUsers.Find(x => x.Name.ToLower().Replace(" ", "") == user.Name.ToLower().Replace(" ", "")), mediaSyncItems);
        }
    }

    private static string? FindItemInLibrary(List<JellyfinItem> library, MediaSyncItem item)
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
    public string EmbyId { get; set; }
    public string? JellyfinId { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> ProviderIds { get; set; }
}