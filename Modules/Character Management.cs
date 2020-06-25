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
						if(Attribute.ToLower() == "image" || Attribute.ToLower() == "token")
						{
							if (!value.IsImageUrl())
							{
								await ReplyAsync(Context.User.Mention + ", this isn't a valid image url.");
								return;
							}
						}
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
		public async Task SlotTalent(string Slot, [Remainder]string Name)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			if (Slot.ToLower() != "1" && Slot.ToLower() != "2" && Slot.ToLower() != "3" &&
				Slot.ToLower() != "4" && Slot.ToLower() != "passive" && Slot.ToLower() != "dash")
			{
				await ReplyAsync(Context.User.Mention + ", Invalid stalent slot. Valid slots: 1, 2, 3, 4, passive and dash.");
				return;
			}
			switch (Slot.ToLower())
			{
				case "dash":
					{
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
								if (Math.Abs(index) >= dashes.Length)
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
									await msg.ModifyAsync(x=>x.Content = Context.User.Mention + ", Changed dash talent to " + dashes[index].Name + ".");
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
						break;
					}
				case "passive":
					{

						var passives = Utils.QueryPassive(Name, u);
						if (passives == null || passives.Length == 0)
						{
							await ReplyAsync(Context.User.Mention + ", That isn't a valid Passive talent.");
							return;
						}
						else if (passives.Length == 1)
						{
							c.Passive = passives[0];
							Utils.UpdateCharacter(c);
							await ReplyAsync(Context.User.Mention + ", Changed Passive talent to " + passives[0].Name + ".");
						}
						else if (passives.Length > 1)
						{
							var sb = new StringBuilder();
							for (int i = 0; i < passives.Length; i++)
							{
								sb.AppendLine("`[" + i + "]` " + passives[i].Name);
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
								if (Math.Abs(index) >= passives.Length)
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
									c.Passive = passives[index];
									Utils.UpdateCharacter(c);
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", Changed Passive talent to " + passives[index].Name + ".");
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
						break;
					}
				default:
					{
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
								case "1":
									c.Slot1 = tals[0];
									break;
								case "2":
									c.Slot2 = tals[0];
									break;
								case "3":
									c.Slot3 = tals[0];
									break;
								case "4":
									c.Slot4 = tals[0];
									break;
							}
							Utils.UpdateCharacter(c);
							await ReplyAsync(Context.User.Mention + ", Changed slot " + Slot + " talent to " + tals[0].Name + ".");
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
								if (Math.Abs(index) >= tals.Length)
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
										case "1":
											c.Slot1 = tals[index];
											break;
										case "2":
											c.Slot2 = tals[index];
											break;
										case "3":
											c.Slot3 = tals[index];
											break;
										case "4":
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
						break;
					}
			}
		}
		[Command("Item")]
		public async Task Items(ItemCommand command, [Remainder] string Item)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			switch ((int)command)
			{
				case 1:
					{
						var items = Utils.QueryItem(Item, u);
						if(items.Length == 0)
						{
							await ReplyAsync(Context.User.Mention + ", I couldn't find an item with that name.");
							return;
						}
						else if(items.Length > 1)
						{
							var sb = new StringBuilder();
							for (int i = 0; i < items.Length; i++)
							{
								sb.AppendLine("`[" + i + "]` " + Utils.RenderItemName(items[i]));
							}
							var msg = await ReplyAsync(Context.User.Mention + ", More than one item whose name starts with \"" + Item + "\" was found. Please respond with the number that matches the item you wish to obtain:\n" + sb.ToString());

							var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

							if (reply == null)
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
								return;
							}
							if (int.TryParse(reply.Content, out int index))
							{
								if (Math.Abs(index) >= items.Length)
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
									c.Inventory.Add(items[index]);
									Utils.UpdateCharacter(c);
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", Added item " + items[index].Name + " to " + c.Name + "'s inventory.");
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
						else if (items.Length == 1)
						{
							c.Inventory.Add(items[0]);
							Utils.UpdateCharacter(c);
							await ReplyAsync(Context.User.Mention + ", Added item " + items[0].Name + " to " + c.Name + "'s inventory.");
							return;
						}
						break;
					}
				case 2:
					{
						var iitems = c.Inventory.Where(x => x.Name.ToLower().StartsWith(Item.ToLower())).ToArray();
						if (iitems.Length == 0)
						{
							await ReplyAsync(Context.User.Mention + ", I couldn't find an item with that name.");
							return;
						}
						else if (iitems.Length > 1)
						{
							var sb = new StringBuilder();
							for (int i = 0; i < iitems.Length; i++)
							{
								sb.AppendLine("`[" + i + "]` " + Utils.RenderItemName(iitems[i]));
							}
							var msg = await ReplyAsync(Context.User.Mention + ", More than one item whose name starts with \"" + Item + "\" was found. Please respond with the number that matches the item you wish to obtain:\n" + sb.ToString());

							var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

							if (reply == null)
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
								return;
							}
							if (int.TryParse(reply.Content, out int index))
							{
								if (Math.Abs(index) >= iitems.Length)
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
									c.Inventory.Remove(iitems[index]);
									Utils.UpdateCharacter(c);
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", removed item " + iitems[index].Name + " from " + c.Name + "'s inventory.");
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
						else if (iitems.Length == 1)
						{
							c.Inventory.Remove(iitems[0]);
							Utils.UpdateCharacter(c);
							await ReplyAsync(Context.User.Mention + ", removed item " + iitems[0].Name + " from " + c.Name + "'s inventory.");
							return;
						}
						break;
					}
				case 3:
					{
						var iitems = c.Inventory.Where(x => x.Name.ToLower().StartsWith(Item.ToLower())).ToArray();
						if (iitems.Length == 0)
						{
							await ReplyAsync(Context.User.Mention + ", I couldn't find an item with that name.");
							return;
						}
						else if (iitems.Length > 1)
						{
							var sb = new StringBuilder();
							for (int i = 0; i < iitems.Length; i++)
							{
								sb.AppendLine("`[" + i + "]` " + Utils.RenderItemName(iitems[i]));
							}
							var msg = await ReplyAsync(Context.User.Mention + ", More than one item whose name starts with \"" + Item + "\" was found. Please respond with the number that matches the item you wish to obtain:\n" + sb.ToString());

							var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

							if (reply == null)
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
								return;
							}
							if (int.TryParse(reply.Content, out int index))
							{
								if (Math.Abs(index) >= iitems.Length)
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
									switch (iitems[index].Type)
									{
										case "usable":
											{
												var i = c.Inventory.IndexOf(iitems[index]);
												c.Inventory[i].Used++;
												await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", used item " + iitems[index].Name + " ("+(int.Parse(c.Inventory[i].Frequency)-c.Inventory[i].Used)+" uses remaining).");
												break;
											}
										case "consumable":
											{
												c.Inventory.Remove(iitems[index]);
												await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", cosnumed item " + iitems[index].Name + ".");
												break;
											}
										default:
											{
												var i = c.Inventory.IndexOf(iitems[index]);
												c.Inventory[i].Use = !c.Inventory[i].Use;
												await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", " +(c.Inventory[i].Use?"equipped":"unequipped")+ " item " + iitems[index].Name + ".");
												break;
											}
									}
									Utils.UpdateCharacter(c);
									
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
						else if (iitems.Length == 1)
						{
							switch (iitems[0].Type)
							{
								case "usable":
									{
										var i = c.Inventory.IndexOf(iitems[0]);
										c.Inventory[i].Used++;
										await ReplyAsync(Context.User.Mention + ", used item " + iitems[0].Name + " (" + (int.Parse(c.Inventory[i].Frequency) - c.Inventory[i].Used) + " uses remaining).");
										break;
									}
								case "consumable":
									{
										c.Inventory.Remove(iitems[0]);
										await ReplyAsync(Context.User.Mention + ", cosnumed item " + iitems[0].Name + ".");
										break;
									}
								default:
									{
										var i = c.Inventory.IndexOf(iitems[0]);
										c.Inventory[i].Use = !c.Inventory[i].Use;
										await ReplyAsync(Context.User.Mention + ", " + (c.Inventory[i].Use ? "equipped" : "unequipped") + " item " + iitems[0].Name + ".");
										break;
									}
							}
							Utils.UpdateCharacter(c);
							return;
						}
						break;
					}
			}
		}
		public enum Slot { slot1, slot2, slot3, slot4, passive, dash};
		public enum ItemCommand { Add = 1, Buy = 1, Remove = 2, Delete = 2, Consume = 3, Equip = 3, Use = 3};
		private string[] Numericals { get; set; } = new string[] { "health", "maxhealth", "energy", "maxenergy", "grit", "nerve", "vigor", "agility", "insight", "presence", "exploration", "exploration_judgement", "survival", "survival_judgement", "combat", "combat_judgement", "social", "social_judgement", "manipulate", "manipulate_judgement", "dash", "maxdash", "armor", "bits", "ingredients", "components", "advance", "woe", "corruption" };
	}
}
