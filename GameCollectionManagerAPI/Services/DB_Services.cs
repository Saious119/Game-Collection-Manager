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

        public async Task<List<Game>> GetGamesAsync(string user)
        {
            if (user == null)
            {
                user = "NoUser";
            }
            //Connect to CockroachDB for Data
            ConnectionStringBuilder();
            log.Info("Found collection for user: " + user);
            //return json to frontend
            return new List<Game>();
        }

        public string ConnectionStringBuilder()
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            connStringBuilder.SslMode = SslMode.VerifyFull;
            string? databaseUrlEnv = "postgresql://andy:password@manga-tracker-5581.g8z.cockroachlabs.cloud:26257/mangadb?sslmode=verify-full";
            if (databaseUrlEnv == null)
            {
                connStringBuilder.Host = "localhost";
                connStringBuilder.Port = 26257;
                connStringBuilder.Username = "username";
                connStringBuilder.Passfile = "password";
                connStringBuilder.IncludeErrorDetail = true;
            }
            else
            {
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
            return "Success";
        }
        public string SimpleUpsert(string connectString)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS users (id INT PRIMARY KEY, name STRING, releaseDate STRING, platform STRING, metacriticScore FLOAT, howlong FLOAT)", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand(""))
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "UPSERT INTO user(id, name, releaseDate, platform, metacriticScore, howlong) VALUES(@id1, @val1, @val2, @val3, @val4, @val5)";
                        cmd.Parameters.AddWithValue("id1", 1);
                        cmd.Parameters.AddWithValue("val1", "Halo 4");
                        cmd.Parameters.AddWithValue("val2", "11/6/2012");
                        cmd.Parameters.AddWithValue("val3", "Xbox 360");
                        cmd.Parameters.AddWithValue("val4", "87");
                        cmd.Parameters.AddWithValue("val5", "7.5");
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "Success";
        }
    }
}
