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

            log.Info("Found collection for user: " + user);
            //return json to frontend
            return new List<Game>();
        }

        public string ConnectionStringBuilder()
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            connStringBuilder.SslMode = SslMode.VerifyFull;
            string? databaseUrlEnv = "postgresql://user:password@manga-tracker-5581.g8z.cockroachlabs.cloud:26257/mangadb?sslmode=verify-full";
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
            connStringBuilder.Database = "bank";
            SimpleConnection(connStringBuilder.ConnectionString);
        }
        static void SimpleConnection(string connectString)
        {
            //using (var conn = new)
        }
    }
}
