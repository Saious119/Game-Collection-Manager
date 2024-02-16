using GameCollectionManagerAPI.Models;

namespace GameCollectionManagerAPI
{
    public interface IDB_Service
    {
        public Task<List<Game>> GetGamesAsync(string user);
    }
}
