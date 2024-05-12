using System.Text;
using System.Text.Json;

namespace EmbyExodus
{
    public class Jellyfin
    {
        public string UrlBase { get; set; }
        public string ApiKey { get; set; }
        private List<JellyfinUser> _users = new List<JellyfinUser>();
        private JellyfinLibrary _library = new JellyfinLibrary();
        private HttpClient _client = new HttpClient();

        public Jellyfin(string urlBase, string apiKey)
        {
            UrlBase = urlBase;
            ApiKey = apiKey;
            _library.Items = new List<JellyfinItem>();
            _client.DefaultRequestHeaders.Add("Authorization", ApiKey);
        }
        //Get all users from Jellyfin
        public async Task<List<JellyfinUser>?> GetJellyfinUsers()
        {
            if (_users.Count > 0)
            {
                return _users;
            }

            var url = $"{UrlBase}Users?api_key={ApiKey}";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<JellyfinUser>>(json);
            Console.WriteLine($"Got {users.Count} users from Jellyfin");
            StringBuilder stringBuilder = new StringBuilder();
            int progress = 0;
            foreach (var user in users)
            {
                progress++;
                //await GetJellyfinWatched(user);
                stringBuilder.AppendLine($"User: {user.Name}, ID: {user.Id} ");
                Console.Write($"\r{progress}/{users.Count} users processed");
            }
            Console.WriteLine();
            Console.WriteLine(stringBuilder.ToString());
            return users;
        }

        //Get each user's watched status from Jellyfin
        public async Task GetJellyfinWatched(JellyfinUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=true&Filters=IsPlayed&IncludeItemTypes=Movie,Episode&Fields=ProviderIds";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            user.Library = JsonSerializer.Deserialize<JellyfinLibrary>(json).Items;
        }

        //Create Jellyfin users, prompt the user for password to set for the new user
        public async Task<JellyfinUser> AddJellyFinUser(string user, string password)
        {

            var url = $"{UrlBase}Users/New?api_key={ApiKey}";
            var content = new StringContent($"{{\"Name\":\"{user}\",\"Password\":\"{password}\"}}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to create user: {json}");
                throw new Exception($"Failed to create user: {json}");
            }
            else
            {
                var createdUser = JsonSerializer.Deserialize<JellyfinUser>(json);
                Console.WriteLine($"Created user: {createdUser.Name}, ID: {createdUser.Id}");
                return createdUser;
            }

        }


        public async Task<EmbyItem?> GetJellyfinItem(EmbyUser user, string itemId)
        {
            var url = $"{UrlBase}/Users/{user.Id}/Items/{itemId}?api_key={ApiKey}&Recursive=True&Fields=ProviderIds&IncludeItemTypes=Episode,Movie";
            var response = await _client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var item = JsonSerializer.Deserialize<EmbyItem>(json);
            return item;
        }

        public async Task UpdateUserLibrary(JellyfinUser user)
        {
            if (_library.Items.Count == 0)
            {
                var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=True&Fields=ProviderIds&IncludeItemTypes=Episode,Movie";
                var response = await _client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();
                _library = JsonSerializer.Deserialize<JellyfinLibrary>(json);
                user.Library = _library.Items;
            }
            else
            {
                user.Library = _library.Items;
            }

        }

        public async Task UpdateWatchedStatus(JellyfinUser user, Dictionary<string, List<MediaSyncItem>> mediaSyncItems)
        {
            int progress = 0;
            Console.WriteLine();
            foreach (MediaSyncItem media in mediaSyncItems[user.Name])
            {
                progress++;
                if (media.JellyfinId != null)
                {
                    string apiUrl = $"{UrlBase}Users/{user.Id}/PlayedItems/{media.JellyfinId}?api_key={ApiKey}";
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                    request.Headers.Add("accept", "application/json");
                    request.Headers.Add("api_key", ApiKey);
                    request.Content = new StringContent(JsonSerializer.Serialize(new { Name = media.Name, Id = media.JellyfinId, Played = 1 }), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    Console.WriteLine($"Couldn't find Id for {media.Name}\n{string.Join(", ", media.ProviderIds)}");
                }
                Console.Write($"Updating items {progress}/{mediaSyncItems[user.Name].Count}\r");
            }


        }
    }
}

public class JellyfinUser
{
    public string Name { get; set; }
    public string Id { get; set; }
    public List<JellyfinItem> Library { get; set; }
}

public class JellyfinItem
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    public Dictionary<string, string> ProviderIds { get; set; }
}

public class JellyfinLibrary
{
    public List<JellyfinItem> Items { get; set; }
}