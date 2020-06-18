using Anubis.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using System.Security.Cryptography.X509Certificates;
using Anubis.Models;

namespace Anubis.Modules
{
	public class Character_Management : InteractiveBase<SocketCommandContext>
	{

		public Utilities Utils { get; set; }

		[Command("Set")]
		public async Task SetValue(string Attribute, [Remainder] string value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			if (c.Attributes.ContainsKey(Attribute.ToLower()))
			{
				if (Numericals.Contains(Attribute.ToLower()))
				{
					if (int.TryParse(value, out int result))
					{
						c.Attributes[Attribute.ToLower()] = value;
						Utils.UpdateCharacter(c);
						await ReplyAsync(Context.User.Mention + ", Changed attribute '" + Attribute.ToLower() + "' to `" + value + "`.");
					}
					else
					{
						await ReplyAsync(Context.User.Mention + ", This attribute can only contain numbers.");
						return;
					}
				}
				else
				{
					if (Attribute.ToLower() == "class")
					{
						var cl = Utils.QueryClass(value, u);
						if (cl == null || cl.Length == 0)
						{
							await ReplyAsync(Context.User.Mention + ", That isn't a valid class.");
							return;
						}
						else if (cl.Length == 1)
						{
							c.Features = cl[0].Features;
							foreach (var att in cl[0].Attributes)
							{
								if (c.Attributes.ContainsKey(att.Key))
								{
									c.Attributes[att.Key] = att.Value;
								}
							}
							c.Attributes["health"] = c.Attributes["maxhealth"];
							c.Attributes["energy"] = c.Attributes["maxenergy"];
							c.Attributes["dash"] = c.Attributes["maxdash"];
							c.Attributes["class"] = cl[0].Name;
							Utils.UpdateCharacter(c);
							await ReplyAsync(Context.User.Mention + ", Changed class to " + cl[0].Name + ". All attributes have been adjusted.");
						}
						else if (cl.Length > 1)
						{
							var sb = new StringBuilder();
							for (int i = 0; i < cl.Length; i++)
							{
								sb.AppendLine("`[" + i + "]` " + cl[i].Name);
							}
							var msg = await ReplyAsync(Context.User.Mention + ", More than one class whose name starts with \"" + value + "\" was found. Please respond with the number that matches the class you wish to assign your active character:\n" + sb.ToString());

							var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

							if (reply == null)
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
								return;
							}
							if (int.TryParse(reply.Content, out int index))
							{
								if (index >= cl.Length)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
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
									c.Features = cl[index].Features;
									foreach (var att in cl[index].Attributes)
									{
										if (c.Attributes.ContainsKey(att.Key))
										{
											c.Attributes[att.Key] = att.Value;
										}
									}
									c.Attributes["health"] = c.Attributes["maxhealth"];
									c.Attributes["energy"] = c.Attributes["maxenergy"];
									c.Attributes["dash"] = c.Attributes["maxdash"];
									c.Attributes["class"] = cl[index].Name;
									Utils.UpdateCharacter(c);
									await ReplyAsync(Context.User.Mention + ", Changed class to " + cl[index].Name + ". All attributes have been adjusted.");
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
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
								return;
							}
						}
					}
					else
					{
						c.Attributes[Attribute.ToLower()] = value;
						Utils.UpdateCharacter(c);
						await ReplyAsync(Context.User.Mention + ", Changed attribute '" + Attribute.ToLower() + "' to `" + value + "`.");
					}
				}
			}
			else
			{
				await ReplyAsync(Context.User.Mention + ", That's not a valid attribute.");
				return;
			}
		}
		[Command("Talents")]
		public async Task ViewTalents()
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			var embed = new EmbedBuilder()
				.WithTitle(c.Name + "'s Talents")
				.WithThumbnailUrl(c.Attributes["image"])
				.AddField("Passive Talent", c.Passive == null ? "None" : c.Passive.Name + "\n" + c.Passive.Description, true)
				.AddField("Dash Talent", c.Dash == null ? "None" : Utils.RenderDash(c.Dash))
				.AddField("Slot 1", c.Slot1 == null ? "None" : Utils.RenderTalent(c.Slot1))
				.AddField("Slot 2", c.Slot2 == null ? "None" : Utils.RenderTalent(c.Slot2))
				.AddField("Slot 3", c.Slot3 == null ? "None" : Utils.RenderTalent(c.Slot3))
				.AddField("Slot 4", c.Slot4 == null ? "None" : Utils.RenderTalent(c.Slot4));

			await ReplyAsync(" ", false, embed.Build());
		}
		[Command("Talent")]
		public async Task SlotTalent(int Slot, [Remainder]string Name)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			if (Slot < 1 || Slot > 4)
			{
				await ReplyAsync(Context.User.Mention + ", Invalid slot number (Min 1, Max 4)");
				return;
			}
			var tals = Utils.QueryTalent(Name, u);
			if (tals == null || tals.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", That isn't a valid talent.");
				return;
			}
			else if (tals.Length == 1)
			{
				switch (Slot)
				{
					case 1:
						c.Slot1 = tals[0];
						break;
					case 2:
						c.Slot2 = tals[0];
						break;
					case 3:
						c.Slot3 = tals[0];
						break;
					case 4:
						c.Slot4 = tals[0];
						break;
				}
				Utils.UpdateCharacter(c);
				await ReplyAsync(Context.User.Mention + ", Changed slot "+Slot+" talent to " + tals[0].Name + ".");
			}
			else if (tals.Length > 1)
			{
				var sb = new StringBuilder();
				for (int i = 0; i < tals.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + tals[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one talent whose name starts with \"" + Name + "\" was found. Please respond with the number that matches the talent you wish to slot:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (index >= tals.Length)
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
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
						switch (Slot)
						{
							case 1:
								c.Slot1 = tals[index];
								break;
							case 2:
								c.Slot2 = tals[index];
								break;
							case 3:
								c.Slot3 = tals[index];
								break;
							case 4:
								c.Slot4 = tals[index];
								break;
						}
						Utils.UpdateCharacter(c);
						await ReplyAsync(Context.User.Mention + ", Changed slot " + Slot + " talent to " + tals[index].Name + ".");
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
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
					return;
				}
			}
		}
		[Command("Dash")]
		public async Task SlotDash([Remainder]string Name)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			var dashes = Utils.QueryDash(Name, u);
			if (dashes == null || dashes.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", That isn't a valid Dash talent.");
				return;
			}
			else if (dashes.Length == 1)
			{
				c.Dash = dashes[0];
				Utils.UpdateCharacter(c);
				await ReplyAsync(Context.User.Mention + ", Changed Dash talent to " + dashes[0].Name + ".");
			}
			else if (dashes.Length > 1)
			{
				var sb = new StringBuilder();
				for (int i = 0; i < dashes.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + dashes[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one talent whose name starts with \"" + Name + "\" was found. Please respond with the number that matches the talent you wish to slot:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (index >= dashes.Length)
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
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
						c.Dash = dashes[index];
						Utils.UpdateCharacter(c);
						await ReplyAsync(Context.User.Mention + ", Changed dash talent to " + dashes[index].Name + ".");
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
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
					return;
				}
			}
		}
		[Command("Passive")]
		public async Task SlotPassive([Remainder]string Name)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			var dashes = Utils.QueryPassive(Name, u);
			if (dashes == null || dashes.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", That isn't a valid Passive talent.");
				return;
			}
			else if (dashes.Length == 1)
			{
				c.Passive = dashes[0];
				Utils.UpdateCharacter(c);
				await ReplyAsync(Context.User.Mention + ", Changed Passive talent to " + dashes[0].Name + ".");
			}
			else if (dashes.Length > 1)
			{
				var sb = new StringBuilder();
				for (int i = 0; i < dashes.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + dashes[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one talent whose name starts with \"" + Name + "\" was found. Please respond with the number that matches the talent you wish to slot:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (index >= dashes.Length)
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
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
						c.Passive = dashes[index];
						Utils.UpdateCharacter(c);
						await ReplyAsync(Context.User.Mention + ", Changed Passive talent to " + dashes[index].Name + ".");
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
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
					return;
				}
			}
		}
		
		
		public enum Slot { slot1, slot2, slot3, slot4, passive, dash};
		private string[] Numericals { get; set; } = new string[] { "health", "maxhealth", "energy", "maxenergy", "grit", "nerve", "vigor", "agility", "insight", "presence", "exploration", "exploration_judgement", "survival", "survival_judgement", "combat", "combat_judgement", "social", "social_judgement", "manipulate", "manipulate_judgement", "dash", "maxdash", "armor", "bits", "ingredients", "components", "advance", "woe", "corruption" };
	}
}
