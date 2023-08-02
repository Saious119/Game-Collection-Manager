using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameCollectionManagerAPI.Models
{
	public class Game
	{
		[BsonId]
		public ObjectId id { get; set; }

		[BsonElement("Name")]
		public string name { get; set; }

		[BsonElement("ReleaseDate")]
		public string releaseDate { get; set; }

		[BsonElement("Platform")]
		public string platform { get; set; }

		[BsonElement("MetacriticScore")]
		public float metacriticScore { get; set; }

		[BsonElement("HowLongToBeat")]
		public float howlong { get; set; }
	}
}

