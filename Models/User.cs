using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anubis.Models
{
	public class User
	{
		[BsonId]
		public ulong Id { get; set; }
		[BsonRef("Characters")]
		public Character Active { get; set; }

		[BsonRef("ContentPacks")]
		public List<ContentPack> Subscriptions { get; set; } = new List<ContentPack>();
	}
}
