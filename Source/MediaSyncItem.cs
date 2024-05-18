namespace EmbyExodus
{
    public class MediaSyncItem
    {
        public string Type { get; set; }
        public string SourceID { get; set; }
        public string? DestinationID { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> ProviderIds { get; set; }
    }
}