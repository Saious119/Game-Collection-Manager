using GameCollectionManager.Shared.Models;

namespace GameCollectionManagerAPI.Services;

public interface IIGDB_Service
{
    public Task<Game> GetIGDBInfo(string gameName);
    public Task<string> GetCoverArt(int CoverID);
    public Task<MultiplayerModes> GetMultiplayerModes(int gameID);
    public Task<string> GetIGDBToken();
}