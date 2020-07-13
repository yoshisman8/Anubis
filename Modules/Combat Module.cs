using Anubis.Models;
using Anubis.Services;
using Dice;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Anubis.Modules
{
	public class Combat_Module : InteractiveBase<SocketCommandContext>
	{
		public Utilities Utils { get; set; }
		public LiteDatabase Database { get; set; }

		[Command("Encounter"), Alias("Combat", "Battle","Enc")]
		[RequireContext(ContextType.Guild)]
		public async Task Encounter(EncounterCommand command = EncounterCommand.Info)
		{
			var battle = Utils.GetBattle(Context.Channel.Id);

			switch ((int)command)
			{
				case 0:
					{
						if (!battle.Ongoing)
						{
							await ReplyAsync(Context.User.Mention + ", There is no active encounter in this channel.");
							return;
						}
						if (battle.MapChanged)
						{
							await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + Context.Channel.Id + ".png"), " ", false, Utils.RenderBattle(battle, true));
							battle.MapChanged = false;
							Utils.UpdateBattle(battle);
							return;
						}
						else
						{
							await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + Context.Channel.Id + ".png"), " ", false, Utils.RenderBattle(battle, false));
							return;
						}
					}
				case 1:
					{
						if (battle.Ongoing && !battle.Started && battle.Director == Context.User.Id)
						{
							battle.Started = true;
							battle.Current = battle.Participants[0];
							battle.Round = 1;
							var user = Context.Guild.GetUser(battle.Current.Player);
							await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + Context.Channel.Id + ".png"), user.Mention + ", " + battle.Current.Name + "'s Turn!", false, Utils.RenderBattle(battle, true));
							battle.MapChanged = false;
							Utils.UpdateBattle(battle);

						}
						else if (battle.Ongoing && battle.Director == Context.User.Id && battle.Participants.Count == 0)
						{
							await ReplyAsync(Context.User.Mention + ", This encounter has no participants!");
							return;
						}
						else if (battle.Ongoing && battle.Started && battle.Director == Context.User.Id)
						{
							await ReplyAsync(Context.User.Mention + ", You're already running an encounter! Use `" + Utils.GetPrefix(Context.Guild.Id) + "Encounter Stop` to stop the current one first.");
							return;
						}
						else if (battle.Ongoing && battle.Director != Context.User.Id)
						{
							await ReplyAsync(Context.User.Mention + ", Someone else is already running a battle in this channel. To end it, use the command `" + Utils.GetPrefix(Context.Guild.Id) + "Encounter Stop`.");
							return;
						}
						else 
						{
							battle.Battlemap = new List<Participant>[9];
							for (int i = 0; i < 9; i++)
							{
								battle.Battlemap[i] = new List<Participant>();
							}
							battle.Director = Context.User.Id;
							battle.Participants = new List<Participant>();
							battle.Round = 0;
							battle.Ongoing = true;
							battle.Current = new Participant();
							battle.MapChanged = false;
							Utils.UpdateBattle(battle);
							var file = Path.Combine(Directory.GetCurrentDirectory(), "data", "battlemap.png");
							var uri = new Uri(file);
							var embed = new EmbedBuilder()
								.WithTitle(Context.User.Username + " has started an encounter in " + Context.Channel.Name + "!")
								.AddField("Players", "Players can join this battle by using the `" + Utils.GetPrefix(Context.Guild.Id) + "Join <Tile> <Initiative>` command!")
								.AddField("NPCs", "The director of the battle can add NPCs to the battle with the `" + Utils.GetPrefix(Context.Guild.Id) + "AddNPC <Initiative> <Tile> <Name>` command!")
								.AddField("Ready to begin?", "Use the `" + Utils.GetPrefix(Context.Guild.Id) + "Battle Start` command again to start combat!")
								.WithImageUrl($"attachment://battlemap.png");
							await Context.Channel.SendFileAsync(file, " ", false, embed.Build());
						}
						break;
					}
				case 2:
					{
						if (battle.Ongoing)
						{
							battle.Ongoing = false;
							battle.Started = false;
							battle.Battlemap = new List<Participant>[9];
							battle.Participants = new List<Participant>();
							battle.Round = 0;

							Utils.UpdateBattle(battle);
							await ReplyAsync(Context.User.Mention + ", Battle over!");
							return;
						}
						else
						{
							await ReplyAsync(Context.User.Mention + ", there is no battle happening on this channel!");
							return;
						}
					}
			}

		}
		[Command("Initiative"), Alias("Join", "init")]
		[RequireContext(ContextType.Guild)]
		public async Task Join(int Tile, int Initiative)
		{
			Tile = Math.Abs(Tile);
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character to join as.");
				return;
			}
			var c = u.Active;
			if (Tile > 9 || Tile < 1)
			{
				await ReplyAsync(Context.User.Mention + ", you're joining an invalid tile (valid numbers are 1-9).");
				return;
			}


			if (b.Participants.Exists(x => x.Id == c.Id))
			{
				var i = b.Participants.FindIndex(x => x.Id == c.Id);
				var token = new Participant()
				{
					Id = c.Id,
					Type = ParticipantType.Player,
					Initiative = Initiative,
					Player = Context.User.Id,
					Name = c.Name,
					Token = (c.Attributes["token"].NullorEmpty() ? (c.Attributes["image"].NullorEmpty() ? "https://media.discordapp.net/attachments/722857470657036299/725046172175171645/defaulttoken.png" : c.Attributes["image"]) : c.Attributes["token"])
				};
				b.Participants[i] = token;
				for (int j = 0; j < 9; j++)
				{
					if (b.Battlemap[j].Exists(x => x.Id == c.Id))
					{
						var ti = b.Battlemap[j].Find(x => x.Id == c.Id);
						b.Battlemap[j].Remove(ti);
					}
				}
				b.Battlemap[Tile - 1].Add(token);
				b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
				b.MapChanged = true;
				Utils.UpdateBattle(b);
				await ReplyAsync(Context.User.Mention + ", " + c.Name + " joined the encounter on " + TileNames[Tile] + " with an initiative roll of `" + Initiative + "`!");
			}
			else
			{
				var token = new Participant()
				{
					Id = c.Id,
					Type = ParticipantType.Player,
					Initiative = Initiative,
					Player = Context.User.Id,
					Name = c.Name,
					Token = (c.Attributes["token"].NullorEmpty() ? (c.Attributes["image"].NullorEmpty() ? "https://media.discordapp.net/attachments/722857470657036299/725046172175171645/defaulttoken.png" : c.Attributes["image"]) : c.Attributes["token"])
				};
				b.Participants.Add(token);
				b.Battlemap[Tile - 1].Add(token);
				b.MapChanged = true;
				b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
				Utils.UpdateBattle(b);
				await ReplyAsync(Context.User.Mention + ", " + c.Name + " joined the encounter on " + TileNames[Tile] + " with an initiative roll of `" + Initiative + "`!");
			}
		}
		[Command("AddNPC")] [RequireContext(ContextType.Guild)]
		public async Task AddNPC(int Tile, int initiative,  string tokenurl, [Remainder]string Name)
		{
			Tile = Math.Abs(Tile);
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			if (b.Ongoing && b.Director != Context.User.Id)
			{
				await ReplyAsync(Context.User.Mention + ", You are not the director for this encounter.");
				return;
			}
			if (Tile > 9 || Tile < 1)
			{
				await ReplyAsync(Context.User.Mention + ", you're joining an invalid tile (valid numbers are 1-9).");
				return;
			}
			if (!tokenurl.IsImageUrl())
			{
				await ReplyAsync(Context.User.Mention + ", this isn't a valid image url.");
				return;
			}
			if (b.Participants.Exists(x => x.Name.ToLower() == Name.ToLower()))
			{
				var i = b.Participants.FindIndex(x => x.Name.ToLower() == Name.ToLower());
				var token = new Participant()
				{
					Type = ParticipantType.NPC,
					Initiative = initiative,
					Player = b.Director,
					Name = Name,
					Token = tokenurl
				};
				b.Participants[i] = token;
				for (int j = 0; j < 9; j++)
				{
					if (b.Battlemap[j].Exists(x => x.Name.ToLower() == Name.ToLower()))
					{
						var ti = b.Battlemap[j].Find(x => x.Name.ToLower() == Name.ToLower());
						b.Battlemap[j].Remove(ti);
					}
				}
				b.Battlemap[Tile - 1].Add(token);
				b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
				b.MapChanged = true;
				Utils.UpdateBattle(b);
				await ReplyAsync(Context.User.Mention + ", Added NPC " + Name + " to the encounter on " + TileNames[Tile] + " with an initiative of `" + initiative + "`!");
			}
			else
			{
				var token = new Participant()
				{
					Type = ParticipantType.NPC,
					Initiative = initiative,
					Player = b.Director,
					Name = Name,
					Token = tokenurl
				};
				b.Participants.Add(token);
				b.Battlemap[Tile - 1].Add(token);
				b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
				b.MapChanged = true;
				Utils.UpdateBattle(b);
				await ReplyAsync(Context.User.Mention + ", Added NPC " + Name + " to the encounter on " + TileNames[Tile] + " with an initiative of `" + initiative + "`!");
			}
		}

		[Command("Remove")]
		[RequireContext(ContextType.Guild)]
		public async Task RemoveNPC([Remainder] string name)
		{
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			if (b.Ongoing && b.Director != Context.User.Id)
			{
				await ReplyAsync(Context.User.Mention + ", You are not the director for this encounter.");
				return;
			}
			var participants = b.Participants.Where(x => x.Name.ToLower().StartsWith(name.ToLower())).ToArray();
			if (participants.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", There are no participants with that name in this encounter.");
				return;
			}
			if (participants.Length == 1)
			{
				var p = participants[0];
				if (b.Current == p)
				{
					TickTurn(ref b);
				}
				b.Participants.Remove(p);
				for (int j = 0; j < 9; j++)
				{
					if (b.Battlemap[j].Exists(x => x == p))
					{
						b.Battlemap[j].Remove(p);
					}
				}
				b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
				Utils.UpdateBattle(b);
				b.MapChanged = true;
				await ReplyAsync(Context.User.Mention + ", Removed " + p.Name + " from the encounter.");
				return;
			}
			else
			{
				var sb = new StringBuilder();
				for (int i = 0; i < participants.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + participants[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one participant was found. Please respond with the number that matches the participant you wish to remove:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (Math.Abs(index) >= participants.Length)
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
						var p = participants[index];
						if (b.Current == p)
						{
							TickTurn(ref b);
						}
						b.Participants.Remove(p);
						for (int j = 0; j < 9; j++)
						{
							if (b.Battlemap[j].Exists(x => x == p))
							{
								b.Battlemap[j].Remove(p);
							}
						}
						b.Participants = b.Participants.OrderBy(x => x.Initiative).Reverse().ToList();
						b.MapChanged = true;
						Utils.UpdateBattle(b);
						await ReplyAsync(Context.User.Mention + ", Removed " + p.Name + " from the encounter.");
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
		[Command("Next"), Alias("Turn")]
		[RequireContext(ContextType.Guild)]
		public async Task Turn()
		{
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			if (!b.Started)
			{
				await ReplyAsync(Context.User.Mention + ", This encounter hasn't started yet!. Start it with `!Encounter Start`");
				return;
			}
			if (b.Current.Player != Context.User.Id && b.Director != Context.User.Id)
			{
				await ReplyAsync(Context.User.Mention + ", It's not your turn!");
				return;
			}
			TickTurn(ref b);
			Utils.UpdateBattle(b);
			var user = Context.Guild.GetUser(b.Current.Player);
			if (b.MapChanged)
			{
				await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + Context.Channel.Id + ".png"), user.Mention + ", " + b.Current.Name + "'s Turn!", false, Utils.RenderBattle(b, true));
				b.MapChanged = false;
				Utils.UpdateBattle(b);
				return;
			}
			else
			{
				await Context.Channel.SendFileAsync(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + Context.Channel.Id + ".png"), user.Mention + ", " + b.Current.Name + "'s Turn!", false, Utils.RenderBattle(b, false));
				return;
			}
		}
		[Command("Move")][RequireContext(ContextType.Guild)]
		public async Task Move(int Tile)
		{
			Tile = Math.Abs(Tile);
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			if (Tile > 9 || Tile < 1)
			{
				await ReplyAsync(Context.User.Mention + ", you're moving to an invalid tile (valid numbers are 1-9).");
				return;
			}
			if(!b.Participants.Exists(x=> x.Id == c.Id))
			{
				await ReplyAsync(Context.User.Mention + ", " + c.Name + " Isn't participating in this Encounter.");
				return;
			}
			var token = b.Participants.Find(x => x.Id == c.Id);
			for (int j = 0; j < 9; j++)
			{
				if (b.Battlemap[j].Exists(x => x.Id == c.Id))
				{
					var ti = b.Battlemap[j].Find(x => x.Id == c.Id);
					b.Battlemap[j].Remove(ti);
				}
			}
			b.Battlemap[Tile - 1].Add(token);
			b.MapChanged = true;
			Utils.UpdateBattle(b);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + " moved to " + TileNames[Tile] + "!");
		}
		
		[Command("Move")]
		[RequireContext(ContextType.Guild)]
		public async Task moveNPC(int Tile, [Remainder] string name)
		{
			Tile = Math.Abs(Tile);
			var b = Utils.GetBattle(Context.Channel.Id);
			if (!b.Ongoing)
			{
				await ReplyAsync(Context.User.Mention + ", There is no encounter happening on this channel. Start one with `!Encounter Start`");
				return;
			}
			if (b.Ongoing && b.Director != Context.User.Id)
			{
				await ReplyAsync(Context.User.Mention + ", You are not the director for this encounter.");
				return;
			}
			if (Tile > 9 || Tile < 1)
			{
				await ReplyAsync(Context.User.Mention + ", you're moving to an invalid tile (valid numbers are 1-9).");
				return;
			}
			var participants = b.Participants.Where(x => x.Name.ToLower().StartsWith(name.ToLower())).ToArray();
			if (participants.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", There are no participants with that name in this encounter.");
				return;
			}
			if (participants.Length == 1)
			{
				var p = participants[0];
				for (int j = 0; j < 9; j++)
				{
					if (b.Battlemap[j].Exists(x => x.Name == p.Name))
					{
						var ti = b.Battlemap[j].Find(x => x.Name == p.Name);
						b.Battlemap[j].Remove(ti);
					}
				}
				b.Battlemap[Tile - 1].Add(p);
				b.MapChanged = true;
				Utils.UpdateBattle(b);
				await ReplyAsync(Context.User.Mention + ", " + p.Name + " moved to "+TileNames[Tile] +".");
				return;
			}
			else
			{
				var sb = new StringBuilder();
				for (int i = 0; i < participants.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + participants[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one participant was found. Please respond with the number that matches the participant you wish to remove:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (Math.Abs(index) >= participants.Length)
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
						var p = participants[index];
						for (int j = 0; j < 9; j++)
						{
							if (b.Battlemap[j].Exists(x => x.Name == p.Name))
							{
								var ti = b.Battlemap[j].Find(x => x.Name == p.Name);
								b.Battlemap[j].Remove(ti);
							}
						}
						b.Battlemap[Tile - 1].Add(p);
						b.MapChanged = true;
						Utils.UpdateBattle(b);
						await ReplyAsync(Context.User.Mention + ", " + p.Name + " moved to " + TileNames[Tile] + ".");
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
		private void TickTurn(ref Battle b)
		{
			int i = b.Participants.IndexOf(b.Current);
			if(i + 1 >= b.Participants.Count)
			{
				b.Current = b.Participants.First();
				b.Round++;
			}
			else
			{
				b.Current = b.Participants[i + 1];
			}
		}
		public enum EncounterCommand { Info = 0, Start = 1, Begin = 1, New = 1,End = 2, Stop = 2, Finish = 2};
		private Dictionary<int, string> TileNames { get; set; } = new Dictionary<int, string>()
		{
			{1, "The Upper Edge" },
			{2, "The Western Flank" },
			{3, "The North Eastern Flank" },
			{4, "The Outer Edge" },
			{5, "The Heat" },
			{6, "The Outer Edge" },
			{7, "The South Western Flank" },
			{8, "The South Eastern Flank" },
			{9, "The Lower Edge" }
		};
	}
}
