using Anubis.Models;
using Anubis.Services;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

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

			var server = col.FindOne(x => x.Id == Context.Guild.Id);

			server.Prefix = Prefix[0].ToString();
			col.Update(server);
			await ReplyAsync(Context.User.Mention + ", changed prefix to `" + Prefix[0] + "`.");
		}
		[Command("Help")]
		public async Task Help()
		{
			var embed = new EmbedBuilder()
				.WithTitle("Command List");
			var sb = new StringBuilder();

			sb.AppendLine("**Create** <Name> - Creates a new character.");
			sb.AppendLine("**Character** <Name> - Switches your Active Character.");
			sb.AppendLine("**Character** - Views your active character's sheet.");
			sb.AppendLine("**Characters** - List all of your characters.");
			sb.AppendLine("**Rename** <Name> <New Name> - Renames a character.");
			sb.AppendLine("**Delete** <Name> - Deletes a character sheet.");

			embed.AddField("Sheet Management", sb.ToString());
			sb.Clear();

			sb.AppendLine("**Set** <Attribute> <Value> - Sents an attribute of your sheet to something. See `Help Attributes` to see all attributes.");
			sb.AppendLine("**Talents** - Display all of your talents and their descriptions.");
			sb.AppendLine("**Talent** <Slot> <Name> - Slot a talent into one of your slots (Valid slots: Dash, Passive, 1, 2, 3 and 4).");
			sb.AppendLine("**Item** <Command> <Name> - Add, Remove, Use, Equip or consume Items. (Valid commands: Add, Remove, Use and Equip).");

			embed.AddField("Character management", sb.ToString());
			sb.Clear();

			sb.AppendLine("**Health** <Value> - Increase or Decrease your Health.");
			sb.AppendLine("**Energy** <Value> - Increase or Decrease your Energy.");
			sb.AppendLine("**Dash** <Value> - Increase or Decrease your Dashes.");
			sb.AppendLine("**Woe** <Value> - Increase or Decrease your Woe.");
			sb.AppendLine("**Corruption** <Value> - Increase or Decrease your Corruption.");
			sb.AppendLine("**Bits** <Value> - Increase or Decrease your Bits.");
			sb.AppendLine("**Ingredients** <Value> - Increase or Decrease your Ingredients.");
			sb.AppendLine("**Components** <Value> - Increase or Decrease your Components.");
			sb.AppendLine("**Skill** <Name> - Perform a skill check.");
			sb.AppendLine("**Check** <Discipline> - Perform a Discipline check.");
			sb.AppendLine("*Roll** <Dice Expression> - Roll dice.");
			sb.AppendLine("**Act** <Talent> - Display and roll for a talent.");
			sb.AppendLine("**Rest** - Start a new Scene and recover 6 energy and recharge all usable items.");

			embed.AddField("Gameplay Module", sb.ToString());
			sb.Clear();

			sb.AppendLine("**Encounter** New - Create an new encounter.");
			sb.AppendLine("**Encounter** Start - Start the first turn of an encounter.");
			sb.AppendLine("**Encounter** End - End an ongoing encounter.");
			sb.AppendLine("**Encounter** - Display the battle summary + Map");
			sb.AppendLine("**Initiative** <tile> - Join a battle and place yourself in the selected tile.");
			sb.AppendLine("**AddNPC** <Initiative> <Tile> <Token Image Url> <Name> - Adds an NPC to the encounter in the selected tile.");
			sb.AppendLine("**Remove** <Name> - Remove a participant from the encounter.");
			sb.AppendLine("**Next** - Advance initiative and ping the next person in line.");
			sb.AppendLine("**Move** <Tile> - Move your character to a different tile.");
			sb.AppendLine("**Move** <Tile> <Name> - Move a participant to a different tile.");
			sb.AppendLine("\nYou can use the `Help combat` command to see more info about combat.");
			embed.AddField("Combat Module", sb.ToString());
			sb.Clear();

			sb.AppendLine("**Prefix** <Prefix> - Changes the prefix for the bot on this server. Only people with the Manage Channel permission can use this command.");
			sb.AppendLine("**Help** - Displays this message");
			sb.AppendLine("**Help** <topic> - Displays help for specific topics");
			embed.AddField("Misc Commands", sb.ToString());

			await ReplyAsync(Context.User.Mention + ", Here are all commands available.", false, embed.Build());
		}
		[Command("Help")]
		public async Task Help([Remainder] string Topic)
		{
			var embed = new EmbedBuilder();
			switch (Topic.ToLower())
			{
				case "attributes":
					embed.WithDescription("The **Set** command allows you to set various attributes of your character sheet. Here are a list of all attributes you can change. Type these attribute as they are when you use the **Set** command.");
					embed.AddField("Attributes", "`species`, `knack`, `trait`, `advancement`, `health`, `maxhealth`, `energy`, `maxenergy`,`dash`, `maxdash`, `woe`, `corruption`, `combat`, `combat_judgement`, `exploration`, `exploration_judgement`, `survival`, `survival_judgement`, `social`, `social_judgement`, `manipulate`, `manipulate_judgement`, `class`");
					embed.AddField("Class", "Changing the **Class** attribute will set your class features as well as change your disciplines, max energy, max HP, grit and Nerve.");
					await ReplyAsync(Context.User.Mention + ", Here's more info on this topic.", false, embed.Build());
					return;
				case "combat":
					embed.WithDescription("The observer can stage encounters for your players. First, you must create the encounter using the `Encounter Create` command. This will make you the encounter's director. Once you're ready to start the encounter, use the `Encounter Start` command to ping the first person in initiative and begin the battle.");
					embed.AddField("Joining in as a player", "As a player, you can join an ongoing encounter by using the `Initiative <Tile>` command. The bot will roll initiative for you and place you in the battle in the tile you selected.");
					embed.AddField("Adding NPCs as the director", "As the director, you can add NPCs to the encounter by using the `AddNPC <Initiative> <Tile> <Token image URL> <Name>`. You *must* supply a token image in order to add an NPC. If you add two NPCs with the same name, the bot will overwritte the existing entry with the new one.");
					embed.AddField("Playing", "Once comabt starts, the bot will ping the first person in initiative. The person whose turn it is (or the director) can use the `Next` command to end their turn and ping the next person in initiative.");
					embed.AddField("Moving around", "You can move your character by using the `Move <Tile>` command. The Director can use the `Move <Tile> <Name>` command to move any participant to any tile.");
					await ReplyAsync(Context.User.Mention + ", Here's more info on this topic.", false, embed.Build());
					return;
			}
		}
	}
}
