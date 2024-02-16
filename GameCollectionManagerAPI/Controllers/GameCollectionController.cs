using System;
using GameCollectionManagerAPI.Models;
using GameCollectionManagerAPI.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace GameCollectionManagerAPI.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class GameCollectionController : ControllerBase
    {
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IDB_Service _DBService;

        public GameCollectionController(IDB_Service dB_Services)
        {
            _DBService = dB_Services;
        }

        [HttpGet("{user}")]
        public string GetCollection(string user)
        {
            var response = _DBService.GetGamesAsync(user).Result;
            var jsonResponse = JsonConvert.SerializeObject(response);
            return jsonResponse;
        }
        [HttpGet]
        public string Hello()
        {
            return "hello";
        }
    }
}
