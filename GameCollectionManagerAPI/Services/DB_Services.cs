using System;
using log4net;
using GameCollectionManagerAPI.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace GameCollectionManagerAPI.Services
{
	public class DB_Services
	{
		List<Game> gameList = new List<Game>();
        private ILog log = LogManager.GetLogger(typeof(Program));

        public DB_Services()
		{
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://guest:defaultPass@serverlessinstance.izekv.mongodb.net/?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("GameDB");
        }
        public async Task<List<Game>> GetGamesAsync(string user)
        {
            if (user == null)
            {
                user = "NoUser";
            }
            //Connect to MongoDB for Data
            var settings = MongoClientSettings.FromConnectionString("mongodb+srv://guest:defaultPass@serverlessinstance.izekv.mongodb.net/?retryWrites=true&w=majority");
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("MangaDB");
            IMongoCollection<Game> collection = null;
            log.Info("Checking DB for user: " + user);
            collection = database.GetCollection<Game>(user);
            if (collection == null)
            {
                log.Info("Making new User " + user);
                await database.CreateCollectionAsync(user);
            }
            collection = database.GetCollection<Game>(user);
            log.Info("Found collection for user: " + user);
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
    }
}

