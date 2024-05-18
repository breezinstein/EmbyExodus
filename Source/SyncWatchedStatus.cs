using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class SyncWatchedStatus
    {
        private IMediaServer server1;
        private IMediaServer server2;

        public SyncWatchedStatus(IMediaServer server1, IMediaServer server2)
        {
            this.server1 = server1;
            this.server2 = server2;
        }

        public void Sync()
        {
            Console.WriteLine();
            Console.WriteLine("===== Sync Watched Status =====");
            var users1 = server1.GetUsers().Result;
            var users2 = server2.GetUsers().Result;

            //create a list of users that are in both servers
            List<MediaUser> commonUsers = new List<MediaUser>();
            foreach (var tempUser in users1)
            {
                if (users2.Exists(x => x.Name == tempUser.Name))
                {
                    commonUsers.Add(tempUser);
                }
            }

            //for each user, get watched status from both servers and find the difference
            var server1Watched = new Dictionary<string, MediaLibrary>();
            var server2Watched = new Dictionary<string, MediaLibrary>();
            foreach (var user in commonUsers)
            {
                server1Watched.Add(user.Name, server1.GetWatched(user).Result);
                server2Watched.Add(user.Name, server2.GetWatched(user).Result);
            }

            //print out a numbered list of users to sync
            Console.WriteLine("Users to sync:");
            for (int i = 0; i < commonUsers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {commonUsers[i].Name}");
            }

            Console.WriteLine("Enter the number of the user you would like to sync");
            var userNumber = Console.ReadLine();
            if (!int.TryParse(userNumber, out int userIndex) || userIndex < 1 || userIndex > commonUsers.Count)
            {
                Console.WriteLine("Invalid user number, exiting...");
                return;
            }
            else
            {
                var user = commonUsers[userIndex - 1];
                Console.WriteLine($"Syncing user {user.Name}");
                var watched1 = server1Watched[user.Name];
                var watched2 = server2Watched[user.Name];

                List<MediaItem> itemsNotInServer1 = new List<MediaItem>();
                List<MediaItem> itemsNotInServer2 = new List<MediaItem>();

                //find items in server 1 that are not in server 2
                foreach (var item in watched1.Items)
                {
                    if (!watched2.Items.Exists(x => x.Id == item.Id))
                    {
                        itemsNotInServer2.Add(item);
                    }
                }

                //find items in server 2 that are not in server 1
                foreach (var item in watched2.Items)
                {
                    if (!watched1.Items.Exists(x => x.Id == item.Id))
                    {
                        itemsNotInServer1.Add(item);
                    }
                }

                Console.WriteLine($"Found {itemsNotInServer2.Count} items in {server1.ServerType} that are not in {server2.ServerType}");
                Console.WriteLine($"Found {itemsNotInServer1.Count} items in {server2.ServerType} that are not in {server1.ServerType}");

                //server1.UpdateWatchedStatus(user, itemsNotInServer2).Wait();
            }
            
        }
    }
}
