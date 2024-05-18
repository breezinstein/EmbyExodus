namespace EmbyExodus
{
    public class MediaItem
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> ProviderIds { get; set; }
    }
}
