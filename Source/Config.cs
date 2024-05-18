namespace EmbyExodus
{
    public enum MediaServerType { Emby, Jellyfin }
    public class Config
    {
        public ServerConfig server1;
        public ServerConfig server2;
        //Constructor that loads config from an ini file
        public Config(string path)
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }
                var key = parts[0];
                var value = parts[1];
                switch (key)
                {
                    case "server1UrlBase":
                        server1.UrlBase = value;
                        break;
                    case "server1ApiKey":
                        server1.ApiKey = value;
                        break;
                    case "server1Type":
                        if (value == "emby")
                        {
                            server1.Type = MediaServerType.Emby;
                        }
                        else if (value == "jellyfin")
                        {
                            server1.Type = MediaServerType.Jellyfin;
                        }
                        break;
                    case "server2UrlBase":
                        server2.UrlBase = value;
                        break;
                    case "server2ApiKey":
                        server2.ApiKey = value;
                        break;
                    case "server2Type":
                        if (value == "emby")
                        {
                            server2.Type = MediaServerType.Emby;
                        }
                        else if (value == "jellyfin")
                        {
                            server2.Type = MediaServerType.Jellyfin;
                        }
                        break;
                }
            }
            //validate config
            if (server1.UrlBase == null || server1.ApiKey == null || server2.UrlBase == null || server2.ApiKey == null)
            {
                throw new Exception("Invalid config file");
            }
        }
    }

    public struct ServerConfig
    {
        public string UrlBase { get; set; }
        public string ApiKey { get; set; }
        public MediaServerType Type { get; set; }
    }
}
