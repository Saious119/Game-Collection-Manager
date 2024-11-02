using System;
using log4net;
using GameCollectionManagerAPI.Models;
using System.Data;
using System.Net.Security;
using Npgsql;
using Newtonsoft.Json;

namespace GameCollectionManagerAPI.Services
{
    public class DB_Services : IDB_Service
    {
        List<Game> gameList = new List<Game>();
        private ILog log = LogManager.GetLogger(typeof(Program));
        private readonly IConfiguration _configuration;

        public DB_Services(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public string ConnectionStringBuilder()
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            connStringBuilder.SslMode = SslMode.VerifyFull;
            //string? databaseUrlEnv = StaticVariables.GAME_DB_CONNECT_STRING;
            string databaseUrlEnv = this._configuration["gamedb_connect_string"];
            Console.WriteLine(databaseUrlEnv);
            if (databaseUrlEnv == null)
            {
                Console.WriteLine("Setting DB to Localhost");
                connStringBuilder.Host = "localhost";
                connStringBuilder.Port = 26257;
                connStringBuilder.Username = "username";
                connStringBuilder.Passfile = "password";
                connStringBuilder.IncludeErrorDetail = true;
            }
            else
            {
                Console.WriteLine("Found Environment DB Connect String");
                Uri databaseUrl = new Uri(databaseUrlEnv);
                connStringBuilder.Host = databaseUrl.Host;
                connStringBuilder.Port = databaseUrl.Port;
                var items = databaseUrl.UserInfo.Split(new[] { ':' });
                if (items.Length > 0) { connStringBuilder.Username = items[0]; }
                if (items.Length > 1) { connStringBuilder.Password = items[1]; }
                connStringBuilder.IncludeErrorDetail = true;
            }
            connStringBuilder.Database = "gamedb";
            Console.WriteLine("Going to connect");
            return connStringBuilder.ToString();
        }
        public async Task<List<Game>> GetGamesAsync(string user)
        {
            if (user == null)
            {
                user = "NoUser";
            }
            //Connect to CockroachDB for Data
            string connectString = ConnectionStringBuilder();
            try
            {
                gameList = new List<Game>();
                using (var conn = new NpgsqlConnection(connectString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"SELECT * FROM {user}", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Game gameToAdd = new Game
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("id")),
                                    aggregated_rating = reader.GetFloat(reader.GetOrdinal("aggregated_rating")),
                                    cover = reader.GetInt32(reader.GetOrdinal("cover")),
                                    release_dates = JsonConvert.DeserializeObject<List<ReleaseDates>>(reader.GetString(reader.GetOrdinal("release_dates"))),
                                    genres = JsonConvert.DeserializeObject<List<Genre>>(reader.GetString(reader.GetOrdinal("genres"))),
                                    involved_companies = JsonConvert.DeserializeObject<List<InvolvedCompanies>>(reader.GetString(reader.GetOrdinal("involved_companies"))),
                                    multiplayer_modes = JsonConvert.DeserializeObject<List<int>>(reader.GetString(reader.GetOrdinal("multiplayer_modes"))),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    platforms = JsonConvert.DeserializeObject<List<Platforms>>(reader.GetString(reader.GetOrdinal("platforms"))),
                                    summary = reader.GetString(reader.GetOrdinal("summary")),
                                    multiplayer_mode_flags = JsonConvert.DeserializeObject<MultiplayerModes>(reader.GetString(reader.GetOrdinal("multiplayer_mode_flags"))),
                                    howLongToBeat = reader.GetFloat(reader.GetOrdinal("howLongToBeat")),
                                    status = reader.GetString(reader.GetOrdinal("status"))
                                };
                                gameList.Add(gameToAdd);
                            }
                        }
                    }
                }
                return gameList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Game>();
            }
        }
        public void SimpleUpsert(string user, Game game)
        {
            try
            {
                string connectString = ConnectionStringBuilder();
                using (var conn = new NpgsqlConnection(connectString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS {user} (Id INTEGER PRIMARY KEY, Name VARCHAR, AggregatedRating REAL, Cover INTEGER, ReleaseDates JSONB, Genres JSONB, InvolvedCompanies JSONB, MultiplayerModes JSONB, Platforms JSONB, Summary VARCHAR, MultiplayerModeFlags JSONB)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand(""))
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $"UPSERT INTO {user}(Id, Name, AggregatedRating, Cover, ReleaseDates, Genres, InvolvedCompanies, MultiplayerModes, Platforms, Summary, MultiplayerModeFlags) VALUES(@Id, @Name, @AggregatedRating, @Cover, @ReleaseDates, @Genres, @InvolvedCompanies, @MultiplayerModes, @Platforms, @Summary, @MultiplayerModeFlags)";
                        cmd.Parameters.AddWithValue("id", game.id);
                        cmd.Parameters.AddWithValue("name", game.name);
                        cmd.Parameters.AddWithValue("aggregated_rating", game.aggregated_rating);
                        cmd.Parameters.AddWithValue("cover", game.cover);
                        cmd.Parameters.AddWithValue("release_dates", JsonConvert.SerializeObject(game.release_dates));
                        cmd.Parameters.AddWithValue("genres", JsonConvert.SerializeObject(game.genres));
                        cmd.Parameters.AddWithValue("involved_companies", JsonConvert.SerializeObject(game.involved_companies));
                        cmd.Parameters.AddWithValue("multiplayer_modes", JsonConvert.SerializeObject(game.multiplayer_modes));
                        cmd.Parameters.AddWithValue("platforms", JsonConvert.SerializeObject(game.platforms));
                        cmd.Parameters.AddWithValue("summary", game.summary);
                        cmd.Parameters.AddWithValue("multiplayer_mode_flags", JsonConvert.SerializeObject(game.multiplayer_mode_flags));
                        cmd.Parameters.AddWithValue("howLongToBeat", game.howLongToBeat);
                        cmd.Parameters.AddWithValue("status", game.status);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
        public void SimpleDelete(string user, Game game)
        {
            try
            {
                string connectString = ConnectionStringBuilder();
                using(var conn = new NpgsqlConnection(connectString))
                {
                    conn.Open();
                    using(var cmd = new NpgsqlCommand($"DELETE FROM {user} WHERE id='{game.id}'", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}
