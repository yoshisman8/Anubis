using Anubis.Models;
using Anubis.Services;
using Discord.Addons.Interactive;
using Discord.Commands;
using LiteDB;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
using static Anubis.Models.Constants;
using System.IO.Pipes;

namespace Anubis.Modules
{
	public class Contept : InteractiveBase<SocketCommandContext>
	{
		public Utilities Utils { get; set; }
		public LiteDatabase Database { get; set; }

		[Command("Upload")]
		public async Task UploadHomebrew()
		{
			if (Context.Message.Attachments.Count == 0)
			{
				await ReplyAsync("No json was attached.");
				return;
			}

			var file = Context.Message.Attachments.FirstOrDefault();

			WebClient client = new WebClient();
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp"));
			string raw = client.DownloadString(file.Url);

			try
			{
				JObject json = JObject.Parse(raw);

				if (json["metadata"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file has no metadata. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["name"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no name. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["tag"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no tag. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["version"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no version number. Fix this error and send the file again.");
					return;
				}

				var col = Database.GetCollection<ContentPack>("ContentPacks");

				if (!col.Exists(x => x.Name == (string)json["metadata"]["name"] && x.Tag == (string)json["metadata"]["tag"] && x.Author == Context.User.Id))
				{
					var pack = new ContentPack()
					{
						Name = (string)json["metadata"]["name"],
						Tag = (string)json["metadata"]["tag"],
						Author = Context.User.Id
					};
					col.Insert(pack);
				}
				var p = col.FindOne(x => x.Name == (string)json["metadata"]["name"] && x.Tag == (string)json["metadata"]["tag"] && x.Author == Context.User.Id);


				

				if (json["classes"] != null && json["classes"].HasValues)
				{
					var classes = json["classes"];
					var parsedclasses = new List<GameClass>();
					foreach (var c in classes)
					{
						var gc = new GameClass();
						if (((string)c["name"]).NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A class with no name was detected. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						gc.Name = (string)c["name"];

						if (c["attributes"].HasValues)
						{
							foreach (var a in c["attributes"])
							{
								if (a["name"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + " has an attribute with no name. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (a["value"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", attribute " + a["name"] + " has no value. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (!Constants.DefaultAttributes.ContainsKey((string)a["name"]))
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", attribute " + a["name"] + " is not a valid attribute (make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								gc.Attributes.Add((string)a["name"], (string)a["value"]);
							}
						}
						if (c["features"].HasValues)
						{
							foreach (var f in c["features"])
							{
								if (f["name"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + " has an class feature with no name. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (f["description"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", class feature " + f["name"] + " has an empty description. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								gc.Features.Add(new Feature() { Name = f["name"].ToString(), Description = f["description"].ToString() });
							}
						}
						parsedclasses.Add(gc);
					}
					p.Classes = parsedclasses;
				}
				if (json["talents"] != null && json["talents"].HasValues)
				{
					var parsedtalents = new List<Talent>();
					foreach (var t in json["talents"])
					{
						if (t["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no cost (if the cost is 0, set the cost to '0 energy'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Utils.CostRegex.IsMatch(t["cost"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has an invalid cost (If this talent has multiple costs, make sure to separate them with a comma). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["discipline"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no discipline. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Enum.TryParse<Disciplines>(t["discipline"].ToString(), out Disciplines result))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has an invalid discipline (make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in t["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (t["skill"].ToString().Split(",").Length == 1 && t["skill"].ToString() != "none" && t["skill"].ToString() != "any" && !Skills.ContainsKey(t["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", Skill " + t["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["range"].ToString() != "-" && !int.TryParse(t["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", " + t["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedtalents.Add(new Talent()
						{
							Name = t["name"].ToString(),
							Description = t["description"].ToString(),
							Cost = t["cost"].ToString(),
							Discipline = t["discipline"].ToString(),
							Skill = t["skill"].ToString(),
							Range = t["range"].ToString()
						});
					}
					p.Talents = parsedtalents;
				}
				if (json["passives"] != null && json["passives"].HasValues)
				{
					var parsedpassives = new List<Passive>();
					foreach (var pa in json["passives"])
					{
						if (pa["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", a passive talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (pa["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", passive talent " + pa["name"] + " has an empty description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedpassives.Add(new Passive()
						{
							Name = pa["name"].ToString(),
							Description = pa["description"].ToString()
						});
					}
					p.Passives = parsedpassives;
				}
				if (json["dashes"] != null && json["dashes"].HasValues)
				{
					var parseddashes = new List<Dash>();
					foreach (var d in json["dashes"])
					{
						if (d["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A dash talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in d["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (d["skill"].ToString().Split(",").Length == 1 && d["skill"].ToString() != "none" && d["skill"].ToString() != "any" && !Skills.ContainsKey(d["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", Skill " + d["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["type"].ToString() != "action" && d["type"].ToString() != "reaction")
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["range"].ToString() != "-" && !int.TryParse(d["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", " + d["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parseddashes.Add(new Dash()
						{
							Name = d["name"].ToString(),
							Description = d["description"].ToString(),
							Skill = d["skill"].ToString(),
							Type = d["type"].ToString(),
							Range = d["range"].ToString()
						});
					}
					p.Dashes = parseddashes;
				}
				if (json["actions"] != null && json["actions"].HasValues)
				{
					var parsedactions = new List<Models.Action>();
					foreach (var a in json["actions"])
					{
						if (a["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", an action has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in a["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (a["skill"].ToString().Split(",").Length == 1 && a["skill"].ToString() != "none" && a["skill"].ToString() != "any" && !Skills.ContainsKey(a["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", Skill " + a["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["type"].ToString() != "action" && a["type"].ToString() != "reaction")
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["range"].ToString() != "-" && !int.TryParse(a["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", " + a["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no cost (if the cost is 0, set the cost to '0 energy'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Utils.CostRegex.IsMatch(a["cost"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has an invalid cost (If this action has multiple costs, make sure to separate them with a comma). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedactions.Add(new Models.Action()
						{
							Name = a["name"].ToString(),
							Description = a["description"].ToString(),
							Cost = a["cost"].ToString(),
							Range = a["range"].ToString(),
							Skill = a["skill"].ToString(),
							Type = a["type"].ToString()
						});
					}
					p.Actions = parsedactions;
				}
				if (json["items"] != null && json["items"].HasValues)
				{
					var parseditems = new List<Item>();
					foreach (var i in json["items"])
					{
						if (i["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", an item has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (i["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no type (Valid types: weapon, armor, shield, usable, consumable, upgrade). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (i["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no cost (If the item has no cost, put something like '-' or '0 bits' on this field). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (Enum.TryParse(i["type"].ToString(), out ItemTypes type))
						{
							switch (type)
							{
								case ItemTypes.armor:
									if (i["armor"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no armor value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["armor"].ToString(), out int arm))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric armor value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["agility"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no increased agility energy cost (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["agility"].ToString(), out int agi))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric increased agility energy cost value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["threshold"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no increased exploration threshold (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["threshold"].ToString(), out int thre))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric increased exploration threshold value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Armor = i["armor"].ToString(),
										Agility = i["agility"].ToString(),
										Threshold = i["threshold"].ToString()
									});
									break;
								case ItemTypes.shield:
									if (i["hands"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no handedness value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["hands"].ToString() != "2" && i["hands"].ToString() != "1" && i["hands"].ToString() != "0")
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid handedness value (Valid values: 0,1 and 2). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["defense"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no defense value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["defense"].ToString(), out int def))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric defense value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["attack"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no attack penalty value (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["attack"].ToString(), out int att))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric attack penalty value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Hands = i["hands"].ToString(),
										Defense = i["defense"].ToString(),
										Attack = i["attack"].ToString()
									});
									break;
								case ItemTypes.weapon:
									if (i["hands"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no handedness value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["hands"].ToString() != "2" && i["hands"].ToString() != "1" && i["hands"].ToString() != "0")
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid handedness value (Valid values: 0,1 and 2). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["damage"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no damage value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["damage"].ToString(), out int dmg))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric damage value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["range"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no range value (if 0, set this field to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["range"].ToString(), out int rng))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric range value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["energy"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no additional energy cost value (if 0, set this field to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["energy"].ToString(), out int eng))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric additional energy cost value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Hands = i["hands"].ToString(),
										Damage = i["damage"].ToString(),
										Range = i["range"].ToString(),
										Energy = i["energy"].ToString()
									});
									break;
								case ItemTypes.usable:
									if (i["frequency"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no per scene frequency. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["frequency"].ToString(), out int freq))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric per scene frequency value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString(),
										Frequency = i["frequency"].ToString()
									});
									break;
								case ItemTypes.upgrade:
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString()
									});
									break;
								case ItemTypes.consumable:
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString()
									});
									break;
							}

						}
						else
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid type (Valid types: weapon, armor, shield, usable, consumable, upgrade). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
					}
					p.Items = parseditems;
				}

				p.Metadata = JsonConvert.DeserializeObject<Metadata>(json["metadata"].ToString());
				col.Update(p);
				col.EnsureIndex(x => x.Name.ToLower());
				col.EnsureIndex("ContentPack", "LOWER($.Name)");
				var user = Utils.GetUser(Context.User.Id);
				if(!user.Subscriptions.Exists(x=> x.Id == p.Id))
				{
					user.Subscriptions.Add(p);
					Utils.UpdateUser(user);
				}

				await ReplyAsync(Context.User.Mention + ", Successfully uploaded version " + p.Metadata.version + " of content pack " + p.Metadata.name + ".");
			
			
			}
			catch
			{
				await ReplyAsync(Context.User.Mention + ", Something is wrong with this JSON file. Please check your file on this website to ensure it's correct: https://jsonformatter.curiousconcept.com/.");
			}

		}
		[Command("CRB")] [RequireOwner]
		public async Task UploadCRB()
		{
			if (Context.Message.Attachments.Count == 0)
			{
				await ReplyAsync("No json was attached.");
				return;
			}

			var file = Context.Message.Attachments.FirstOrDefault();

			WebClient client = new WebClient();
			Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp"));
			string raw = client.DownloadString(file.Url);


			try
			{
				JObject json = JObject.Parse(raw);

				if (json["metadata"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file has no metadata. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["name"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no name. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["tag"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no tag. Fix this error and send the file again.");
					return;
				}
				if (json["metadata"]["version"] == null)
				{
					await ReplyAsync(Context.User.Mention + ", This file's metadata has no version number. Fix this error and send the file again.");
					return;
				}
				var col = Database.GetCollection<ContentPack>("ContentPacks");

				if (!col.Exists(x => x.Id == 1))
				{
					var pack = new ContentPack()
					{
						Metadata = JsonConvert.DeserializeObject<Metadata>(json["metadata"].ToString()),
						Author = Context.User.Id
					};
					col.Insert(pack);
				}
				var p = col.FindOne(x => x.Id == 1);

				if (json["classes"] != null && json["classes"].HasValues)
				{
					var classes = json["classes"];
					var parsedclasses = new List<GameClass>();
					foreach (var c in classes)
					{
						var gc = new GameClass();
						if (((string)c["name"]).NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A class with no name was detected. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						gc.Name = (string)c["name"];

						if (c["attributes"].HasValues)
						{
							foreach (var a in c["attributes"])
							{
								if (a["name"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + " has an attribute with no name. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (a["value"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", attribute " + a["name"] + " has no value. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (!Constants.DefaultAttributes.ContainsKey((string)a["name"]))
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", attribute " + a["name"] + " is not a valid attribute (make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								gc.Attributes.Add((string)a["name"], (string)a["value"]);
							}
						}
						if (c["features"].HasValues)
						{
							foreach (var f in c["features"])
							{
								if (f["name"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + " has an class feature with no name. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								if (f["description"].ToString().NullorEmpty())
								{
									await ReplyAsync(Context.User.Mention + ", Class " + gc.Name + ", class feature " + f["name"] + " has an empty description. Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
								gc.Features.Add(new Feature() { Name = f["name"].ToString(), Description = f["description"].ToString() });
							}
						}
						parsedclasses.Add(gc);
					}
					p.Classes = parsedclasses;
				}
				if (json["talents"] != null && json["talents"].HasValues)
				{
					var parsedtalents = new List<Talent>();
					foreach (var t in json["talents"])
					{
						if (t["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no cost (if the cost is 0, set the cost to '0 energy'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Utils.CostRegex.IsMatch(t["cost"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has an invalid cost (If this talent has multiple costs, make sure to separate them with a comma). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["discipline"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no discipline. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Enum.TryParse<Disciplines>(t["discipline"].ToString(), out Disciplines result))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has an invalid discipline (make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in t["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (t["skill"].ToString().Split(",").Length == 1 && t["skill"].ToString() != "none" && t["skill"].ToString() != "any" && !Skills.ContainsKey(t["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", Skill " + t["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (t["range"].ToString() != "-" && !int.TryParse(t["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + t["name"] + ", " + t["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedtalents.Add(new Talent()
						{
							Name = t["name"].ToString(),
							Description = t["description"].ToString(),
							Cost = t["cost"].ToString(),
							Discipline = t["discipline"].ToString(),
							Skill = t["skill"].ToString(),
							Range = t["range"].ToString()
						});
					}
					p.Talents = parsedtalents;
				}
				if (json["passives"] != null && json["passives"].HasValues)
				{
					var parsedpassives = new List<Passive>();
					foreach (var pa in json["passives"])
					{
						if (pa["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", a passive talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (pa["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", passive talent " + pa["name"] + " has an empty description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedpassives.Add(new Passive()
						{
							Name = pa["name"].ToString(),
							Description = pa["description"].ToString()
						});
					}
					p.Passives = parsedpassives;
				}
				if (json["dashes"] != null && json["dashes"].HasValues)
				{
					var parseddashes = new List<Dash>();
					foreach (var d in json["dashes"])
					{
						if (d["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", A dash talent has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in d["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (d["skill"].ToString().Split(",").Length == 1 && d["skill"].ToString() != "none" && d["skill"].ToString() != "any" && !Skills.ContainsKey(d["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", Skill " + d["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["type"].ToString() != "action" && d["type"].ToString() != "reaction")
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (d["range"].ToString() != "-" && !int.TryParse(d["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", talent " + d["name"] + ", " + d["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parseddashes.Add(new Dash()
						{
							Name = d["name"].ToString(),
							Description = d["description"].ToString(),
							Skill = d["skill"].ToString(),
							Type = d["type"].ToString(),
							Range = d["range"].ToString()
						});
					}
					p.Dashes = parseddashes;
				}
				if (json["actions"] != null && json["actions"].HasValues)
				{
					var parsedactions = new List<Models.Action>();
					foreach (var a in json["actions"])
					{
						if (a["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", an action has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["description"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no description. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["skill"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no skill (If no skill is used, set this field to 'none'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["skill"].ToString().Split(",").Length > 1)
						{
							foreach (var sk in a["skill"].ToString().Split(","))
							{
								if (sk != "none" && sk != "any" && !Skills.ContainsKey(sk))
								{
									await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", Skill " + sk + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
									File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
									return;
								}
							}
						}
						if (a["skill"].ToString().Split(",").Length == 1 && a["skill"].ToString() != "none" && a["skill"].ToString() != "any" && !Skills.ContainsKey(a["skill"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", Skill " + a["skill"] + " is not valid (Make sure it's all lowercase). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["type"].ToString() != "action" && a["type"].ToString() != "reaction")
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no type (Valid types are action and reaction). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["range"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no range (If range is not a part of this talent, set this field to '-'.). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["range"].ToString() != "-" && !int.TryParse(a["range"].ToString(), out int result2))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + ", " + a["range"] + " is not a valid number. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (a["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has no cost (if the cost is 0, set the cost to '0 energy'). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (!Utils.CostRegex.IsMatch(a["cost"].ToString()))
						{
							await ReplyAsync(Context.User.Mention + ", action " + a["name"] + " has an invalid cost (If this action has multiple costs, make sure to separate them with a comma). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						parsedactions.Add(new Models.Action()
						{
							Name = a["name"].ToString(),
							Description = a["description"].ToString(),
							Cost = a["cost"].ToString(),
							Range = a["range"].ToString(),
							Skill = a["skill"].ToString(),
							Type = a["type"].ToString()
						});
					}
					p.Actions = parsedactions;
				}
				if (json["items"] != null && json["items"].HasValues)
				{
					var parseditems = new List<Item>();
					foreach (var i in json["items"])
					{
						if (i["name"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", an item has no name. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (i["type"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no type. Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (i["cost"].ToString().NullorEmpty())
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no cost (If the item has no cost, put something like '-' or '0 bits' on this field). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
						if (Enum.TryParse(i["type"].ToString(), out ItemTypes type))
						{
							switch (type)
							{
								case ItemTypes.armor:
									if (i["armor"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no armor value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["armor"].ToString(), out int arm))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric armor value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["agility"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no increased agility energy cost (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["agility"].ToString(), out int agi))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric increased agility energy cost value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["threshold"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no increased exploration threshold (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["threshold"].ToString(), out int thre))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric increased exploration threshold value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Armor = i["armor"].ToString(),
										Agility = i["agility"].ToString(),
										Threshold = i["threshold"].ToString()
									});
									break;
								case ItemTypes.shield:
									if (i["hands"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no handedness value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["hands"].ToString() != "2" && i["hands"].ToString() != "1" && i["hands"].ToString() != "0")
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid handedness value (Valid values: 0,1 and 2). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["defense"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no defense value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["defense"].ToString(), out int def))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric defense value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["attack"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no attack penalty value (If 0, set value to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["attack"].ToString(), out int att))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric attack penalty value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Hands = i["hands"].ToString(),
										Defense = i["defense"].ToString(),
										Attack = i["attack"].ToString()
									});
									break;
								case ItemTypes.weapon:
									if (i["hands"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no handedness value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["hands"].ToString() != "2" && i["hands"].ToString() != "1" && i["hands"].ToString() != "0")
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid handedness value (Valid values: 0,1 and 2). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["damage"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no damage value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["damage"].ToString(), out int dmg))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric damage value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["range"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no range value (if 0, set this field to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["range"].ToString(), out int rng))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric range value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["energy"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no additional energy cost value (if 0, set this field to 0). Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["energy"].ToString(), out int eng))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric additional energy cost value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Hands = i["hands"].ToString(),
										Damage = i["damage"].ToString(),
										Range = i["range"].ToString(),
										Energy = i["energy"].ToString()
									});
									break;
								case ItemTypes.usable:
									if (i["frequency"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no per scene frequency. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (!int.TryParse(i["frequency"].ToString(), out int freq))
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid/non-numeric per scene frequency value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString(),
										Frequency = i["frequency"].ToString()
									});
									break;
								case ItemTypes.upgrade:
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString()
									});
									break;
								case ItemTypes.consumable:
									if (i["effect"].ToString().NullorEmpty())
									{
										await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has no effect value. Fix this error and send the file again.");
										File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
										return;
									}
									parseditems.Add(new Item()
									{
										Name = i["name"].ToString(),
										Type = i["type"].ToString(),
										Cost = i["cost"].ToString(),
										Effect = i["effect"].ToString()
									});
									break;
							}

						}
						else
						{
							await ReplyAsync(Context.User.Mention + ", item " + i["name"] + " has an invalid type (Valid types: weapon, armor, shield, usable, consumable, upgrade). Fix this error and send the file again.");
							File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "data", "temp", file.Filename));
							return;
						}
					}
					p.Items = parseditems;
				}

				p.Metadata = JsonConvert.DeserializeObject<Metadata>(json["metadata"].ToString());
				col.Update(p);
				await ReplyAsync(Context.User.Mention + ", Successfully uploaded version " + p.Metadata.version + " of content pack " + p.Metadata.name + ".");
			}
			catch
			{
				await ReplyAsync(Context.User.Mention + ", Something is wrong with this JSON file. Please check your file on this website to ensure it's correct: https://jsonformatter.curiousconcept.com/.");
			}

		}
		[Command("Subscribe"), Alias("Sub")]
		public async Task subscribe([Remainder] string name)
		{
			var user = Utils.GetUser(Context.User.Id);

			var col = Database.GetCollection<ContentPack>("ContentPacks");

			var packs = col.Find(x => x.Name.StartsWith(name.ToLower()));
			packs = packs.OrderBy(x => x.Name);

			if (packs.Count() == 0)
			{
				await ReplyAsync(Context.User.Mention + ", there are no content packs with that name.");
				return;
			}
			else if (packs.Count() == 1 && packs.FirstOrDefault().Metadata.name.ToLower() == name.ToLower())
			{
				var p = packs.FirstOrDefault();
				if (user.Subscriptions.Contains(p))
				{
					await ReplyAsync(Context.User.Mention + ", You're already subscribed to this content pack.");
					return;
				}
				else
				{
					user.Subscriptions.Add(p);
					Utils.UpdateUser(user);
					await ReplyAsync( Context.User.Mention + ", You're now subscribed to the content pack " + p.Metadata.name);
				}
			}
			else if(packs.Count() > 1 && packs.Count() < 6)
			{
				
				var sb = new StringBuilder();
				sb.AppendLine(Context.User.Mention + ", Multiple content packs where found. Please respond with the number of the pack you wish to subscribe to:");
				for(int i = 0; i < packs.Count(); i++)
				{
					sb.AppendLine("`[" + i + "]` " + packs.ElementAt(i).Metadata.name);
				}
				var msg = await ReplyAsync(sb.ToString());

				var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));
				
				if(response == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", Timed out on selection.");
					return;
				}
				if(int.TryParse(response.Content,out int index))
				{
					if(Math.Abs(index) >= packs.Count())
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This is not a valid option.");
						try
						{
							await response.DeleteAsync();
						}
						catch
						{

						}
						return;
					}
					else
					{
						try
						{
							await response.DeleteAsync();
						}
						catch
						{

						}
						var p = packs.ElementAt(index);
						if (user.Subscriptions.Contains(p))
						{
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You're already subscribed to this content pack.");
							return;
						}
						else
						{
							user.Subscriptions.Add(p);
							Utils.UpdateUser(user);
							await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You're now subscribed to the content pack " + p.Metadata.name);
						}
					}
				}
				else
				{
					try
					{
						await response.DeleteAsync();
					}
					catch
					{

					}
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling command.");
					return;
				}
			}
			
		}
		[Command("Unsubscribe"), Alias("Unsub")]
		public async Task unsubscribe([Remainder] string name)
		{
			var user = Utils.GetUser(Context.User.Id);

			var packs = user.Subscriptions.Where(x => x.Metadata.name.StartsWith(name.ToLower()));
			packs = packs.OrderBy(x => x.Metadata.name);

			if (packs.Count() == 0)
			{
				await ReplyAsync(Context.User.Mention + ", You aren't subscribed to any content pack with that name.");
				return;
			}
			else if (packs.Count() == 1 && packs.FirstOrDefault().Metadata.name.ToLower() == name.ToLower())
			{
				var p = packs.FirstOrDefault();
				user.Subscriptions.Remove(p);
				Utils.UpdateUser(user);
				await ReplyAsync(Context.User.Mention + ", You're now unsubscribed from the content pack " + p.Metadata.name);

			}
			else if (packs.Count() > 1 && packs.Count() < 6)
			{

				var sb = new StringBuilder();
				sb.AppendLine(Context.User.Mention + ", Multiple content packs where found. Please respond with the number of the pack you wish to unsubscribe to:");
				for (int i = 0; i < packs.Count(); i++)
				{
					sb.AppendLine("`[" + i + "]` " + packs.ElementAt(i).Metadata.name);
				}
				var msg = await ReplyAsync(sb.ToString());

				var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));

				if (response == null)
				{
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", Timed out on selection.");
					return;
				}
				if (int.TryParse(response.Content, out int index))
				{
					if (Math.Abs(index) >= packs.Count())
					{
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This is not a valid option.");
						try
						{
							await response.DeleteAsync();
						}
						catch
						{

						}
						return;
					}
					else
					{
						try
						{
							await response.DeleteAsync();
						}
						catch
						{

						}
						var p = packs.ElementAt(index);
						user.Subscriptions.Remove(p);
						Utils.UpdateUser(user);
						await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", You're now unsubscribed from the content pack " + p.Metadata.name);

					}
				}
				else
				{
					try
					{
						await response.DeleteAsync();
					}
					catch
					{

					}
					await msg.ModifyAsync(x => x.Content = Context.User.Mention + ", This isn't a number. Cancelling command.");
					return;
				}
			}

		}
	}
}
