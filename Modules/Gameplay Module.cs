﻿using Anubis.Models;
using Anubis.Services;
using Dice;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
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
		[Command("Dash")]
		public async Task Dash(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["dash"]);

			int res = hp + value;
			c.Attributes["dash"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s Dashes have changed " + hp + "->" + res + ".");
		}
		[Command("Woe")]
		public async Task woe(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["woe"]);

			int res = hp + value;
			c.Attributes["woe"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s Woe has changed " + hp + "->" + res + ".");
		}
		[Command("Corruption")]
		public async Task corruption(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["corruption"]);

			int res = hp + value;
			c.Attributes["corruption"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s Corruption has changed " + hp + "->" + res + ".");
		}
		[Command("Bits"), Alias("Gerbits")]
		public async Task bits(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["bits"]);

			int res = hp + value;
			c.Attributes["bits"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s bits have changed " + hp + "->" + res + ".");
		}
		[Command("Ingredients")]
		public async Task ingredients(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["ingredients"]);

			int res = hp + value;
			c.Attributes["ingredients"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s ingredients have changed " + hp + "->" + res + ".");
		}
		[Command("Components")]
		public async Task comps(int value)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int hp = int.Parse(c.Attributes["components"]);

			int res = hp + value;
			c.Attributes["compnents"] = res.ToString();
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + "'s components have changed " + hp + "->" + res + ".");
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

			if (Constants.Skills.TryGetValue(skill.ToLower(), out Skill value))
			{
				var dice = Roller.Roll("1d20");
				var embed = new EmbedBuilder()
					.WithTitle(c.Name + " makes a " + skill + " check.")
					.WithThumbnailUrl(c.Attributes["image"]);
				var fortune = int.Parse(c.Attributes[value.Discipline]);
				var judgement = int.Parse(c.Attributes[value.Discipline + "_judgement"]);
			

				if (dice.Value <= judgement)
				{
					embed.WithColor(Color.Red);
					embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
				}
				else if (dice.Value >= fortune)
				{
					embed.WithColor(Color.Green);
					embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
				}
				else
				{
					embed.WithColor(new Color(255, 255, 0));
					embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
				}
				await ReplyAsync(" ", false, embed.Build());
			}
			else
			{
				await ReplyAsync(Context.User.Mention + ", This isn't a valid skill.");
				return;
			}
		}

		[Command("Check")]
		public async Task Discipline([Remainder]string Discipline)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;
			if (!Disciplines.Contains(Discipline))
			{
				await ReplyAsync(Context.User.Mention + ", This isn't a valid Discipline");
				return;
			}
			var dice = Roller.Roll("1d20");
			var embed = new EmbedBuilder()
				.WithTitle(c.Name + " makes a " + Discipline + " check.")
				.WithThumbnailUrl(c.Attributes["image"]);
			var fortune = int.Parse(c.Attributes[Discipline]);
			var judgement = int.Parse(c.Attributes[Discipline + "_judgement"]);


			if (dice.Value <= judgement)
			{
				embed.WithColor(Color.Red);
				embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
			}
			else if (dice.Value >= fortune)
			{
				embed.WithColor(Color.Green);
				embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
			}
			else
			{
				embed.WithColor(new Color(255, 255, 0));
				embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
			}
			await ReplyAsync(" ", false, embed.Build());
		}
		[Command("Roll"), Alias("R", "Dice")]
		[Summary("Make a dice roll.")]
		public async Task Roll([Remainder] string Expression)
		{
			try
			{
				var results = Roller.Roll(Expression);
				decimal total = results.Value;


				var embed = new EmbedBuilder()
					.WithTitle(Context.User.Username + " rolled some dice.")
					.WithDescription(ParseResult(results) + "\nTotal = `" + total + "`");

				Random randonGen = new Random();
				Color randomColor = new Color(randonGen.Next(255), randonGen.Next(255),
				randonGen.Next(255));
				embed.WithColor(randomColor);

				await ReplyAsync(" ",false, embed.Build());
			}

			catch
			{
				await ReplyAsync("No dice were found to roll");
			}

		}

		[Command("Act"),Alias("Action")]
		public async Task Act([Remainder]string Name)
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			var action = Utils.QueryActions(Name, u,c);
			if(action.Length == 0)
			{
				await ReplyAsync(Context.User.Mention + ", No action or talent with that name was found.");
				return;
			}
			else if(action.Length == 1)
			{
				if (action[0] as Talent != null)
				{
					var act = action[0] as Talent;
					var embed = new EmbedBuilder()
						.WithTitle(c.Name+" performs "+act.Name+"!")
						.WithThumbnailUrl(c.Attributes["image"])
						.AddField("Talent",Utils.RenderTalent(act));
					var skills = act.Skill.Split(',');
					if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
					{
						var dice = Roller.Roll("1d20");
						var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
						var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


						if (dice.Value <= judgement)
						{
							embed.WithColor(Color.Red);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
						}
						else if (dice.Value >= fortune)
						{
							embed.WithColor(Color.Green);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
						}
						else
						{
							embed.WithColor(new Color(255, 255, 0));
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
						}
						await ReplyAsync(" ", false, embed.Build());
					}
					if (skills.Length == 1 && skills[0] == "any")
					{
						var msg = await ReplyAsync(Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
						var reply = await NextMessageAsync();
						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						else if (Constants.Skills.TryGetValue(reply.Content.ToLower(), out Skill sk))
						{
							var dice = Roller.Roll("1d20");
							var fortune = int.Parse(c.Attributes[sk.Discipline]);
							var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


							if (dice.Value <= judgement)
							{
								embed.WithColor(Color.Red);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
							}
							else if (dice.Value >= fortune)
							{
								embed.WithColor(Color.Green);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
							}
							else
							{
								embed.WithColor(new Color(255, 255, 0));
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
							}
						}
						await msg.ModifyAsync(x => x.Content = ".");
						await msg.ModifyAsync(x => x.Embed = embed.Build());
					}
					else if (skills.Length > 1)
					{
						var sb = new StringBuilder();
						for (int i = 0; i < skills.Length; i++)
						{
							sb.AppendLine("`[" + i + "]` " + skills[i]);
						}
						var msg = await ReplyAsync(Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb.ToString());

						var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						if (int.TryParse(reply.Content, out int index))
						{
							if (Math.Abs(index) >= action.Length)
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
								string sk = skills[index];
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}

								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply.DeleteAsync();
								}
								catch
								{

								}
							}
						}
						else
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
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
					
					return;

				}
				else if (action[0] as Dash != null)
				{
					var act = action[0] as Dash;
					var embed = new EmbedBuilder()
						.WithTitle(c.Name + " performs " + act.Name + "!")
						.WithThumbnailUrl(c.Attributes["image"])
						.AddField("Talent", Utils.RenderDash(act));
					var skills = act.Skill.Split(',');
					if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
					{
						var dice = Roller.Roll("1d20");
						var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
						var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


						if (dice.Value <= judgement)
						{
							embed.WithColor(Color.Red);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
						}
						else if (dice.Value >= fortune)
						{
							embed.WithColor(Color.Green);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
						}
						else
						{
							embed.WithColor(new Color(255, 255, 0));
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
						}
						await ReplyAsync(" ", false, embed.Build());
					}
					if (skills.Length == 1 && skills[0] == "any")
					{
						var msg = await ReplyAsync(Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
						var reply = await NextMessageAsync();
						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						else if (Constants.Skills.TryGetValue(reply.Content.ToLower(), out Skill sk))
						{
							var dice = Roller.Roll("1d20");
							var fortune = int.Parse(c.Attributes[sk.Discipline]);
							var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


							if (dice.Value <= judgement)
							{
								embed.WithColor(Color.Red);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
							}
							else if (dice.Value >= fortune)
							{
								embed.WithColor(Color.Green);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
							}
							else
							{
								embed.WithColor(new Color(255, 255, 0));
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
							}
						}
						await msg.ModifyAsync(x => x.Content = ".");
						await msg.ModifyAsync(x => x.Embed = embed.Build());
					}
					else if (skills.Length > 1)
					{
						var sb = new StringBuilder();
						for (int i = 0; i < skills.Length; i++)
						{
							sb.AppendLine("`[" + i + "]` " + skills[i]);
						}
						var msg = await ReplyAsync(Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb.ToString());

						var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						if (int.TryParse(reply.Content, out int index))
						{
							if (Math.Abs(index) >= action.Length)
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
								string sk = skills[index];
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}

								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply.DeleteAsync();
								}
								catch
								{

								}
							}
						}
						else
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
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
					return;
				}
				else
				{
					var act = action[0];
					var embed = new EmbedBuilder()
						.WithTitle(c.Name + " performs " + act.Name + "!")
						.WithThumbnailUrl(c.Attributes["image"])
						.AddField("Action", Utils.RenderAction(act));
					var skills = act.Skill.Split(',');
					if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
					{
						var dice = Roller.Roll("1d20");
						var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
						var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


						if (dice.Value <= judgement)
						{
							embed.WithColor(Color.Red);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
						}
						else if (dice.Value >= fortune)
						{
							embed.WithColor(Color.Green);
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
						}
						else
						{
							embed.WithColor(new Color(255, 255, 0));
							embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
						}
						await ReplyAsync(" ", false, embed.Build());
					}
					if (skills.Length == 1 && skills[0] == "any")
					{
						var msg = await ReplyAsync(Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
						var reply = await NextMessageAsync();
						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						else if (Constants.Skills.TryGetValue(reply.Content.ToLower(), out Skill sk))
						{
							var dice = Roller.Roll("1d20");
							var fortune = int.Parse(c.Attributes[sk.Discipline]);
							var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


							if (dice.Value <= judgement)
							{
								embed.WithColor(Color.Red);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
							}
							else if (dice.Value >= fortune)
							{
								embed.WithColor(Color.Green);
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
							}
							else
							{
								embed.WithColor(new Color(255, 255, 0));
								embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
							}
						}
						await msg.ModifyAsync(x => x.Content = ".");
						await msg.ModifyAsync(x => x.Embed = embed.Build());
					}
					else if (skills.Length > 1)
					{
						var sb = new StringBuilder();
						for (int i = 0; i < skills.Length; i++)
						{
							sb.AppendLine("`[" + i + "]` " + skills[i]);
						}
						var msg = await ReplyAsync(Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb.ToString());

						var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

						if (reply == null)
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
							return;
						}
						if (int.TryParse(reply.Content, out int index))
						{
							if (Math.Abs(index) >= action.Length)
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
								string sk = skills[index];
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}

								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply.DeleteAsync();
								}
								catch
								{

								}
							}
						}
						else
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
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
					return;
				}
			}
			else if(action.Length > 1)
			{
				var sb = new StringBuilder();
				for (int i = 0; i < action.Length; i++)
				{
					sb.AppendLine("`[" + i + "]` " + action[i].Name);
				}
				var msg = await ReplyAsync(Context.User.Mention + ", More than one action or talent whose name starts with \"" + Name + "\" was found. Please respond with the number of the action/talent you wish to perform:\n" + sb.ToString());

				var reply = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

				if (reply == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
					return;
				}
				if (int.TryParse(reply.Content, out int index))
				{
					if (Math.Abs(index) >= action.Length)
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
						if (action[index] as Talent != null)
						{
							var act = action[index] as Talent;
							var embed = new EmbedBuilder()
								.WithTitle(c.Name + " performs " + act.Name + "!")
								.WithThumbnailUrl(c.Attributes["image"])
								.AddField("Talent", Utils.RenderTalent(act));
							var skills = act.Skill.Split(',');
							if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
							{
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}
								await ReplyAsync(" ", false, embed.Build());
							}
							if (skills.Length == 1 && skills[0] == "any")
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
								var reply1 = await NextMessageAsync();
								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								else if (Constants.Skills.TryGetValue(reply1.Content.ToLower(), out Skill sk))
								{
									var dice = Roller.Roll("1d20");
									var fortune = int.Parse(c.Attributes[sk.Discipline]);
									var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


									if (dice.Value <= judgement)
									{
										embed.WithColor(Color.Red);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
									}
									else if (dice.Value >= fortune)
									{
										embed.WithColor(Color.Green);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
									}
									else
									{
										embed.WithColor(new Color(255, 255, 0));
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
									}
								}
								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply1.DeleteAsync();
								}
								catch { }
							}
							else if (skills.Length > 1)
							{
								var sb1 = new StringBuilder();
								for (int i = 0; i < skills.Length; i++)
								{
									sb1.AppendLine("`[" + i + "]` " + skills[i]);
								}
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb1.ToString());

								var reply1 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								if (int.TryParse(reply1.Content, out int index1))
								{
									if (Math.Abs(index1) >= action.Length)
									{
										await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
										try
										{
											await reply1.DeleteAsync();
										}
										catch
										{

										}
										return;
									}
									else
									{
										string sk = skills[index1];
										var dice = Roller.Roll("1d20");
										var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
										var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


										if (dice.Value <= judgement)
										{
											embed.WithColor(Color.Red);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
										}
										else if (dice.Value >= fortune)
										{
											embed.WithColor(Color.Green);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
										}
										else
										{
											embed.WithColor(new Color(255, 255, 0));
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
										}

										await msg.ModifyAsync(x => x.Content = ".");
										await msg.ModifyAsync(x => x.Embed = embed.Build());
										try
										{
											await reply.DeleteAsync();
										}
										catch
										{

										}
									}
									try
									{
										await reply1.DeleteAsync();
									}
									catch { }
								}
								else
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
									try
									{
										await reply1.DeleteAsync();
									}
									catch
									{

									}
									return;
								}
							}

						}
						else if (action[index] as Dash != null)
						{
							var act = action[index] as Dash;
							var embed = new EmbedBuilder()
								.WithTitle(c.Name + " performs " + act.Name + "!")
								.WithThumbnailUrl(c.Attributes["image"])
								.AddField("Talent", Utils.RenderDash(act));
							var skills = act.Skill.Split(',');
							if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
							{
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}
								await ReplyAsync(" ", false, embed.Build());
							}
							if (skills.Length == 1 && skills[0] == "any")
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
								var reply1 = await NextMessageAsync();
								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								else if (Constants.Skills.TryGetValue(reply1.Content.ToLower(), out Skill sk))
								{
									var dice = Roller.Roll("1d20");
									var fortune = int.Parse(c.Attributes[sk.Discipline]);
									var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


									if (dice.Value <= judgement)
									{
										embed.WithColor(Color.Red);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
									}
									else if (dice.Value >= fortune)
									{
										embed.WithColor(Color.Green);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
									}
									else
									{
										embed.WithColor(new Color(255, 255, 0));
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
									}
								}
								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply1.DeleteAsync();
								}
								catch { }
							}
							else if (skills.Length > 1)
							{
								var sb1 = new StringBuilder();
								for (int i = 0; i < skills.Length; i++)
								{
									sb1.AppendLine("`[" + i + "]` " + skills[i]);
								}
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb1.ToString());

								var reply1 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								if (int.TryParse(reply1.Content, out int index1))
								{
									if (Math.Abs(index1) >= action.Length)
									{
										await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
										try
										{
											await reply1.DeleteAsync();
										}
										catch
										{

										}
										return;
									}
									else
									{
										string sk = skills[index1];
										var dice = Roller.Roll("1d20");
										var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
										var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


										if (dice.Value <= judgement)
										{
											embed.WithColor(Color.Red);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
										}
										else if (dice.Value >= fortune)
										{
											embed.WithColor(Color.Green);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
										}
										else
										{
											embed.WithColor(new Color(255, 255, 0));
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
										}

										await msg.ModifyAsync(x => x.Content = ".");
										await msg.ModifyAsync(x => x.Embed = embed.Build());
										try
										{
											await reply.DeleteAsync();
										}
										catch
										{

										}
									}
									try
									{
										await reply1.DeleteAsync();
									}
									catch { }
								}
								else
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
									try
									{
										await reply1.DeleteAsync();
									}
									catch
									{

									}
									return;
								}
							}
						}
						else
						{
							var act = action[index];
							var embed = new EmbedBuilder()
								.WithTitle(c.Name + " performs " + act.Name + "!")
								.WithThumbnailUrl(c.Attributes["image"])
								.AddField("Action", Utils.RenderAction(act));
							var skills = act.Skill.Split(',');
							if (skills.Length == 1 && skills[0] != "none" && skills[0] != "any")
							{
								var dice = Roller.Roll("1d20");
								var fortune = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline]);
								var judgement = int.Parse(c.Attributes[Constants.Skills[skills[0]].Discipline + "_judgement"]);


								if (dice.Value <= judgement)
								{
									embed.WithColor(Color.Red);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
								}
								else if (dice.Value >= fortune)
								{
									embed.WithColor(Color.Green);
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
								}
								else
								{
									embed.WithColor(new Color(255, 255, 0));
									embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
								}
								await ReplyAsync(" ", false, embed.Build());
							}
							if (skills.Length == 1 && skills[0] == "any")
							{
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action/talent can use any skill. Respond with the name of the skill you wish to use.");
								var reply1 = await NextMessageAsync();
								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								else if (Constants.Skills.TryGetValue(reply1.Content.ToLower(), out Skill sk))
								{
									var dice = Roller.Roll("1d20");
									var fortune = int.Parse(c.Attributes[sk.Discipline]);
									var judgement = int.Parse(c.Attributes[sk.Discipline + "_judgement"]);


									if (dice.Value <= judgement)
									{
										embed.WithColor(Color.Red);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
									}
									else if (dice.Value >= fortune)
									{
										embed.WithColor(Color.Green);
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
									}
									else
									{
										embed.WithColor(new Color(255, 255, 0));
										embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
									}
								}
								await msg.ModifyAsync(x => x.Content = ".");
								await msg.ModifyAsync(x => x.Embed = embed.Build());
								try
								{
									await reply1.DeleteAsync();
								}
								catch { }
							}
							else if (skills.Length > 1)
							{
								var sb1 = new StringBuilder();
								for (int i = 0; i < skills.Length; i++)
								{
									sb1.AppendLine("`[" + i + "]` " + skills[i]);
								}
								await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This action or talent can use more than one skill. Please respond with the number of the skill you wish to roll:\n" + sb1.ToString());

								var reply1 = await NextMessageAsync(timeout: TimeSpan.FromSeconds(10));

								if (reply1 == null)
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You took too respond.");
									return;
								}
								if (int.TryParse(reply1.Content, out int index1))
								{
									if (Math.Abs(index1) >= action.Length)
									{
										await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't one of the options. Please use the command again.");
										try
										{
											await reply1.DeleteAsync();
										}
										catch
										{

										}
										return;
									}
									else
									{
										string sk = skills[index1];
										var dice = Roller.Roll("1d20");
										var fortune = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline]);
										var judgement = int.Parse(c.Attributes[Constants.Skills[act.Skill].Discipline + "_judgement"]);


										if (dice.Value <= judgement)
										{
											embed.WithColor(Color.Red);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Judgement)");
										}
										else if (dice.Value >= fortune)
										{
											embed.WithColor(Color.Green);
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Fortune)");
										}
										else
										{
											embed.WithColor(new Color(255, 255, 0));
											embed.WithDescription(ParseResult(dice) + " = `" + dice.Value + "` (Temperance)");
										}

										await msg.ModifyAsync(x => x.Content = ".");
										await msg.ModifyAsync(x => x.Embed = embed.Build());
										try
										{
											await reply.DeleteAsync();
										}
										catch
										{

										}
									}
									try
									{
										await reply1.DeleteAsync();
									}
									catch { }
								}
								else
								{
									await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling operation.");
									try
									{
										await reply1.DeleteAsync();
									}
									catch
									{

									}
									return;
								}
							}
						}
						
						
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
		
		[Command("Rest"), Alias("Scene")]
		public async Task Rest()
		{
			var u = Utils.GetUser(Context.User.Id);

			if (u.Active == null)
			{
				await ReplyAsync(Context.User.Mention + ", you have no active character.");
				return;
			}
			var c = u.Active;

			int en = int.Parse(c.Attributes["energy"]);
			int enm = int.Parse(c.Attributes["maxenergy"]);

			if (en + 6 > enm) c.Attributes["energy"] = enm.ToString();
			else c.Attributes["energy"] = (en + 6).ToString();

			foreach (var i in c.Inventory.Where(x => x.Type == "usable"))
			{
				int index = c.Inventory.IndexOf(i);
				c.Inventory[index].Used = 0;
			}
			Utils.UpdateCharacter(c);
			await ReplyAsync(Context.User.Mention + ", " + c.Name + " has started a new Scene. Energy refilled by 6 and all usable items have recharged.");
		}
		
		private string ParseResult(RollResult result)
		{
			var sb = new StringBuilder();

			foreach (var dice in result.Values)
			{
				switch (dice.DieType)
				{
					case DieType.Normal:
						switch (dice.NumSides)
						{
							case 4:
								sb.Append(Icons.d4[(int)dice.Value] + " ");
								break;
							case 6:
								sb.Append(Icons.d6[(int)dice.Value] + " ");
								break;
							case 8:
								sb.Append(Icons.d8[(int)dice.Value] + " ");
								break;
							case 10:
								sb.Append(Icons.d10[(int)dice.Value] + " ");
								break;
							case 12:
								sb.Append(Icons.d12[(int)dice.Value] + " ");
								break;
							case 20:
								sb.Append(Icons.d20[(int)dice.Value] + " ");
								break;
							default:
								sb.Append(dice.Value);
								break;
						}
						break;
					case DieType.Special:
						switch ((SpecialDie)dice.Value)
						{
							case SpecialDie.Add:
								sb.Append("+ ");
								break;
							case SpecialDie.CloseParen:
								sb.Append(") ");
								break;
							case SpecialDie.Comma:
								sb.Append(", ");
								break;
							case SpecialDie.Divide:
								sb.Append("/ ");
								break;
							case SpecialDie.Multiply:
								sb.Append("* ");
								break;
							case SpecialDie.Negate:
								sb.Append("- ");
								break;
							case SpecialDie.OpenParen:
								sb.Append(") ");
								break;
							case SpecialDie.Subtract:
								sb.Append("- ");
								break;
							case SpecialDie.Text:
								sb.Append(dice.Data);
								break;
						}
						break;
					default:
						sb.Append(dice.Value + " ");
						break;
				}
			}

			return sb.ToString().Trim();
		}
		private string[] Disciplines = { "combat", "manipulate", "social", "exploration", "survival" };
	}
}
