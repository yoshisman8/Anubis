using Anubis.Models;
using Anubis.Services;
using Dice;
using Discord;
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

			if (Constants.Skills.TryGetValue(skill.ToLower(), out Skill value))
			{
				var dice = Roller.Roll("1d20");
				var embed = new EmbedBuilder()
					.WithTitle(c.Name+" makes a "+skill+" check.")
					.WithDescription(ParseResult(dice)+" = `"+dice.Value+"`");
				var fortune = int.Parse(c.Attributes[value.Discipline]);
				var judgement = int.Parse(c.Attributes[value.Discipline + "_judgement"]);
				
				if(dice.Value <= judgement)
				{
					embed.WithColor(Color.Red);
				}
				else if(dice.Value >= fortune)
				{
					embed.WithColor(Color.Green);
				}
				else
				{
					embed.WithColor(new Color(255, 255, 0));
				}
				await ReplyAsync(" ", false, embed.Build());
			}
			else
			{
				await ReplyAsync(Context.User.Mention + ", This isn't a valid skill.");
				return;
			}
		}
		[Command("Dash")]
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
	}
}
