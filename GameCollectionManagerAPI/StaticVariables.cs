namespace GameCollectionManagerAPI
{
    public static class StaticVariables
    {
        public static string? GAME_DB_CONNECT_STRING => Environment.GetEnvironmentVariable("gamedb_connect_string");
    }
}
