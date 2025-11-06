namespace GameCollectionManager.Shared.Models
{
    public class GameDAO
    {
        public int id { get; set; }
        public string owner { get; set; }
        public float? aggregatedrating { get; set; }
        public int? cover { get; set; }
        public string? genres { get; set; }
        public string? involvedcompanies { get; set; }
        public string? multiplayermodes { get; set; }
        public string? name { get; set; }
        public string? platforms { get; set; }
        public string? releasedates { get; set; }
        public string? summary { get; set; }
        public string? multiplayermodeflags { get; set; }
        public float? howlongtobeat { get; set; }
        public float? metacriticscore { get; set; }
        public string? status { get; set; }
        public int? queuepos { get; set; }
    }
}
