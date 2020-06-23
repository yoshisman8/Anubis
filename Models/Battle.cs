using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anubis.Models
{
	public class Battle
	{
		[BsonId]
		public ulong ChannelId { get; set; }
		public List<Participant>[] Battlemap { get; set; } = new List<Participant>[9];
		public List<Participant> Participants { get; set; } = new List<Participant>();
		public Participant Current { get; set; }
		public int Round { get; set; } = 0;
		public bool Ongoing { get; set; } = false;
		public bool Started { get; set; } = false;
		public bool MapChanged { get; set; } = false;
		public ulong Director { get; set; }
	}
	public class Participant
	{
		public string Name { get; set; }
		public ulong Player { get; set; }
		public int Initiative { get; set; }
		public string Token { get; set; } = "https://media.discordapp.net/attachments/722857470657036299/725046172175171645/defaulttoken.png";
		public int Id { get; set; }
		public ParticipantType Type { get; set; } = ParticipantType.NPC;
	}
	public enum ParticipantType { Player, NPC }
}
