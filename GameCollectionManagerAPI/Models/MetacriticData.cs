using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCollectionManagerAPI.Models
{
    public class MetacriticData
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public string? genre { get; set; }
        public string? platform { get; set; }
        public string? developer { get; set; }
        public string? thumbnailUrl { get; set; }
        public int? metaScore { get; set; }
        public int? userScore { get; set; }
        public Review[] recentReviews { get; set; }
        public Review[] recentUserReviews { get; set; }
        public MetacriticData(string? title, string? description, string? genre, string? platform, string? developer, string? thumbnailUrl, int? metaScore, int? userScore, Review[] recentReviews, Review[] recentUserReviews)
        {
            this.title = title;
            this.description = description;
            this.genre = genre;
            this.platform = platform;
            this.developer = developer;
            this.thumbnailUrl = thumbnailUrl;
            this.metaScore = metaScore;
            this.userScore = userScore;
            this.recentReviews = recentReviews;
            this.recentUserReviews = recentUserReviews;
        }
        public MetacriticData() 
        {
            this.title = "Dummy";
            this.metaScore = 69;
        }
    }
}
