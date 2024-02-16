using System;
using GameCollectionManagerAPI.Models;
using GameCollectionManagerAPI.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace GameCollectionManagerAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class GameCollectionController : ControllerBase
	{
        private ILog log = LogManager.GetLogger(typeof(Program));
		private DB_Services _DBService;

        public GameCollectionController(DB_Services dB_Services)
		{
			_DBService = dB_Services;
		}

		[HttpGet("{user}")]
		public string GetCollection(string user){
			DB_Services db = new DB_Services();
			var response = db.GetGamesAsync(user);
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

