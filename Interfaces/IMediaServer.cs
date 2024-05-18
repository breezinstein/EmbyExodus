using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbyExodus.Interfaces
{
    public interface IMediaServer
    {
        MediaServerType ServerType { get; }
        List<MediaUser> Users { get; }
        MediaLibrary Library { get; }
        Task<List<MediaUser>?> GetUsers();
        Task<MediaLibrary> GetWatched(MediaUser user);
        Task<MediaUser> CreateUser(string user, string? password);
        Task DeleteUser(MediaUser user);
        Task UpdateWatchedStatus(MediaUser user, Dictionary<string, List<MediaSyncItem>> mediaSyncItems);
    }
}
