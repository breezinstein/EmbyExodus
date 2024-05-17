using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbyExodus.Interfaces
{
    internal interface IMediaServer
    {
        List<IMediaUser> Users { get; set; }
        IMediaLibrary Library { get; set; }
        public Task<List<IMediaUser>?> GetUsers();
        public Task<IMediaLibrary> GetWatched(IMediaUser user);
        public Task<IMediaUser> CreateUser(string user, string password);
        public Task<IMediaUser> UpdateUserWatched(IMediaUser user, IMediaLibrary library);
    }

    internal interface IMediaUser
    {
        string Name { get; set; }
        string Id { get; set; }
        List<IMediaItem> Library { get; set; }
    }

    internal interface IMediaLibrary
    {
        List<IMediaItem> Items { get; set; }
    }

    internal interface IMediaItem
    {
        string Type { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        Dictionary<string, string> ProviderIds { get; set; }
    }
}
