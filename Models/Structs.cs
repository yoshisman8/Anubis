using Anubis.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Anubis.Models
{
	public class GameClass
	{
		public string Name { get; set; }
		public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
		public List<Feature> Features { get; set; } = new List<Feature>();
	}
	public class Feature 
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
	public class Talent : Action
	{
		public string Discipline { get; set; }
	}
	public class Dash : Action
	{
		public string Discipline { get; set; }
	}
	public class Passive
	{
		public string Name { get; set; }
		public string Discipline { get; set; }
		public string Description { get; set; }
	}
	public class Item 
	{
		public string Name { get; set; }
		public string Cost { get; set; }
		public string Type { get; set; }
		public string Armor { get; set; }
		public string Agility { get; set; }
		public string Threshold { get; set; }
		public string Defense { get; set; }
		public string Attack { get; set; }
		public string Damage { get; set; }
		public string Hands { get; set; }
		public string Range { get; set; }
		public string Energy { get; set; }
		public string Frequency { get; set; }
		public string Effect { get; set; }
		public bool Use { get; set; } = false;
		public int Used { get; set; } = 0;
		public string Note { get; set; }

		public override bool Equals(object obj)
		{
			return (obj is Item) && (Equals(ToString(), obj.ToString()));
		}
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
	public class Action
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Skill { get; set; }
		public bool Roll { get; set; }
	}
	public static class Constants
	{
		public static Dictionary<string, string> Skills = new Dictionary<string, string>()
		{
			{ "awareness", "exploration" },
			{ "balance", "exploration" },
			{ "climb", "exploration" },
			{ "jump", "exploration" },
			{ "lift", "exploration" },
			{ "reflex", "exploration" },
			{ "sneak", "exploration" },
			{ "swim", "exploration" },
			{ "cartography", "survival" },
			{ "cook", "survival" },
			{ "craft", "survival" },
			{ "forage", "survival" },
			{ "fortitude", "survival" },
			{ "heal", "survival" },
			{ "nature", "survival" },
			{ "track", "survival" },
			{ "aim", "combat" },
			{ "defend", "combat" },
			{ "fight", "combat" },
			{ "maneuver", "combat" },
			{ "empathy", "social" },
			{ "handle animal", "social" },
			{ "influence", "social" },
			{ "intimidate", "social" },
			{ "lead", "social" },
			{ "negotiate", "social" },
			{ "perform", "social" },
			{ "resolve", "social" },
			{ "access", "manipulate" },
			{ "repurpose", "manipulate" },
			{ "salvage", "manipulate" },
			{ "wield", "manipulate" },
			{ "exploration", "exploartion"},
			{ "survival", "survival"},
			{ "combat", "combat" },
			{ "social", "social" },
			{ "manipulate", "manipulate" }
		};
		public static Dictionary<string, string> DefaultAttributes { get; set; } = new Dictionary<string, string>()
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
			{ "exploration_judgement", "8" },
			{ "survival", "18" },
			{ "survival_judgement", "8" },
			{ "combat", "18" },
			{ "combat_judgement", "8" },
			{ "social", "18" },
			{ "social_judgement", "8" },
			{ "manipulate", "18" },
			{ "manipulate_judgement", "8" },
			{ "dash" , "2" },
			{ "maxdash", "2" },
			{ "armor","0" },
			{ "bits","0" },
			{ "ingredients","0" },
			{ "components","0" },
			{ "advancement","0" },
			{ "woe","0"  },
			{ "corruption", "0" },
			{ "knack",""},
			{ "trait","" },
			{ "image","" },
			{ "token","" }
		};
		public enum Disciplines { combat, exploration, social, survival, manipulate }
		public enum ItemTypes { armor, shield, weapon, usable, consumable, upgrade }
	}
}
