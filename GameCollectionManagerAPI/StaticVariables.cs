namespace GameCollectionManagerAPI
{
    public static class StaticVariables
    {
        public static string? GAME_DB_CONNECT_STRING => Environment.GetEnvironmentVariable("gamedb_connect_string");
        public static string? IGDB_CLIENT_ID => Environment.GetEnvironmentVariable("IGDB_CLIENT_ID");
        public static string? IGDB_CLIENT_SECRET => Environment.GetEnvironmentVariable("IGDB_CLIENT_SECRET");
    }
}
