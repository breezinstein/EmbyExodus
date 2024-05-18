using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class MigrateWatchedStatus
    {
        private IMediaServer sourceServer;
        private IMediaServer destinationServer;

        public MigrateWatchedStatus(IMediaServer server1, IMediaServer server2)
        {
            this.sourceServer = server1;
            this.destinationServer = server2;
        }

        public void Migrate()
        {
            Console.WriteLine();
            Console.WriteLine("===== Migrate Watched Status =====");
            var sourceUsers = sourceServer.GetUsers().Result;
            var destinationUsers = destinationServer.GetUsers().Result;

            if (sourceUsers == null || destinationUsers == null)
            {
                Console.WriteLine("No users found in one of the servers, exiting...");
                return;
            }

            //create a list of users that are in both servers
            List<MediaUser> commonUsers = new List<MediaUser>();
            foreach (var tempUser in sourceUsers)
            {
                if (destinationUsers.Exists(x => x.Name == tempUser.Name))
                {
                    commonUsers.Add(tempUser);
                }
            }

            //Print a numbered list of users that are in both servers
            Console.WriteLine("Users found in both servers:");
            for (int i = 0; i < commonUsers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {commonUsers[i].Name}");
            }


            //select user to migrate watched status
            Console.WriteLine("Enter the number of the user you would like to migrate watched status for");
            var userNumber = Console.ReadLine();
            if (!int.TryParse(userNumber, out int userIndex) || userIndex < 1 || userIndex > commonUsers.Count)
            {
                Console.WriteLine("Invalid user number, exiting...");
                return;
            }

            var user = commonUsers[userIndex - 1];

            var watched = sourceServer.GetWatched(user).Result;
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
                    SourceID = item.Id,
                    Name = item.Name,
                    ProviderIds = item.ProviderIds
                };
                Console.Write($"Getting items {progress}/{watched.Items.Count}\r");
                media.ProviderIds.Remove("sonarr");
                Program.mediaSyncItems[user.Name].Add(media);
            }
            Console.WriteLine();

            progress = 0;
            foreach (MediaSyncItem item in Program.mediaSyncItems[user.Name])
            {
                progress++;
                Console.Write($"{progress}/{Program.mediaSyncItems[user.Name].Count}\r");
                item.DestinationID = Program.FindItemInLibrary(destinationServer.Library.Items, item);
            }

            var destUser = destinationUsers.FirstOrDefault(x => x.Name == user.Name);
            destinationServer.UpdateWatchedStatus(destUser, Program.mediaSyncItems).Wait();
            Console.WriteLine($"Migrated {watched.Items.Count} watched items for {user.Name}");
            Console.WriteLine();
        }
    }
}
