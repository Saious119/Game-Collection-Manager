using System;

namespace GameCollectionManagerAPI.Models
{
    public class Game
    {
        public string name { get; set; } //id primary key = name

        public string releaseDate { get; set; }

        public string platform { get; set; }

        public string metacriticScore { get; set; }

        public string howlong { get; set; }

        public Game(string name, string releaseDate, string platform, string metacriticScore, string howlong)
        {
            this.name = name;
            this.releaseDate = releaseDate;
            this.platform = platform;
            this.metacriticScore = metacriticScore;
            this.howlong = howlong;
        }
    }
}
