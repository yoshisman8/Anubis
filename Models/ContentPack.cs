using Discord;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anubis.Models
{
	public class ContentPack
	{
		[BsonId]
		public int Id { get; set; }
		public ulong Author { get; set; }
		public Metadata Metadata { get; set; }
		public List<Talent> Talents { get; set; } = new List<Talent>();
		public List<GameClass> Classes { get; set; } = new List<GameClass>();
		public List<Passive> Passives { get; set; } = new List<Passive>();
		public List<Dash> Dashes { get; set; } = new List<Dash>();
		public List<Item> Items { get; set; } = new List<Item>();
		public List<Action> Actions { get; set; } = new List<Action>();
	}
	public struct Metadata
	{
		public string name { get; set; }
		public string tag { get; set; }
		public string version { get; set; }
		public string author { get; set; }
	}
}
