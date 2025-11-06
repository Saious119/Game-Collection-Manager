using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameCollectionManager.Shared.Models
{
    public class GameJsonConverter : JsonConverter<Game>
    {
        public override Game ReadJson(JsonReader reader, Type objectType, Game existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var gameDao = serializer.Deserialize<GameDAO>(reader);
            if (gameDao == null) return null;

            var game = new Game
            {
                id = gameDao.id,
                name = gameDao.name,
                aggregated_rating = gameDao.aggregatedrating ?? 0,
                cover = gameDao.cover ?? 0,
                summary = gameDao.summary,
                status = gameDao.status,
                queuepos = gameDao.queuepos,
                howLongToBeat = gameDao.howlongtobeat ?? 0,
                metacriticScore = gameDao.metacriticscore ?? 0
            };

            // Handle genres
            if (!string.IsNullOrEmpty(gameDao.genres))
            {
                try
                {
                    game.genres = JsonConvert.DeserializeObject<List<Genre>>(gameDao.genres) ?? new List<Genre>();
                }
                catch { game.genres = new List<Genre>(); }
            }
            else
            {
                game.genres = new List<Genre>();
            }

            // Handle platforms
            if (!string.IsNullOrEmpty(gameDao.platforms))
            {
                try
                {
                    game.platforms = JsonConvert.DeserializeObject<List<Platforms>>(gameDao.platforms) ?? new List<Platforms>();
                }
                catch { game.platforms = new List<Platforms>(); }
            }
            else
            {
                game.platforms = new List<Platforms>();
            }

            // Handle release dates
            if (!string.IsNullOrEmpty(gameDao.releasedates))
            {
                try
                {
                    game.release_dates = JsonConvert.DeserializeObject<List<ReleaseDates>>(gameDao.releasedates) ?? new List<ReleaseDates>();
                }
                catch { game.release_dates = new List<ReleaseDates>(); }
            }
            else
            {
                game.release_dates = new List<ReleaseDates>();
            }

            return game;
        }

        public override void WriteJson(JsonWriter writer, Game value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}