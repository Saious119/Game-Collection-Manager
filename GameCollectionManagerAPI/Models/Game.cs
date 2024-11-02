using System;

namespace GameCollectionManagerAPI.Models
{
    public class Game
    {
        public int id { get; set; }
        public float aggregated_rating { get; set; }
        public int cover { get; set; }
        public List<Genre> genres { get; set; }
        public List<InvolvedCompanies> involved_companies { get; set; }
        public List<int> multiplayer_modes { get; set; }
        public string name { get; set; }
        public List<Platforms> platforms { get; set; }
        public List<ReleaseDates> release_dates { get; set; }
        public string summary { get; set; }
        public MultiplayerModes multiplayer_mode_flags { get; set; }
        public float howLongToBeat {  get; set; }
        public string status { get; set; }
    }
    public class InvolvedCompanies
    {
        public int id { get; set; }
        public Company company { get; set; }
    }
    public class Company
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Platforms
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class ReleaseDates
    {
        public int id { get; set; }
        public string human { get; set; }
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class MultiplayerModes
    {
        public int id { get; set; }
        public bool campaigncoop { get; set; }
        public bool dropin { get; set; }
        public int game { get; set; }
        public bool lancoop { get; set; }
        public bool offlinecoop { get; set; }
        public int offlinecoopmax { get; set; }
        public int offlinemax { get; set; }
        public bool onlinecoop { get; set; }
        public int onlinecoopmax { get; set; }
        public int onlinemax { get; set; }
        public int platform { get; set; }
        public bool splitscreen { get; set; }
        public string checksum { get; set; }
    }
}
