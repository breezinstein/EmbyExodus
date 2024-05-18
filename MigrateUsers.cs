
using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class MigrateUsers
    {
        private IMediaServer server1;
        private IMediaServer server2;

        public MigrateUsers(IMediaServer server1, IMediaServer server2)
        {
            this.server1 = server1;
            this.server2 = server2;
        }

        public void Migrate()
        {
            var users1 = server1.GetUsers().Result;
            //print out users
            Console.WriteLine("Users on server 1:");
            foreach (var user in users1)
            {
                Console.WriteLine(user.Name);
            }

            var users2 = server2.GetUsers().Result;

            //check if any users are missing from Jellyfin
            foreach (var user in users1)
            {
                if (users2.Any(x => x.Name == user.Name))
                {
                    Console.WriteLine($"{user.Name} already exists on server 2, skipping...");
                    continue;
                }
                // ask to create user
                Console.WriteLine($"User {user.Name} not found in server 2, add {user.Name}?");
                Console.WriteLine($"Press Y to add {user.Name}, any other key to skip");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key != ConsoleKey.Y)
                {
                    continue;
                }
                string? password = "";
                Console.WriteLine($"Enter password for {user.Name}: ");
                password = Console.ReadLine();
                server2.CreateUser(user.Name, password).Wait();
            }
        }

        public void Delete()
        {
            //select which server to delete user from
            Console.WriteLine("Enter number of server to delete user from: ");
            Console.WriteLine("1. Server 1");
            Console.WriteLine("2. Server 2");
            var serverNumber = int.Parse(Console.ReadLine());
            var server = serverNumber == 1 ? server1 : server2;

            //list out users with numbers
            var users1 = server.GetUsers().Result;
            Console.WriteLine("Users on server 1:");
            for (int i = 0; i < users1.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {users1[i].Name}");
            }
            //select user to delete
            Console.WriteLine("Enter number of user to delete: ");
            var userNumber = int.Parse(Console.ReadLine());
            var user = users1[userNumber - 1];
            //confirm deletion
            Console.WriteLine($"Delete user {user.Name}?");
            Console.WriteLine($"Press Y to delete {user.Name}, any other key to skip");
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key != ConsoleKey.Y)
            {
                return;
            }
            server.DeleteUser(user).Wait();

        }
    }
}
