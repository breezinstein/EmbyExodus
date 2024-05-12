using System.Text;
using System.Text.Json;

namespace EmbyExodus
{
    public class Emby
    {
        public string UrlBase { get; set; }
        public string ApiKey { get; set; }

        private List<EmbyUser> _users = new List<EmbyUser>();

        public Emby(string urlBase, string apiKey)
        {
            UrlBase = urlBase;
            ApiKey = apiKey;
        }
        //Get all users from Emby
        public async Task<List<EmbyUser>?> GetEmbyUsers()
        {
            if (_users.Count > 0)
            {
                return _users;
            }

            string url = $"{UrlBase}Users?api_key={ApiKey}";
            var response = await new HttpClient().GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<EmbyUser>>(json);
            Console.WriteLine($"Got {users.Count} users from Emby");
            int progress = 0;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var user in users)
            {
                progress++;
                await GetEmbyWatched(user);
                stringBuilder.AppendLine($"User: {user.Name}, ID: {user.Id} ");
                Console.Write($"\r{progress}/{users.Count} users processed");
            }
            Console.WriteLine();
            Console.WriteLine(stringBuilder.ToString());
            return users;
        }

        //Get each user's watched status from Emby
        public async Task GetEmbyWatched(EmbyUser user)
        {
            var url = $"{UrlBase}Users/{user.Id}/Items?api_key={ApiKey}&Recursive=true&Filters=IsPlayed&IncludeItemTypes=Movie,Episode&Fields=ProviderIds";
            var response = await new HttpClient().GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            user.Watched = JsonSerializer.Deserialize<EmbyWatched>(json);
        }
    }
}

public class EmbyUser
{
    public string Name { get; set; }
    public string Id { get; set; }

    public EmbyWatched Watched { get; set; }
}

public class EmbyWatched
{
    public List<EmbyItem> Items { get; set; }
    public int TotalRecordCount { get; set; }
}
public class EmbyItem
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string Type { get; set; }
    public Dictionary<string, string> ProviderIds { get; set; }

}