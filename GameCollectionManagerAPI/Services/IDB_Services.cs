using GameCollectionManagerAPI.Models;

namespace GameCollectionManagerAPI
{
    public interface IDB_Service
    {
        public List<Game> GetGamesAsync(string user);
    }
}
