using Anubis.Models;
using Anubis.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using Discord;

namespace Anubis.Modules
{
	public class Character_Management : InteractiveBase<SocketCommandContext>
	{
		public Utilities Utils { get; set; }
		public LiteDatabase Database { get; set; }

		[Command("Set")]
		public async Task Set(string attribute, string newmin, string newmax)
		{

		}
		[Command("Create"), Alias("New")] 
		public async Task create([Remainder]string name)
		{
			var user = Utils.GetUser(Context.User.Id);

			var character = new Character()
			{
				Owner = Context.User.Id,
				Name = name
			};

			var col = Database.GetCollection<Character>("Characters");

			var value = col.Insert(character);
			var u = col.FindOne(x => x.Id == value.AsInt32);
			user.Active = u;
			Utils.UpdateUser(user);

			await ReplyAsync(Context.User.Mention + ", Character `" + name + "` has been created and assigned as your active character!");
		}
		[Command("Delete"), Alias("Del","Remove","Rem")]
		public async Task Del ([Remainder] string Name)
		{
			var chars = Utils.GetAllCharacters(Context.User.Id);
			if (chars == null)
			{
				await ReplyAsync(Context.User.Mention + ", You have no characters.");
				return;
			}

			var query = chars.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToList();
			if (query.Count == 0)
			{
				await ReplyAsync(Context.User.Mention + ", I could not find a character you created with the name \"" + Name + "\".");
				return;
			}
			else if(query.Count == 1)
			{
				var ch = query.FirstOrDefault();
				var user = Utils.GetUser(Context.User.Id);

				user.Active = ch;
				Utils.UpdateUser(user);
				await ReplyAsync(Context.User.Mention + ", Your active character is now `" + ch.Name + "`.");
			}
			else
			{
				var sb = new StringBuilder();
				for(int i = 0; i < query.Count; i++)
				{
					sb.AppendLine("`[" + i + "]` " + query[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", It appears more than one of your characters starts with the word \"" + Name + "\". Please respond with the number that matches the charcter you wish to set as your active:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention+", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (index >= chars.Count())
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention+", This isn't one of the options. Please use the command again.");
						try
						{
							await reply.DeleteAsync();
						}
						catch
						{

						}
						return;
					}
					else
					{
						var c = query.ElementAt(index);
						var u = Utils.GetUser(Context.User.Id);
						u.Active= c;
						Utils.UpdateUser(u);
						await ReplyAsync("Changed your active character to " + c.Name + ".");
						try
						{
							await reply.DeleteAsync();
						}
						catch
						{

						}
						return;
					}
				}
				else
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. I'm ");
					return;
				}
			}
		}
		[Command("Character"), Alias("Char","Sheet")]
		public async Task viewcurr()
		{
			var ch = Utils.GetCharacter(Context.User.Id);
			if (ch == null)
			{
				await ReplyAsync(Context.User.Mention+", You have no active character. Create a new one or change your active character to an existing one.");

				return;
			}

			else
			{
				await ReplyAsync(" ", false, Utils.RenderSheet(ch));
			}
		}
	}
}
