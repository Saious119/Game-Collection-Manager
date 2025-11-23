using System;
using System.Collections.Generic;
using System.Text;

namespace GameCollectionManager.Shared.Models
{
    public class SteamLibraryModel
    {
        public string? game { get; set; }
        public int? id { get; set; }
        public decimal? hours { get; set; }
        public DateTime? last_played { get; set; }
        public string? steam_deck { get; set; }
        public int? metascore { get; set; }
        public int? userscore { get; set; }
        public int? wilsonscore { get; set; }
        public int? sdbrating { get; set; }
        public int? userscore_count { get; set; }
        public DateTime? release_date { get; set; }
    }
}
