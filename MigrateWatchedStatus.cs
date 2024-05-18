using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class MigrateWatchedStatus
    {
        private IMediaServer server1;
        private IMediaServer server2;

        public MigrateWatchedStatus(IMediaServer server1, IMediaServer server2)
        {
            this.server1 = server1;
            this.server2 = server2;
        }

        public void Migrate()
        {
            var users1 = server1.GetUsers().Result;
            var users2 = server2.GetUsers().Result;

            foreach (var user in users1)
            {
                var tempUser = users2.FirstOrDefault(x => x.Name == user.Name);
                if (tempUser == null)
                {
                    //Console.WriteLine($"User {user.Name} not found in server 2, skipping...");
                    continue;
                }
                //confirm if user wants to migrate watched status
                Console.WriteLine($"Migrate watched status for {user.Name}?");
                Console.WriteLine($"Press Y to migrate watched status for {user.Name}, any other key to skip");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key != ConsoleKey.Y)
                {
                    continue;
                }
                var watched = server1.GetWatched(user).Result;
                Console.WriteLine($"Migrating {watched.Items.Count} watched items for {user.Name}");
                Program.mediaSyncItems.Add(user.Name, new List<MediaSyncItem>());
                int progress = 0;
                Console.WriteLine();
                foreach (var item in watched.Items)
                {
                    progress++;
                    MediaSyncItem media = new MediaSyncItem
                    {
                        Type = item.Type,
                        Server1Id = item.Id,
                        Name = item.Name,
                        ProviderIds = item.ProviderIds
                    };
                    Console.Write($"Getting items {progress}/{watched.Items.Count}\r");
                    media.ProviderIds.Remove("sonarr");
                    Program.mediaSyncItems[user.Name].Add(media);
                }
                Console.WriteLine();

                Console.WriteLine($"Finding IDs for {user.Name}");
                progress = 0;
                foreach (MediaSyncItem item in Program.mediaSyncItems[user.Name])
                {
                    progress++;
                    Console.Write($"{progress}/{Program.mediaSyncItems[user.Name].Count}\r");
                    item.Server2Id = Program.FindItemInLibrary(server2.Library.Items, item);
                }
                server2.UpdateWatchedStatus(tempUser, Program.mediaSyncItems).Wait();
            }
        }
    }
}
