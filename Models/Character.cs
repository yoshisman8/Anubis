using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;

namespace Anubis.Models
{
	public class Character
	{
		[BsonId]
		public int Id { get; set; }
		public ulong Owner { get; set; }
		public string Name { get; set; }
		public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>()
		{
			{ "health", "10" },
			{ "maxhealth","10" },
			{ "energy","10"},
			{ "maxenergy","10" },
			{ "class", "" },
			{ "species", ""},
			{ "grit","1" },
			{ "nerve", "1"},
			{ "vigor","0"},
			{ "agility","0"},
			{ "insight", "0" },
			{ "presence","0" },
			{ "exploration", "18" },
			{ "exploration_judgement", "9" },
			{ "survival", "18" },
			{ "survival_judgement", "9" },
			{ "combat", "18" },
			{ "combat_judgement", "9" },
			{ "social", "18" },
			{ "social_judgement", "9" },
			{ "manipulate", "18" },
			{ "manipulate_judgement", "9" },
			{ "dash" , "2" },
			{ "maxdash", "2" },
			{ "armor","0" },
			{ "bits","0" },
			{ "advance","0" },
			{ "woe","0"  },
			{ "corruption", "0" },
			{ "knack",""},
			{ "trait","" },
		};
		public Dash Dash { get; set; }
		public Passive Passive { get; set; }
		public Talent Slot1 { get; set; }
		public Talent Slot2 { get; set; }
		public Talent Slot3 { get; set; }
		public Talent Slot4 { get; set; }

		public List<Feature> Features { get; set; } = new List<Feature>();
		public List<Item> Inventory { get; set; } = new List<Item>();

		
	}

}
