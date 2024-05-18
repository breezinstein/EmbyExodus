using System.Text;
using System.Text.Json;
using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class Emby : IMediaServer
    {
        public string UrlBase { get; set; }
        public string ApiKey { get; set; }
        public MediaServerType ServerType => MediaServerType.Emby;
        public List<MediaUser> Users { get => _users; set => _users = value; }
        public MediaLibrary Library { get => _library; set => _library = value; }

        private List<MediaUser> _users = new List<MediaUser>();
        private MediaLibrary _library = new MediaLibrary();

        private HttpClient _client = new HttpClient();

        public Emby(string urlBase, string apiKey)
        {
            UrlBase = urlBase;
            ApiKey = apiKey;
            _library.Items = new List<MediaItem>();
            GetLibrary().Wait();
        }

        private async Task GetLibrary()
        {
            Console.WriteLine("Getting library from Emby");
            //Console.WriteLine("Creating a temporary user to get the library from Emby");
            var user = await CreateUser("embyexodustemp7868", "");
            await GetLibrary(user, _library);
            //Console.WriteLine("Deleting temporary user");
            await DeleteUser(user);
            Console.WriteLine();
        }

        public async Task<List<MediaUser>?> GetUsers()
        {
            if (_users.Count > 0)
            {
                return _users;
            }

            string url = $"{UrlBase}Users?api_key={ApiKey}";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<MediaUser>>(json);
            //validate users
            if (users == null)
            {
                Console.WriteLine("No users found in Emby");
                return null;
            }
            Console.WriteLine($"Got {users.Count} users from Emby");
            return users;
        }

        public async Task<MediaUser> CreateUser(string user, string password)
        {
            var url = $"{UrlBase}Users/New?api_key={ApiKey}&Name={user}&Password={password}";
            var response = await _client.PostAsync(url, null);
            var json = await response.Content.ReadAsStringAsync();
            var newUser = JsonSerializer.Deserialize<MediaUser>(json);
            if (newUser == null)
            {
                Console.WriteLine($"Failed to create user {user} in Emby");
                return null;
            }
            //Console.WriteLine($"User {newUser.Name} created in Emby");
            return newUser;
        }

        public async Task DeleteUser(MediaUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}?api_key={ApiKey}";
            var response = await _client.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                //Console.WriteLine($"User {user.Name} deleted from Emby");
            }
            else
            {
                //Console.WriteLine($"Failed to delete user {user.Name} from Emby");
            }
        }

        public async Task<MediaLibrary> GetWatched(MediaUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=true&Filters=IsPlayed&IncludeItemTypes=Movie,Episode&Fields=ProviderIds";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var library = JsonSerializer.Deserialize<MediaLibrary>(json);
            //validate library
            if (library.Items == null)
            {
                Console.WriteLine($"No watched items found for user {user.Name}");
                library.Items = new List<MediaItem>();
            }
            return library;
        }


        public async Task GetLibrary(MediaUser user, MediaLibrary library)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=true&IncludeItemTypes=Movie,Episode&Fields=ProviderIds";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var lib = JsonSerializer.Deserialize<MediaLibrary>(json);
            library.Items = lib.Items;
            Console.WriteLine($"Emby Library contains {library.Items.Count} items");
            
        }

        public async Task UpdateWatchedStatus(MediaUser user, Dictionary<string, List<MediaSyncItem>> mediaSyncItems)
        {
            int progress = 0;
            foreach (MediaSyncItem media in mediaSyncItems[user.Name])
            {
                progress++;
                if (media.DestinationID != null)
                {
                    //call the emby api to update the watched status
                    var url = $"{UrlBase}Users/{user.Id}/PlayedItems/{media.DestinationID}?api_key={ApiKey}";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("api_key", ApiKey);
                    request.Content = new StringContent(JsonSerializer.Serialize(new { Name = media.Name, Id = media.DestinationID, Played = 1 }), Encoding.UTF8, "application/json");
                    var response = await _client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    Console.WriteLine($"No Emby ID found for {media.Name}");
                }
                Console.Write($"Updating items: {progress}/{mediaSyncItems[user.Name].Count}\r");
            }
        }
    }
}
