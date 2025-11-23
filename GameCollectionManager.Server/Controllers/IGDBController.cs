using GameCollectionManager.Shared.Models;
using GameCollectionManagerAPI.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;

namespace GameCollectionManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IGDBController : ControllerBase
    {
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IIGDB_Service _iGDB_Service;

        public IGDBController(IIGDB_Service iGDB_Service)
        {
            _iGDB_Service = iGDB_Service;
        }

        [HttpGet("GetGameInfo/{game}")]
        public async Task<ActionResult<Game>> GetGameInfoAsync(string game)
        {
            try
            {
                var result = await _iGDB_Service.GetIGDBInfo(game);
                try
                {
                    result.multiplayer_mode_flags = await _iGDB_Service.GetMultiplayerModes(result.id);
                }
                catch (Exception ex)
                {
                    log.Warn($"Error fetching multiplayer modes for {game}: {ex.Message}");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.Error($"Error fetching game info for {game}: {ex.Message}");
                return StatusCode(500, $"Error fetching game info: {ex.Message}");
            }
        }
        [HttpGet("GetGameCover/{coverID}")]
        public async Task<ActionResult<string>> GetGameInfoAsync(int coverID)
        {
            try
            {
                var result = await _iGDB_Service.GetCoverArt(coverID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.Error($"Error fetching cover art for {coverID}: {ex.Message}");
                return StatusCode(500, $"Error fetching game cover: {ex.Message}");
            }
        }
        [HttpGet("Search")]
        public async Task<ActionResult<List<Game>>> Search([FromQuery] string name)
        {
            try
            {
                var games = await _iGDB_Service.SearchIGDBInfo(name);
                return Ok(games);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error searching games: {ex.Message}");
            }
        }
    }
}
