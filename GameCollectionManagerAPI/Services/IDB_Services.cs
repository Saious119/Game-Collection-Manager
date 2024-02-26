using GameCollectionManagerAPI.Models;

namespace GameCollectionManagerAPI
{
    public interface IDB_Service
    {
        public string ConnectionStringBuilder();
        public Task<List<Game>> GetGamesAsync(string user);
        public void SimpleUpsert(string user, Game game);
        public void SimpleDelete(string user, Game game);

    }
}
