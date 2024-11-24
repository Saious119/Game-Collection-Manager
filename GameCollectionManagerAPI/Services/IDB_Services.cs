using GameCollectionManagerAPI.Models;

namespace GameCollectionManagerAPI
{
    public interface IDB_Service
    {
        public Task<List<GameDAO>> GetGamesAsync(string user);
        public Task CreateTable(GameDAO game);
        public Task SimpleUpsert(GameDAO game);
        public Task SimpleDelete(GameDAO game);

    }
}
