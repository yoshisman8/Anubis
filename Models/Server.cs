using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anubis.Models
{
	public class Server
	{
		[BsonId]
		public ulong Id { get; set; }
		public string Prefix { get; set; }
	}
}
