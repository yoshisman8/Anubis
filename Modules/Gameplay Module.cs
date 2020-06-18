using Anubis.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Anubis.Modules
{
	public class Gameplay_Module : InteractiveBase<SocketCommandContext>
	{
		public Utilities Utils { get; set; }

		[Command("Health"), Alias("HP")]
		public async Task changeHP(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["health"]);

			int res = hp + value;
			c.Attributes["health"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s Health has changed " + hp + "->" + res + ".");
		    
		}
		[Command("Energy"), Alias("EN")]
		public async Task changeEN(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["energy"]);

			int res = hp + value;
			c.Attributes["energy"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s Energy has changed " + hp + "->" + res + ".");

		}
		[Command("Skill")]
		public async Task RollSkill([Remainder]string skill)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
		}
	}
}
