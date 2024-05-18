
using EmbyExodus;
using EmbyExodus.Interfaces;

namespace EmbyExodus
{
    public class MigrateUsers
    {
        private IMediaServer sourceServer;
        private IMediaServer destinationServer;

        public MigrateUsers(IMediaServer server1, IMediaServer server2)
        {
            this.sourceServer = server1;
            this.destinationServer = server2;
        }

        public void Migrate()
        {
            Console.WriteLine();
            Console.WriteLine("===== Migrate Users =====");
            var sourceUsers = sourceServer.GetUsers().Result;
            var destinationUsers = destinationServer.GetUsers().Result;

            //creating a list of users in the source server that are not in the destination server
            var usersToAdd = sourceUsers.Where(x => !destinationUsers.Any(y => y.Name == x.Name)).ToList();

            //print out a numbered list of users to add
            Console.WriteLine("Users to add:");
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {usersToAdd[i].Name}");
            }

            Console.WriteLine("Enter the number of the user you would like to add");
            var userNumber = Console.ReadLine();
            if (!int.TryParse(userNumber, out int userIndex) || userIndex < 1 || userIndex > usersToAdd.Count)
            {
                Console.WriteLine("Invalid user number, exiting...");
                return;
            }
            else
            {
                var user = usersToAdd[userIndex - 1];
                Console.WriteLine($"Adding user {user.Name}");
                //ask for password
                Console.WriteLine($"Enter password for {user.Name}: ");
                var password = Console.ReadLine();
                destinationServer.CreateUser(user.Name, password).Wait();
            }
        }

        public void Delete()
        {
            //select which server to delete user from
            Console.WriteLine("Enter number of server to delete user from: ");
            Console.WriteLine("1. Server 1");
            Console.WriteLine("2. Server 2");
            var serverNumber = int.Parse(Console.ReadLine());
            var server = serverNumber == 1 ? sourceServer : destinationServer;

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
