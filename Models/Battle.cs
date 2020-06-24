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
	public struct Participant
	{
		public string Name { get; set; }
		public ulong Player { get; set; }
		public int Initiative { get; set; }
		public string Token { get; set; }
		public int Id { get; set; }
		public ParticipantType Type { get; set; } 

		public static bool operator ==(Participant p1, Participant p2)
		{
			return p1.Equals(p2);
		}
		public static bool operator !=(Participant p1, Participant p2)
		{
			return !p1.Equals(p2);
		}
	}
	public enum ParticipantType { Player, NPC }
}
