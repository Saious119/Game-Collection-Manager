namespace GameCollectionManager.Services
{
    public class DiscordConfigReader
    {
        public static string? botToken { get; set; }
        public static string? clientID { get; set; }
        public static string? clientSecret { get; set; }
        public static string? userDiscordID { get; set; }
        public DiscordConfigReader() { }
        public void Init()
        {
            var lines = File.ReadAllLines("DiscordAuth.txt");
            botToken = lines[0];
            clientID = lines[1];
            clientSecret = lines[2];
        }
    }
}
