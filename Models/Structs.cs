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
		
	}
	public class Passive
	{
		public string Name { get; set; }
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
		public string Type { get; set; }
		public string Description { get; set; }
		public string Cost { get; set; }
		public string Skill { get; set; }
		public string Range { get; set; }
	}
	public struct Skill
	{
		public string Discipline { get; set; }
		public string Ability { get; set; }
		public string Boost { get; set; }
		public Skill(string discipline, string ability, string boost)
		{
			Discipline = discipline;
			Ability = ability;
			Boost = boost;
		}
	}
	public static class Constants
	{
		public static Dictionary<string, Skill> Skills = new Dictionary<string, Skill>()
		{
			{ "awareness", new Skill("exploration","insight","nerve") },
			{ "balance", new Skill("exploration","agility","grit") },
			{ "climb", new Skill("exploration","vigor","grit") },
			{ "jump", new Skill("exploration","vigor","grit") },
			{ "lift", new Skill("exploration","vigor","grit") },
			{ "reflex", new Skill("exploration","agility","grit") },
			{ "sneak", new Skill("exploration","agility","grit") },
			{ "swim", new Skill("exploration","agility","grit") },
			{ "cartography", new Skill("survival","insight","nerve") },
			{ "cook", new Skill("survival","insight","nerve") },
			{ "craft", new Skill("survival","insight","nerve") },
			{ "forage", new Skill("survival","insight","nerve") },
			{ "fortitude", new Skill("survival","vigor","grit") },
			{ "heal", new Skill("survival","insight","nerve") },
			{ "nature", new Skill("survival","track","nerve") },
			{ "track", new Skill("survival","insight","nerve") },
			{ "aim", new Skill("combat","agility","grit") },
			{ "defend", new Skill("combat","vigor","grit") },
			{ "fight", new Skill("combat","vigor","grit") },
			{ "maneuver", new Skill("combat","agility","grit") },
			{ "empathy", new Skill("social","precense","nerve") },
			{ "handle animal", new Skill("social","precense","nerve") },
			{ "influence", new Skill("social","precense","nerve") },
			{ "intimidate", new Skill("social","precense","nerve") },
			{ "lead", new Skill("social","precense","nerve") },
			{ "negociate", new Skill("social","precense","nerve") },
			{ "perform", new Skill("social","precense","nerve") },
			{ "resolve", new Skill("social","insight","nerve") },
			{ "access", new Skill("manipulate","insight","nerve") },
			{ "repurpose", new Skill("manipulate","insight","nerve") },
			{ "salvage", new Skill("manipulate","insight","nerve") },
			{ "wield", new Skill("manipulate","insight","nerve") },
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
