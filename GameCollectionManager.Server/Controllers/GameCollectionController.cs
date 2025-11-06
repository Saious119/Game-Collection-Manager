using System;
using System.Collections;
using System.Net;
using GameCollectionManager.Shared.Models;
using GameCollectionManagerAPI.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace GameCollectionManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameCollectionController : ControllerBase
    {
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IDB_Service _DBService;
        private readonly IMetaCritic_Services _CriticService;

        public GameCollectionController(IDB_Service dB_Services, IMetaCritic_Services openCritic_Services)
        {
            _DBService = dB_Services;
            _CriticService = openCritic_Services;
        }

        [HttpGet("GetCollection/{user}")]
        public async Task<ActionResult<List<GameDAO>>> GetCollection(string user)
        {
            try
            {
                var response = await _DBService.GetGamesAsync(user);
                return Ok(response); // Return the data directly, ASP.NET Core will handle the serialization
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Get Info For User: " + user);
            }
        }

        [HttpPost]
        [Route("AddNewGame")]
        public async Task<IActionResult> PostNewGame([FromBody] GameDAO gameDAO, string user)
        {
            Console.WriteLine("Adding a new Game!");
            if (gameDAO == null)
            {
                Console.WriteLine("You didn't send a game");
                return StatusCode(400, "You didn't send a game");
            }
            try
            {
                gameDAO.owner = user;
                await _DBService.SimpleUpsert(gameDAO);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Add New Game");
            }
        }

        [HttpPost]
        [Route("DeleteGame")]
        public async Task<IActionResult> DeleteGame([FromBody] GameDAO gameDAO, string user)
        {
            try
            {
                gameDAO.owner = user;
                await _DBService.SimpleDelete(gameDAO);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Failed to Delete Game");
            }
        }

        [HttpGet]
        [Route("GetMetaCriticInfo")]
        public async Task<MetacriticData> getOpenCriticInfo(string name, string platform)
        {
            //return new MetacriticData();
            return await _CriticService.getMetaCriticInfo(name, platform);
        }
        [HttpGet]
        [Route("GetQueue/{user}")]
        public async Task<ActionResult<List<GameDAO>>> GetQueue(string user)
        {
            try
            {
                var response = await _DBService.GetQueueAsync(user);
                return Ok(response); // Return the data directly, ASP.NET Core will handle the serialization
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Get Queue For User: " + user);
            }
        }
        [HttpPost]
        [Route("AddToQueue")]
        public async Task<IActionResult> AddToQueue([FromBody] GameDAO game, string user)
        {
            Console.WriteLine("Adding a new Game to Queue!");
            if (game == null)
            {
                Console.WriteLine("You didn't send a game");
                return StatusCode(400, "You didn't send a game");
            }
            try
            {
                game.owner = user;
                await _DBService.AddToQueueAsync(game);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Add New Game to Queue");
            }
        }
        [HttpPost]
        [Route("RemoveFromQueue")]
        public async Task<IActionResult> RemoveFromQueue([FromBody] GameDAO game, string user)
        {
            if (game == null)
            {
                Console.WriteLine("You didn't send a game");
                return StatusCode(400, "You didn't send a game");
            }
            try
            {
                game.owner = user;
                await _DBService.RemoveFromQueueAsync(game);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Remove Game From Queue");
            }
        }
    }
}
