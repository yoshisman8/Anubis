using Antlr4.Runtime.Tree.Xpath;
using Anubis.Models;
using Discord;
using Discord.WebSocket;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Processors;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Anubis.Services
{
	public static class Helpers
	{
		public static bool IsImageUrl(this string URL)
		{
			try
			{
				var req = (HttpWebRequest)HttpWebRequest.Create(URL);
				req.Method = "HEAD";
				using (var resp = req.GetResponse())
				{
					return resp.ContentType.ToLower(CultureInfo.InvariantCulture)
							.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
				}
			}
			catch
			{
				return false;
			}
		}
		public static bool NullorEmpty(this string _string)
		{
			if (_string == null) return true;
			if (_string == "") return true;
			else return false;
		}
		public static SocketTextChannel GetTextChannelByName(this SocketGuild Guild, string Name)
		{
			var results = Guild.TextChannels.Where(x => x.Name.ToLower() == Name.ToLower());
			if (results == null || results.Count() == 0) return null;
			else return results.FirstOrDefault();
		}
	}
	public class Utilities
	{
		private LiteDatabase Database { get; set; }
		public Regex CostRegex = new Regex(@"(\d+\s*((?i)\benergy\b|(?i)\bwoe\b|(?i)\bdash\b|(?i)\bhealth\b|(?i)\bingredient\b|(?i)\bcomponent\b|(?i)\bbit\b))");

		public Utilities(LiteDatabase _database)
		{
			Database = _database;
		}
		/// <summary>
		/// Gets an user's database entry.
		/// </summary>
		/// <param name="Id">User's discord ulong ID</param>
		/// <returns>The user file</returns>
		public User GetUser(ulong Id)
		{
			var col = Database.GetCollection<User>("Users");

			if(col.Exists(x=>x.Id == Id))
			{
				return col.Include(x=>x.Active)
					.Include(x=>x.Subscriptions)
					.FindOne(x => x.Id == Id);
			}
			else
			{
				var user = new User() { Id = Id };
				col.Insert(user);
				return user;
			}
		}

		/// <summary>
		/// Gets an User's active character
		/// </summary>
		/// <param name="Id">The user's Discord ID</param>
		/// <returns>The Active Character file, Or Null if none is set</returns>
		public Character GetCharacter(ulong Id)
		{
			var user = GetUser(Id);

			return user.Active;
		}
		public List<Character> GetAllCharacters(ulong id)
		{
			var col = Database.GetCollection<Character>("Characters");

			var all = col.Find(x => x.Owner == id).OrderBy(x=>x.Name).ToList();
			if (all.Count == 0) return null;
			else return all;
		}
		public void DeleteCharacter(Character ch)
		{
			var col = Database.GetCollection<Character>("Characters");

			col.Delete(ch.Id);
		}
		public void UpdateCharacter(Character ch)
		{
			var col = Database.GetCollection<Character>("Characters");
			col.Update(ch);
		}
		public void UpdateUser(User U)
		{
			var col = Database.GetCollection<User>("Users");
			col.Update(U);

		}
		public Battle GetBattle(ulong channel)
		{
			var col = Database.GetCollection<Battle>("Battles");
			if(!col.Exists(x=>x.ChannelId == channel))
			{
				col.Insert(new Battle()
				{
					ChannelId = channel
				});
			}
			var battle = col.FindOne(x => x.ChannelId == channel);

			return battle;
		}
		public Character FindCharacter(int Id)
		{
			var col = Database.GetCollection<Character>("Characters");
			return col.FindOne(x => x.Id == Id);
		}
		public void UpdateBattle(Battle b)
		{
			var col = Database.GetCollection<Battle>("Battles");
			col.Update(b);
		}

		#region Renders
		public Embed RenderSheet(Character character)
		{
			var embed = new EmbedBuilder()
				.WithTitle(character.Name+(character.Attributes["class"].NullorEmpty()?"":" the "+ character.Attributes["class"]))
				.WithThumbnailUrl(character.Attributes["image"]);
			var sb = new StringBuilder();

			sb.AppendLine("Health [" + character.Attributes["health"] + "/" + character.Attributes["maxhealth"] + "]");
			sb.AppendLine("Energy [" + character.Attributes["energy"] + "/" + character.Attributes["maxenergy"] + "]");
			sb.AppendLine("Dashes [" + character.Attributes["dash"] + "/" + character.Attributes["maxdash"] + "]");
			sb.AppendLine("Woe [" + character.Attributes["woe"] + "/9]");
			sb.AppendLine("Corruption [" + character.Attributes["corruption"] + "/13]");
			embed.AddField("Vitals", sb.ToString(), true);
			sb.Clear();

			sb.AppendLine("Grit [" + character.Attributes["grit"] + "] | [" + character.Attributes["nerve"] + "] Nerve");
			sb.AppendLine("Vigor [" + character.Attributes["vigor"] + "]");
			sb.AppendLine("Agility [" + character.Attributes["agility"] + "]");
			sb.AppendLine("Insight [" + character.Attributes["insight"] + "]");
			sb.AppendLine("Presence [" + character.Attributes["presence"] + "]");
			embed.AddField("Abilities", sb.ToString(), true);
			sb.Clear();

			sb.AppendLine(Icons.SheetIcons["exploration"] + " [" + character.Attributes["exploration_judgement"] + "] [" + character.Attributes["exploration"] + "]");
			sb.AppendLine(Icons.SheetIcons["survival"] + " [" + character.Attributes["survival_judgement"] + "] [" + character.Attributes["survival"] + "]");
			sb.AppendLine(Icons.SheetIcons["combat"] + " [" + character.Attributes["combat_judgement"] + "] [" + character.Attributes["combat"] + "]");
			sb.AppendLine(Icons.SheetIcons["social"] + " [" + character.Attributes["social_judgement"] + "] [" + character.Attributes["social"] + "]");
			sb.AppendLine(Icons.SheetIcons["manipulate"] + " [" + character.Attributes["manipulate_judgement"] + "] [" + character.Attributes["manipulate"] + "]");
			embed.AddField("Disciplines", sb.ToString(), true);
			sb.Clear();

			foreach (var cf in character.Features)
			{
				sb.AppendLine("**" + cf.Name + "**");
				sb.AppendLine(cf.Description);
			}
			embed.AddField("Class Features", sb.Length == 0 ? "None" : sb.ToString());
			sb.Clear();

			sb.AppendLine("Species: " + character.Attributes["species"]);
			sb.AppendLine("Trait: " + character.Attributes["trait"]);
			sb.AppendLine("Knack: " + character.Attributes["knack"]);
			sb.AppendLine("Advancement: " + character.Attributes["advancement"]);
			embed.AddField("Persona", sb.ToString(),true);
			sb.Clear();

			sb.AppendLine(Icons.SheetIcons["passive"]+" " + (character.Passive == null ? "None" : character.Passive.Name));
			sb.AppendLine(Icons.SheetIcons["dash"] + " " + (character.Dash == null ? "None" : character.Dash.Name));
			sb.AppendLine(Icons.SheetIcons["slot1"] + " " + (character.Slot1== null ? "None" : character.Slot1.Name));
			sb.AppendLine(Icons.SheetIcons["slot2"] + " " + (character.Slot2== null ? "None" : character.Slot2.Name));
			sb.AppendLine(Icons.SheetIcons["slot3"] + " " + (character.Slot3== null ? "None" : character.Slot3.Name));
			sb.AppendLine(Icons.SheetIcons["slot4"] + " " + (character.Slot4 == null ? "None" : character.Slot4.Name));

			embed.AddField("Talents", sb.ToString(),true);
			sb.Clear();

			var inv = ParseInventory(character.Inventory);

			sb.AppendLine(Icons.SheetIcons["Gearbit"] + " " + character.Attributes["bits"] + " | " + Icons.SheetIcons["Ingredient"] + " " + character.Attributes["ingredients"] + " | " + Icons.SheetIcons["Component"] + " " + character.Attributes["components"]);
			foreach (var i in inv)
			{
				sb.AppendLine("• " + RenderItemName(i.Key) + (i.Value > 1 ? " x" + i.Value : ""));
			}
			embed.AddField("Inventory", sb.ToString(),true);

			return embed.Build();
		}
		public Embed RenderBattle(Battle b, bool refreshmap = false)
		{
			var embed = new EmbedBuilder()
				.WithTitle("Battle Summary")
				.WithDescription("Round: " + b.Round);
			var sb = new StringBuilder();
			foreach (var c in b.Participants)
			{
				if (c == b.Current)
				{
					sb.AppendLine("[" + c.Initiative + "] **" + c.Name + "**");
					if(c.Type == ParticipantType.Player)
					{
						var character = FindCharacter(c.Id);
						sb.AppendLine("> Health: [" + character.Attributes["health"] + "/" + character.Attributes["maxhealth"] + "]");
						sb.AppendLine("> Energy: [" + character.Attributes["energy"] + "/" + character.Attributes["maxenergy"] + "]");
					}
				}
				else
				{
					sb.AppendLine("[" + c.Initiative + "] " + c.Name + "");

				}
			}
			embed.AddField("Initiative", sb.ToString());
			if (refreshmap)
			{
				RenderBattleMap(b);
				embed.WithImageUrl($"attachment://battlemap-{b.ChannelId}.png");
			}
			else
			{
				embed.WithImageUrl($"attachment://battlemap-{b.ChannelId}.png");
			}
			return embed.Build();
		}

		public void RenderBattleMap(Battle b)
		{
			byte[] Battlemap = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "data", "battlemap.png"));

			using (MemoryStream inStream = new MemoryStream(Battlemap))
			{
				using (MemoryStream outStream = new MemoryStream())
				{
					using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
					{
						imageFactory.Load(inStream);

						for(int i = 0; i < 9; i++)
						{
							for(int j = 0; j < Math.Min(5, b.Battlemap[i].Count); j++)
							{
								imageFactory.Overlay(new ImageLayer()
								{
									Image = PrepareToken(b.Battlemap[i][j].Token),
									Opacity = 100,
									Position = GetPoint(i,j)
								});
							}
						}
						imageFactory.Format(new PngFormat { Quality = 100 });
						imageFactory.Save(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", "battlemap-" + b.ChannelId + ".png"));
						imageFactory.Dispose();
					}
				}
			}
		}
		public System.Drawing.Image PrepareToken(string imageurl)
		{
			var img = DownloadImageFromUrl(imageurl);
			var imageFactory = new ImageFactory(true);

			imageFactory.Load(img);

			imageFactory.Resize(new ResizeLayer(new Size(125, 150), ResizeMode.Min));
			return imageFactory.Image;
		}
		public System.Drawing.Image DownloadImageFromUrl(string imageUrl)
		{
			System.Drawing.Image image = null;

			try
			{
				System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(imageUrl);
				webRequest.AllowWriteStreamBuffering = true;
				webRequest.Timeout = 30000;

				System.Net.WebResponse webResponse = webRequest.GetResponse();

				System.IO.Stream stream = webResponse.GetResponseStream();

				image = System.Drawing.Image.FromStream(stream);

				webResponse.Close();
			}
			catch (Exception ex)
			{
				return null;
			}

			return image;
		}
		public string RenderItemName(Item item)
		{
			switch (item.Type)
			{
				case "armor":
					return item.Name + " " + (item.Use ? "[E]" : "");
				case "weapon":
					return item.Name + " " + (item.Use ? "[W]" : "");
				case "shield":
					return item.Name + " " + (item.Use ? "[W]" : "");
				case "usable":
					return	item.Name + " " + "[" + (int.Parse(item.Frequency)-item.Used) + "/"+item.Frequency+"]";
				default:
					return item.Name;
			}
		}
		public string RenderTalent(Talent talent)
		{
			var sb = new StringBuilder();

			sb.AppendLine(Icons.SheetIcons[talent.Discipline]+" "+talent.Name);
			sb.Append("("+talent.Cost);
			if (talent.Skill != "none") sb.Append(" | "+talent.Skill);
			if (talent.Range != "-") sb.Append(" | "+"range " + talent.Range);
			sb.Append(")");
			sb.Append("\n");
			sb.AppendLine(talent.Description);
			return sb.ToString();
		}
		public string RenderPassive(Passive talent)
		{
			var sb = new StringBuilder();

			sb.AppendLine(Icons.SheetIcons[talent.Discipline] + " " + talent.Name);
			sb.Append("\n");
			sb.AppendLine(talent.Description);
			return sb.ToString();
		}
		public string RenderDash(Dash talent)
		{
			var sb = new StringBuilder();

			sb.AppendLine(talent.Name);
			sb.Append("(" + talent.Type);
			if (talent.Skill != "none") sb.Append(" | "+talent.Skill);
			if (talent.Range != "-") sb.Append(" | range " + talent.Range);
			sb.Append(")");
			sb.Append("\n");
			sb.AppendLine(talent.Description);
			return sb.ToString();
		}
		public string RenderAction(Models.Action action)
		{
			var sb = new StringBuilder();
			sb.AppendLine(action.Name);
			sb.Append("(" + action.Type+" | " + action.Cost + " | ");
			if (action.Skill != "none") sb.Append(" | " + action.Skill);
			if (action.Range != "-") sb.Append(" | range " + action.Range);
			sb.Append(")");
			sb.Append("\n");
			sb.AppendLine(action.Description);
			return sb.ToString();
		}
		public Dictionary<Item, int> ParseInventory(List<Item> Inventory)
		{
			var final = Inventory.GroupBy(x => x).ToDictionary(y => y.Key,z=> z.Count());
			return final;
		}
		#endregion
		public string GetPrefix(ulong guild)
		{
			var col = Database.GetCollection<Server>("Servers");

			var g = col.FindOne(x => x.Id == guild);

			return g.Prefix;
		}
		#region Queries
		public Talent[] QueryTalent(string Name, User user)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Talents;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Talents.Count() > 0) talents.AddRange(cp.Talents);
			}
			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		public GameClass[] QueryClass(string Name, User user)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Classes;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Classes.Count() > 0) talents.AddRange(cp.Classes);
			}
			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		public Dash[] QueryDash(string Name, User user)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Dashes;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Dashes.Count() > 0) talents.AddRange(cp.Dashes);
			}
			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		public Passive[] QueryPassive(string Name, User user)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Passives;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Passives.Count() > 0) talents.AddRange(cp.Passives);
			}
			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		public Item[] QueryItem(string Name, User user)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Items;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Items.Count() > 0) talents.AddRange(cp.Items);
			}
			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		public Models.Action[] QueryActions(string Name, User user, Character character)
		{
			var col = Database.GetCollection<ContentPack>("ContentPacks");
			var CRB = col.FindOne(x => x.Id == 1);
			var talents = CRB.Actions;
			foreach (var cp in user.Subscriptions)
			{
				if (cp.Actions.Count() > 0) talents.AddRange(cp.Actions);
			}
			if (character.Dash != null) talents.Add(character.Dash);
			if (character.Slot1 != null) talents.Add(character.Slot1);
			if (character.Slot2 != null) talents.Add(character.Slot2);
			if (character.Slot3 != null) talents.Add(character.Slot3);
			if (character.Slot4 != null) talents.Add(character.Slot4);

			var results = talents.Where(x => x.Name.ToLower().StartsWith(Name.ToLower())).ToArray();
			return results;
		}
		#endregion


		private int horizontalDistance = 200;
		private int verticalDistance = 115;
		private int horizontalTileDistance = 280;
		private int verticalTileDistance = 175;

		private Point GetPoint(int tile, int subtile)
		{
			Point O = new Point(857, 12);

			switch (tile)
			{
				case 0:
					break;
				case 1:
					O = new Point(O.X - horizontalTileDistance, O.Y + verticalTileDistance);
					break;
				case 2:
					O = new Point(O.X + horizontalTileDistance, O.Y + verticalTileDistance);
					break;
				case 3:
					O = new Point(O.X - (horizontalTileDistance *2), O.Y + (verticalTileDistance * 2));
					break;
				case 4:
					O = new Point(O.X, O.Y + verticalTileDistance *2);
					break;
				case 5:
					O = new Point(O.X + (horizontalTileDistance * 2), O.Y + (verticalTileDistance * 2));
					break;
				case 6:
					O = new Point(O.X - horizontalTileDistance, O.Y + (verticalTileDistance * 3));
					break;
				case 7:
					O = new Point(O.X + horizontalTileDistance, O.Y + (verticalTileDistance * 3));
					break;
				case 8:
					O = new Point(O.X, O.Y + (verticalTileDistance * 4));
					break;
				
			}


			switch (subtile)
			{
				case 0:
					return O;
				case 1:
					return new Point(O.X - horizontalDistance, O.Y + verticalDistance);
				case 2:
					return new Point(O.X, O.Y + verticalDistance);
				case 3:
					return new Point(O.X + horizontalDistance, O.Y + verticalDistance);
				case 4:
					return new Point(O.X, O.Y + (verticalDistance *2));
			}
			return O;
		}

	}
	public static class Icons
	{
		public static Dictionary<int, string> d20 { get; set; } = new Dictionary<int, string>()
		{
			{20, "<:d20_20:663149799792705557>" },
			{19, "<:d20_19:663149782847586304>" },
			{18, "<:d20_18:663149770621190145>" },
			{17, "<:d20_17:663149758885396502>" },
			{16, "<:d20_16:663149470216749107>" },
			{15, "<:d20_15:663149458963300352>" },
			{14, "<:d20_14:663149447278100500>" },
			{13, "<:d20_13:663149437459234846>" },
			{12, "<:d20_12:663149424909746207>" },
			{11, "<:d20_11:663149398712123415>" },
			{10, "<:d20_10:663149389396574212>" },
			{9, "<:d20_9:663149377954775076>" },
			{8, "<:d20_8:663149293695139840>" },
			{7, "<:d20_7:663149292743032852>" },
			{6, "<:d20_6:663149290532634635>" },
			{5, "<:d20_5:663147362608480276>" },
			{4, "<:d20_4:663147362512011305>" },
			{3, "<:d20_3:663147362067415041>" },
			{2, "<:d20_2:663147361954037825>" },
			{1, "<:d20_1:663146691016523779>" }
		};
		public static Dictionary<int, string> d12 { get; set; } = new Dictionary<int, string>()
		{
			{12, "<:d12_12:663152540426174484>" },
			{11, "<:d12_11:663152540472442900>" },
			{10, "<:d12_10:663152540439019527>" },
			{9, "<:d12_9:663152540199682061>" },
			{8, "<:d12_8:663152540459728947>" },
			{7, "<:d12_7:663152540116058133>" },
			{6, "<:d12_6:663152540484894740>" },
			{5, "<:d12_5:663152540250144804>" },
			{4, "<:d12_4:663152540426305546>" },
			{3, "<:d12_3:663152540161933326>" },
			{2, "<:d12_2:663152538291404821>" },
			{1, "<:d12_1:663152538396393482>" }
		};
		public static Dictionary<int, string> d10 { get; set; } = new Dictionary<int, string>()
		{
			{10, "<:d10_10:663158741352579122>" },
			{9, "<:d10_9:663158741331476480>" },
			{8, "<:d10_8:663158741079687189>" },
			{7, "<:d10_7:663158742636036138>" },
			{6, "<:d10_6:663158741121761280>" },
			{5, "<:d10_5:663158740576632843>" },
			{4, "<:d10_4:663158740685553713>" },
			{3, "<:d10_3:663158740442415175>" },
			{2, "<:d10_2:663158740496810011>" },
			{1, "<:d10_1:663158740463255592>" }
		};
		public static Dictionary<int, string> d8 { get; set; } = new Dictionary<int, string>()
		{
			{8, "<:d8_8:663158785795162112>" },
			{7, "<:d8_7:663158785841561629>" },
			{6, "<:d8_6:663158785774190595>" },
			{5, "<:d8_5:663158785271005185>" },
			{4, "<:d8_4:663158785107296286>" },
			{3, "<:d8_3:663158785543503920>" },
			{2, "<:d8_2:663158785224867880>" },
			{1, "<:d8_1:663158784859963473>" }
		};
		public static Dictionary<int, string> d6 { get; set; } = new Dictionary<int, string>()
		{
			{6, "<:d6_6:663158852551835678>" },
			{5, "<:d6_5:663158852136599564>" },
			{4, "<:d6_4:663158856247148566>" },
			{3, "<:d6_3:663158852358766632>" },
			{2, "<:d6_2:663158852354834452>" },
			{1, "<:d6_1:663158852354572309>" }
		};
		public static Dictionary<int, string> d4 { get; set; } = new Dictionary<int, string>()
		{
			{4, "<:d4_4:663158852472274944>" },
			{3, "<:d4_3:663158852178411560>" },
			{2, "<:d4_2:663158851734077462>" },
			{1, "<:d4_1:663158851909976085>" }
		};
		public static Dictionary<string, string> SheetIcons { get; set; } = new Dictionary<string, string>()
		{
			{"exploration", "<:explore:722466457375735889>" },
			{"combat", "<:combat:722466457501564968>" },
			{"survival", "<:survival:722466457740640296>" },
			{"social", "<:social:722466457446907996>" },
			{"manipulate", "<:manipulate:722466457581256724>" },
			{"Gearbit" ,"<:Gearbits:722835895321100330>"},
			{"Component","<:component:722835895488741466>" },
			{"Ingredient","<:ingredients:722835895253860364>" },
			{"passive","<:Passive_Talent:723536829734846515>" },
			{"dash", "<:Dash_Talent:723536830053482507>" },
			{"slot1","<:Talent_1:723536830019928076>" },
			{"slot2","<:Talent_2:723536829764337725>" },
			{"slot3","<:Talent_3:723536830149951618>" },
			{"slot4","<:Talent_4:723536830183637093>" }
		};
	}
}
