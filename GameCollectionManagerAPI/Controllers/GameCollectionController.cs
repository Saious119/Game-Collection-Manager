using System;
using System.Collections;
using System.Net;
using GameCollectionManagerAPI.Models;
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
        public string GetCollection(string user)
        {
            try
            {
                var response = _DBService.GetGamesAsync(user).Result;
                var jsonResponse = JsonConvert.SerializeObject(response);
                return jsonResponse;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        [HttpPost]
        [Route("AddNewGame")]
        public HttpStatusCode PostNewGame(string user, [FromBody] Game game)
        {
            Console.WriteLine("Adding a new Game!");
            if(game == null)
            {
                Console.WriteLine("You didn't send a game");
                return HttpStatusCode.BadRequest;
            }
            try
            {
                _DBService.SimpleUpsert(user, game);
                return HttpStatusCode.OK;
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return HttpStatusCode.InternalServerError;
            }
        }

        [HttpPost]
        [Route("DeleteGame")]
        public HttpStatusCode DeleteGame(string user, Game game) 
        {
            try
            {
                _DBService.SimpleDelete(user, game);
                return HttpStatusCode.OK;
            } catch(Exception ex)
            {
                Console.WriteLine(ex); 
                return HttpStatusCode.InternalServerError;
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
