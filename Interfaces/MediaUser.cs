namespace EmbyExodus.Interfaces
{
    public class MediaUser
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<MediaItem> Library { get; set; }
    }
}
