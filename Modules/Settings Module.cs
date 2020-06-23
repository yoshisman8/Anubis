using Anubis.Models;
using Anubis.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anubis.Modules
{
	public class Settings_Module : InteractiveBase<SocketCommandContext>
	{
		public Utilities Utils { get; set; }
		public LiteDatabase Database { get; set; }

		[Command("Prefix")] [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
		[RequireContext(ContextType.Guild)]
		public async Task ChangePrefix([Remainder]string Prefix)
		{
			var col = Database.GetCollection<Server>("Servers");

			var server = col.FindById(Context.Guild.Id);

			server.Prefix = Prefix[0].ToString();
			col.Update(server);
			await ReplyAsync(Context.User.Mention + ", changed prefix to `" + Prefix[0] + "`.");
		}
	}
}
