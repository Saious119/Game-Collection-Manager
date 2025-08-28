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
    [Route("[controller]")]
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
        public async Task<IActionResult> GetCollection(string user)
        {
            try
            {
                var response = await _DBService.GetGamesAsync(user);
                var jsonResponse = JsonConvert.SerializeObject(response);
                return Ok(jsonResponse);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Get Info For User: " + user);
            }
        }

        [HttpPost]
        [Route("AddNewGame")]
        public async Task<IActionResult> PostNewGame([FromBody] Game game, string user)
        {
            Console.WriteLine("Adding a new Game!");
            if(game == null)
            {
                Console.WriteLine("You didn't send a game");
                return StatusCode(400, "You didn't send a game");
            }
            try
            {
                GameDAO gameDAO = game.ToGameDao();
                gameDAO.owner = user;
                await _DBService.SimpleUpsert(gameDAO);
                return Ok();
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Failed to Add New Game");
            }
        }

        [HttpPost]
        [Route("DeleteGame")]
        public async Task<IActionResult> DeleteGame([FromBody] Game game, string user) 
        {
            try
            {
                GameDAO gameDAO = game.ToGameDao();
                gameDAO.owner = user;
                await _DBService.SimpleDelete(gameDAO);
                return Ok();
            } catch(Exception ex)
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
    }
}
