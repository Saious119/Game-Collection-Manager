namespace GameCollectionManagerAPI
{
    public class StaticVariables
    {
        StaticVariables()
        {
            //
        }
        public static string? GAME_DB_CONNECT_STRING => Environment.GetEnvironmentVariable("gamedb_connect_string");
        public static string? IGDB_CLIENT_ID => Environment.GetEnvironmentVariable("IGDB_CLIENT_ID");
        public static string? IGDB_CLIENT_SECRET => Environment.GetEnvironmentVariable("IGDB_CLIENT_SECRET");

        public static string? Discord_ClientId => Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
        public static string? Discord_ClientSecret => Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
        public static string? Discord_RedirectUri => Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI");
    }
}
