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
            throw new NotImplementedException();
        }
    }
}
