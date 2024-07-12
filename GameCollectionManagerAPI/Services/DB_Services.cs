using System;
using log4net;
using GameCollectionManagerAPI.Models;
using System.Data;
using System.Net.Security;
using Npgsql;

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
                                //Console.WriteLine("{0}\n{1}\n{2}\n{3}\n{4}\n{5}",reader.GetValue(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5));
                                Game gameToAdd = new Game(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5));
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
                    using (var cmd = new NpgsqlCommand($"CREATE TABLE IF NOT EXISTS {user} (id STRING PRIMARY KEY, name STRING, releaseDate STRING, platform STRING, metacriticScore STRING, howlong STRING)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand(""))
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = $"UPSERT INTO {user}(id, name, releaseDate, platform, metacriticScore, howlong) VALUES(@id1, @val1, @val2, @val3, @val4, @val5)";
                        cmd.Parameters.AddWithValue("id1", game.name);
                        cmd.Parameters.AddWithValue("val1", game.name);
                        cmd.Parameters.AddWithValue("val2", game.releaseDate);
                        cmd.Parameters.AddWithValue("val3", game.platform);
                        cmd.Parameters.AddWithValue("val4", game.metacriticScore);
                        cmd.Parameters.AddWithValue("val5", game.howlong);
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
                    using(var cmd = new NpgsqlCommand($"DELETE FROM {user} WHERE id='{game.name}'", conn))
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
