using GameCollectionManager.Shared.Models;

namespace GameCollectionManagerAPI
{
    public interface IDB_Service
    {
        public Task<List<GameDAO>> GetGamesAsync(string user);
        public Task<List<GameDAO>> GetQueueAsync(string user);
        public Task CreateTable(GameDAO game);
        public Task SimpleUpsert(GameDAO game);
        public Task SimpleDelete(GameDAO game);
        public Task AddToQueueAsync(GameDAO game);
        public Task RemoveFromQueueAsync(GameDAO game);

    }
}
