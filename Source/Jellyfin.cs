using System.Text;
using System.Text.Json;
using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class Jellyfin : IMediaServer
    {
        public string UrlBase { get; set; }
        public string ApiKey { get; set; }
        public MediaServerType ServerType => MediaServerType.Jellyfin;
        public List<MediaUser> Users => _users;
        public MediaLibrary Library => _library;

        private List<MediaUser> _users = new List<MediaUser>();
        private MediaLibrary _library = new MediaLibrary();

        private HttpClient _client = new HttpClient();

        public Jellyfin(string urlBase, string apiKey)
        {
            UrlBase = urlBase;
            ApiKey = apiKey;
            _library.Items = new List<MediaItem>();
            _client.DefaultRequestHeaders.Add("Authorization", ApiKey);
            GetLibrary().Wait();
        }

        async Task GetLibrary()
        {
            Console.WriteLine("Getting library from Jellyfin");
            //Console.WriteLine("Creating a temporary user to get the library from Jellyfin");
            var user = await CreateUser("embyexodustemp7868", "");
            await GetLibrary(user, _library);
            //Console.WriteLine("Deleting temporary user");
            await DeleteUser(user);
            Console.WriteLine();
        }

        //Get all users from Jellyfin
        public async Task<List<MediaUser>?> GetUsers()
        {
            if (_users.Count > 0)
            {
                return _users;
            }

            var url = $"{UrlBase}Users?api_key={ApiKey}";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            _users = JsonSerializer.Deserialize<List<MediaUser>>(json);
            Console.WriteLine($"Got {_users.Count} users from Jellyfin");
            //StringBuilder stringBuilder = new StringBuilder();
            //int progress = 0;
            //foreach (var user in _users)
            //{
            //    progress++;
            //    //await GetJellyfinWatched(user);
            //    stringBuilder.AppendLine($"User: {user.Name}, ID: {user.Id} ");
            //    Console.Write($"\r{progress}/{_users.Count} users processed");
            //}
            //Console.WriteLine();
            //Console.WriteLine(stringBuilder.ToString());

            return _users;
        }

        //Get each user's watched status from Jellyfin
        public async Task<MediaLibrary> GetWatched(MediaUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=true&Filters=IsPlayed&IncludeItemTypes=Movie,Episode&Fields=ProviderIds";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MediaLibrary>(json);
        }

        //Create Jellyfin users, prompt the user for password to set for the new user
        public async Task<MediaUser> CreateUser(string user, string password)
        {

            var url = $"{UrlBase}Users/New?api_key={ApiKey}";
            var content = new StringContent($"{{\"Name\":\"{user}\",\"Password\":\"{password}\"}}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                //Console.WriteLine($"Failed to create user: {json}");
                throw new Exception($"Failed to create user: {json}");
            }
            else
            {
                var createdUser = JsonSerializer.Deserialize<MediaUser>(json);
               // Console.WriteLine($"Created user: {createdUser.Name}, ID: {createdUser.Id}");
                //add the user to the list of users
                _users.Add(createdUser);
                return createdUser;
            }

        }

        //Delete a user from Jellyfin
        public async Task DeleteUser(MediaUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}?api_key={ApiKey}";
            var response = await _client.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
               // Console.WriteLine($"Failed to delete user: {json}");
                throw new Exception($"Failed to delete user: {json}");
            }
            else
            {
                //Console.WriteLine($"Deleted user: {user.Name}");
                _users.Remove(user);
            }
        }

        private async Task GetLibrary(MediaUser user, MediaLibrary _library)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=True&Fields=ProviderIds&IncludeItemTypes=Episode,Movie";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var _lib = JsonSerializer.Deserialize<MediaLibrary>(json);
            _library.Items = _lib.Items;

            Console.WriteLine($"Jellyfin Library contains {_library.Items.Count} items");
        }

        public async Task UpdateWatchedStatus(MediaUser user, Dictionary<string, List<MediaSyncItem>> mediaSyncItems)
        {
            int progress = 0;
            Console.WriteLine();
            foreach (MediaSyncItem media in mediaSyncItems[user.Name])
            {
                progress++;
                if (media.DestinationID != null)
                {
                    string apiUrl = $"{UrlBase}Users/{user.Id}/PlayedItems/{media.DestinationID}?api_key={ApiKey}";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("api_key", ApiKey);
                    request.Content = new StringContent(JsonSerializer.Serialize(new { Name = media.Name, Id = media.DestinationID, Played = 1 }), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    Console.WriteLine($"Couldn't find Id for {media.Name}\n{string.Join(", ", media.ProviderIds)}");
                }
                Console.Write($"Updating items {progress}/{mediaSyncItems[user.Name].Count}\r");
            }
            Console.WriteLine();
            Console.WriteLine($"Updated {progress} items for {user.Name}");
        }
    }
}
