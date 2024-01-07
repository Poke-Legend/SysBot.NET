﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using PKHeX.Core;
using System;
using System.Threading.Tasks;
using System.Globalization;
using static SysBot.Pokemon.Discord.Helpers.ColorHelper;
using static PKHeX.Core.AutoMod.Aesthetics;
using System.Linq;
using SysBot.Base;
using static SysBot.Pokemon.Discord.Helpers.EggColors;
using SysBot.Pokemon.Discord.Helpers;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using MathNet.Numerics;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace SysBot.Pokemon.Discord
{
    public static class QueueHelper<T> where T : PKM, new()
    {
        private const uint MaxTradeCode = 9999_9999;

        private static EmbedBuilder Embed { get; set; } = new();

        private static string? ETA;
        private static string? Queuepos;
        private static QueueResultAdd? Added;

        public static MoveType TeraTypeOriginal { get; set; } // You can change the type to byte or int if required.
        public static MoveType TeraTypeOverride { get; set; }
        public static MoveType TeraType => TeraTypeUtil.GetTeraType((byte)TeraTypeOriginal, (byte)TeraTypeOverride);
        public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, SocketUser trader, int catchID = 0)
        {
            if ((uint)code > MaxTradeCode)
            {
                await SendEmbedMessage(context, "Trade code should be 00000000-99999999!", trade).ConfigureAwait(false);

                return;
            }

            try
            {
                const string helper = "I've added you to the queue! I'll message you here when your trade is starting.";
                var embedHelper = new EmbedBuilder() { Color = GetDiscordColor(trade.IsShiny ? ShinyMap[((Species)trade.Species, trade.Form)] : (PersonalColor)trade.PersonalInfo.Color), Description = helper };
                IUserMessage test = await trader.SendMessageAsync(embed: embedHelper.Build()).ConfigureAwait(false);
                var hub = SysCord<T>.Runner.Hub;

                // Try adding
                var result = AddToTradeQueue(context, trade, code, trainer, sig, routine, type, trader, out var msg, catchID);


                // Notify in channel
                if (hub.Config.Trade.UseTradeEmbeds)
                {
                    if (Added == QueueResultAdd.AlreadyInQueue)
                    {
                        await context.Channel.SendMessageAsync(msg).ConfigureAwait(false);
                    }
                    if (type is PokeTradeType.Specific && Added != QueueResultAdd.AlreadyInQueue)
                    {
                        await AddToTradeQueueEmbed(trade, trader, msg);
                        await context.Channel.SendMessageAsync(embed: Embed.Build()).ConfigureAwait(false);
                    }
                }
                else
                {
                    await SendEmbedMessage(context, msg, trade).ConfigureAwait(false);
                }

                // Notify the user privately about the trade code. 
                await SendEmbedMessageToUser(trader, $"Your trade code will be **{code:0000 0000}**.", trade).ConfigureAwait(false);

                // Clean Up
                if (result)
                {
                    // Delete the user's join message for privacy
                    if (!context.IsPrivate)
                        await context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);
                }
                else
                {
                    // Delete our "I'm adding you!", and send the same message that we sent to the general channel.
                    await test.DeleteAsync().ConfigureAwait(false);
                }

            }
            catch (HttpException ex)
            {
                await HandleDiscordExceptionAsync(context, trader, ex).ConfigureAwait(false);
            }

        }
      
        public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, int catchID = 0)
        {
            await AddToQueueAsync(context, code, trainer, sig, trade, routine, type, context.User, catchID).ConfigureAwait(false);
        }

      
        private static bool AddToTradeQueue(SocketCommandContext context, T pk, int code, string trainerName, RequestSignificance sig, PokeRoutineType type, PokeTradeType t, SocketUser trader, out string msg, int catchID = 0)
        {

            var userID = trader.Id;
            var name = trader.Username;

            var trainer = new PokeTradeTrainerInfo(trainerName, userID);
            var notifier = new DiscordTradeNotifier<T>(pk, trainer, code, trader);
            var detail = new PokeTradeDetail<T>(pk, trainer, notifier, t, code, sig == RequestSignificance.Favored);
            var trade = new TradeEntry<T>(detail, userID, type, name);
            var mgr = SysCordSettings.Manager;
            var hub = SysCord<T>.Runner.Hub;
            var Info = hub.Queues.Info;
            var inqueuemsg = "Sorry, you are already in the queue.";

            Added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

            if (Added == QueueResultAdd.AlreadyInQueue && !mgr.CanUseSudo(trader.Id))
            {
                EmbedBuilder embed = new()
                {
                    Description = inqueuemsg,

                    Color = GetDiscordColor(pk.IsShiny ? ShinyMap[((Species)pk.Species, pk.Form)] : (PersonalColor)pk.PersonalInfo.Color)
                };

                context.Channel.SendMessageAsync(embed: embed.Build());

                msg = "";
               return false;
            }

            var position = Info.CheckPosition(userID, type);

            var ticketID = "";
            if (TradeStartModule<T>.IsStartChannel(context.Channel.Id))
                ticketID = $", unique ID: {detail.ID}";

            var pokeName = "";
            if ((t == PokeTradeType.Specific || t == PokeTradeType.SupportTrade || t == PokeTradeType.Giveaway) && pk.Species != 0)
            pokeName = $"{(hub.Config.Trade.UseTradeEmbeds ? "" : t == PokeTradeType.SupportTrade && pk.Species != (int)Species.Ditto && pk.HeldItem != 0 ? $" Receiving: {(Species)pk.Species} ({ShowdownParsing.GetShowdownText(pk).Split('@', '\n')[1].Trim()})" : $" Receiving: {(Species)pk.Species}. ")}";
            string? pokeName2 = $" Receiving: {(t == PokeTradeType.SupportTrade && pk.Species != (int)Species.Ditto && pk.HeldItem != 0 ? $"{(Species)pk.Species} ({ShowdownParsing.GetShowdownText(pk).Split('@', '\n')[1].Trim()})" : $"{(Species)pk.Species}")}.";
            msg = $" Trade Code sent to DM. \n ";
            Queuepos = $" You are in queue #{position.Position} \n{msg}";

            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                ETA = $"Estimated Wait: {eta:F1} minutes. \n";
                msg += ETA;
            }
            return true;
        }

        public static async Task AddToMysteryEggQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, SocketUser trader, int catchID = 0)
        {
            LogUtil.LogText($"Starting AddToMysteryEggQueueAsync for trader {trainer}");

            var userID = trader.Id;
            var trainerInfo = new PokeTradeTrainerInfo(trainer, userID);
            var notifier = new DiscordTradeNotifier<T>(trade, trainerInfo, code, trader);
            var tradeDetail = new PokeTradeDetail<T>(trade, trainerInfo, notifier, type, code);


            tradeDetail.IsMysteryTrade = true;

            if ((uint)code > MaxTradeCode)
            {
                await SendEmbedMessage(context, "Trade code should be 00000000-99999999!", trade).ConfigureAwait(false);

                return;
            }

            try
            {
                const string helper = "I've added you to the mystery queue! I'll message you here when your trade is starting.";
                var embedHelper = new EmbedBuilder()
                {
                    Color = GetDiscordColor(trade.IsShiny ? ShinyMap[((Species)trade.Species, trade.Form)] : (PersonalColor)trade.PersonalInfo.Color),
                    Description = helper,
                    ImageUrl = "https://raw.githubusercontent.com/Poke-Legend/HomeIMG/97a2b209f9d0483a8e237fe2a89d78547b4ea232/Sprites/512x512/MysteryEgg.png"
                };


                IUserMessage test = await trader.SendMessageAsync(embed: embedHelper.Build()).ConfigureAwait(false);


                var hub = SysCord<T>.Runner.Hub;
                var result = AddToTradeQueue(context, trade, code, trainer, sig, routine, type, trader, out var msg, catchID);


                if (hub.Config.Trade.UseTradeEmbeds)
                {

                    if (Added == QueueResultAdd.AlreadyInQueue)
                    {
                        await context.Channel.SendMessageAsync(msg).ConfigureAwait(false);
                    }
                    else if (type is PokeTradeType.Mystery && Added != QueueResultAdd.AlreadyInQueue)
                    {
                        // Modified section starts here
                        var footer = new EmbedFooterBuilder
                        {
                            Text = $"{Queuepos}\nRequested on: {DateTime.Now.ToString("D", CultureInfo.CurrentCulture)} at {DateTime.Now.ToString("t", CultureInfo.CurrentCulture)}"
                        };
                        var name = trader.Username;
                        var author = new EmbedAuthorBuilder
                        {
                            Name = $"{name}'s{(trade.IsShiny ? " Shiny" : "")} Pokémon",
                            IconUrl = trader.GetAvatarUrl(),
                        };
                        var embedToSend = new EmbedBuilder
                        {
                            Title = " ❓❓ Mystery Egg Trade ❓❓ \n Check your DM's for the Trade Code!",
                            Color = GetDiscordColor(trade.IsShiny ? ShinyMap[((Species)trade.Species, trade.Form)] : (PersonalColor)trade.PersonalInfo.Color),
                            Author = author,
                            ThumbnailUrl = $"https://raw.githubusercontent.com/Poke-Legend/HomeIMG/main/Ballimg/80x80/{((Ball)trade.Ball).ToString().ToLower()}ball.png",
                            Footer = footer,
                            ImageUrl = "https://raw.githubusercontent.com/Poke-Legend/HomeIMG/97a2b209f9d0483a8e237fe2a89d78547b4ea232/Sprites/512x512/MysteryEgg.png" // Replace with your image URL
                        };

                        await context.Channel.SendMessageAsync(embed: embedToSend.Build()).ConfigureAwait(false);
                        // Modified section ends here
                    }
                }
                else
                {
                    await SendEmbedMessage(context, msg, trade).ConfigureAwait(false);
                }



               await SendEmbedMessageToUser(trader, $"Your trade code will be **{code:0000 0000}**.", trade).ConfigureAwait(false);


                if (result && !context.IsPrivate)
                {

                    await context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);

                }
                else if (!result)
                {

                    await test.DeleteAsync().ConfigureAwait(false);

                }
            }
            catch (HttpException ex)
            {
                LogUtil.LogText($"Caught HttpException: {ex.Message}");
                await HandleDiscordExceptionAsync(context, trader, ex).ConfigureAwait(false);
            }
        }

        private static async Task AddToTradeQueueEmbed(T pk, SocketUser trader, string msg)
        {
            var nickName = pk.Species == 0 ? string.Empty : $" ({pk.Nickname})";
            LogUtil.LogText($"This is the nickname {nickName}.");
            var user = trader;
            var name = user.Username;
            var form = TradeExtensions<T>.FormOutput(pk.Species, pk.Form, out _);
            GameStrings strings = GameInfo.GetStrings(GameLanguage.DefaultLanguage);
            var items = strings.GetItemStrings((EntityContext)8, GameVersion.SWSH);
            var itemName = items[pk.HeldItem];

            string footerIcon;

            if (pk.HeldItem == 0)
            {
                footerIcon = $"https://raw.githubusercontent.com/Poke-Legend/HomeIMG/main/Ballimg/128x128/{((Ball)pk.Ball).ToString().ToLower()}ball.png";
            }
            else
            {
                footerIcon = $"https://raw.githubusercontent.com/Poke-Legend/HomeIMG/main/Ballimg/128x128/{((Ball)pk.Ball).ToString().ToLower()}ball.png";
            }
            var author = new EmbedAuthorBuilder
            {
                Name = $"{name}'s{(pk.IsShiny ? " Shiny" : "")} Pokémon",
                IconUrl = user.GetAvatarUrl(),
            };
            var footer = new EmbedFooterBuilder
            {
                IconUrl = footerIcon,
                Text = Queuepos + ETA + DateTime.Now

            };

            string formOrHeldItem = string.IsNullOrEmpty(form) ? itemName : form;
            string heldItemLabel = string.IsNullOrEmpty(form) ? "" : "**HeldItem:** ";
            string movesList = "";
            for (int i = 0; i < pk.Moves.Length; i++)
            {
                if (pk.Moves[i] != 0)
                {
                    movesList += $"- {(Move)pk.Moves[i]}\n";
                }
            }
        
            var teraType = Tera9RNG.GetTeraType(Tera9RNG.GetOriginalSeed(pk), GemType.Default, (ushort)pk.Species, pk.Form);
                                  
            // Add '**' before and after genderText to make it bold
            var genderText = pk.Gender == 0 ? " (M)" : pk.Gender == 1 ? " (F)" : "";

            // Create a variable for the name section
            string nameText = $"{(pk.IsShiny ? "✨ " : "")}{(Species)pk.Species}{genderText}\n";
            string nameText2 = $"Terastalize";
            string nameText3 = $"Nature";
            string formText = $"{(!string.IsNullOrEmpty(form) ? $" **Form**: {form}" : "")}\n";
            // Create variables for the other sections
            string heldItemText = pk.HeldItem != 0 ? $"**Held Item**: {itemName}\n" : "";

            bool areAllIVsMaxed = pk.IV_HP == 31 && pk.IV_ATK == 31 && pk.IV_DEF == 31 && pk.IV_SPA == 31 && pk.IV_SPD == 31 && pk.IV_SPE == 31;

            // Create the IV section of the text
            string ivText = areAllIVsMaxed ? "**IVs**: 6IV\n" :
                $"**IVs**: {string.Join(" / ", new[]
                {
            pk.IV_HP != 31 ? $"{pk.IV_HP} HP" : "",
            pk.IV_ATK != 31 ? $"{pk.IV_ATK} ATK" : "",
            pk.IV_DEF != 31 ? $"{pk.IV_DEF} DEF" : "",
            pk.IV_SPA != 31 ? $"{pk.IV_SPA} SPA" : "",
            pk.IV_SPD != 31 ? $"{pk.IV_SPD} SPD" : "",
            pk.IV_SPE != 31 ? $"{pk.IV_SPE} SPE" : ""
                }.Where(s => !string.IsNullOrEmpty(s)))}\n";

            List<string> evStats = new List<string>();
            if (pk.EV_HP != 0) evStats.Add($"{pk.EV_HP} HP");
            if (pk.EV_ATK != 0) evStats.Add($"{pk.EV_ATK} ATK");
            if (pk.EV_DEF != 0) evStats.Add($"{pk.EV_DEF} DEF");
            if (pk.EV_SPA != 0) evStats.Add($"{pk.EV_SPA} SPA");
            if (pk.EV_SPD != 0) evStats.Add($"{pk.EV_SPD} SPD");
            if (pk.EV_SPE != 0) evStats.Add($"{pk.EV_SPE} SPE");

            string evText = evStats.Count > 0 ? $"**EVs**: {string.Join(" / ", evStats)}\n" : "";
            
            string abilityText = $"**Ability**: {(Ability)pk.Ability}\n";
            string teraTypeText = $"**Type**: {(MoveType)teraType}\n";
            string levelText = pk.CurrentLevel != 1 ? $"**Level**: {pk.CurrentLevel}\n" : "";
            string languageText = $"**Language**: {(LanguageID)pk.Language}\n";

            string natureText = $"**{(Nature)pk.Nature}** **Nature** \n";
            string movesHeaderText = "**Moves**:\n";
            string movesText = movesList;
            string imageUrl;
            if (nickName.Trim().Equals("(Egg)", StringComparison.OrdinalIgnoreCase))
            {
                LogUtil.LogText($"This is the nickname {nickName}.");
                // Determine the PersonalColor here
                PersonalColor personalColor = pk.IsShiny ?
                    ShinyMap[((Species)pk.Species, pk.Form)] :
                    (PersonalColor)pk.PersonalInfo.Color;

                // Use the PersonalColor to get the imageUrl from EggColors
                EggColor eggColor = EggColors.ColorToEggColor[personalColor];

                // Get the imageUrl based on EggColor
                imageUrl = GetEggImageUrl(eggColor);
            }
            else
            {
                LogUtil.LogText($"This is the nickname {nickName}.");
                imageUrl = TradeExtensions<PK9>.PokeImg(pk, false, false);
            }

            string thumbnailURL;

            if (pk.HeldItem == 0)
            {
                // Set a default thumbnail URL when there is no held item.
                // Replace with the appropriate URL or path for the default image.
                thumbnailURL = "https://sysbots.net/wp-content/uploads/2023/09/logosys.png";
            }
            else
            {
                // Use the ItemImg method to get the URL for the held item.
                thumbnailURL = await TradeExtensions<PK9>.ItemImg(itemName);
            }


            Embed = new EmbedBuilder
            {
                Color = GetDiscordColor(pk.IsShiny ? ShinyMap[((Species)pk.Species, pk.Form)] : (PersonalColor)pk.PersonalInfo.Color),
                
                ThumbnailUrl = thumbnailURL,
                Author = author,
                Footer = footer,
                ImageUrl = imageUrl,
             };
            Embed.Fields.Add(new EmbedFieldBuilder
            {
                Name = $"{nameText}",
                Value = $"{formText}{heldItemText}{ivText}{evText}{languageText}{abilityText}{levelText}",
                IsInline = false,
                
            });
            Embed.Fields.Add(new EmbedFieldBuilder
            {
                 Name = $"{nameText2}",
                 Value = $"{teraTypeText}", // Ensure teraTypeText is set appropriately
                 IsInline = false,
            });
            Embed.Fields.Add(new EmbedFieldBuilder
            {
                Name = $"{nameText3}",
                Value = $"{natureText}",
                IsInline = false,

            });
            Embed.Fields.Add(new EmbedFieldBuilder
            {
                Name = movesHeaderText,
                Value = movesText,
                IsInline = false,

            });


            var channel = await trader.CreateDMChannelAsync().ConfigureAwait(false);
            await channel.SendMessageAsync(embed: Embed.Build()).ConfigureAwait(false);
        }
        private static string GetEggImageUrl(EggColor eggColor)
        {
            // Convert EggColor to PersonalColor using the mapping
            PersonalColor personalColor = EggColors.ColorToEggColor.FirstOrDefault(x => x.Value == eggColor).Key;

            // Get the URL based on the PersonalColor
            if (EggColors.ColorToImageUrl.ContainsKey(personalColor))
            {
                return EggColors.ColorToImageUrl[personalColor];
            }

            // Default to a generic egg image if the PersonalColor is not found
            return "https://i.imgur.com/sfaIQec.png";
        }

        private static async Task HandleDiscordExceptionAsync(SocketCommandContext context, SocketUser trader, HttpException ex)
        {
            string message = string.Empty;
            switch (ex.DiscordCode)
            {
                case DiscordErrorCode.InsufficientPermissions or DiscordErrorCode.MissingPermissions:
                    {
                        // Check if the exception was raised due to missing "Send Messages" or "Manage Messages" permissions. Nag the bot owner if so.
                        var permissions = context.Guild.CurrentUser.GetPermissions(context.Channel as IGuildChannel);
                        if (!permissions.SendMessages)
                        {
                            // Nag the owner in logs.
                            message = "You must grant me \"Send Messages\" permissions!";
                            Base.LogUtil.LogError(message, "QueueHelper");
                            return;
                        }
                        if (!permissions.ManageMessages)
                        {
                            var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                            var owner = app.Owner.Id;
                            message = $"<@{owner}> You must grant me \"Manage Messages\" permissions!";
                        }
                    }
                    break;
                case DiscordErrorCode.CannotSendMessageToUser:
                    {
                        // The user either has DMs turned off, or Discord thinks they do.
                        message = context.User == trader ? "You must enable private messages in order to be queued!" : "The mentioned user must enable private messages in order for them to be queued!";
                    }
                    break;
                default:
                    {
                        // Send a generic error message.
                        message = ex.DiscordCode != null ? $"Discord error {(int)ex.DiscordCode}: {ex.Reason}" : $"Http error {(int)ex.HttpCode}: {ex.Message}";
                    }
                    break;
            }
            await context.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }
        public static async Task SendEmbedMessage(SocketCommandContext context, string message, T pk)
        {
            var embed = new EmbedBuilder()
            {
                Color = GetDiscordColor(pk.IsShiny ? ShinyMap[((Species)pk.Species, pk.Form)] : (PersonalColor)pk.PersonalInfo.Color),
                Description = message
            }.Build();

            await context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        public static async Task SendEmbedMessageToUser(SocketUser user, string message, T pk)
        {
            var embed = new EmbedBuilder()
            {
                Color = GetDiscordColor(pk.IsShiny ? ShinyMap[((Species)pk.Species, pk.Form)] : (PersonalColor)pk.PersonalInfo.Color),
                Description = message
            }
            .WithCurrentTimestamp()
            .Build();

            var dmChannel = await user.CreateDMChannelAsync().ConfigureAwait(false);
            await dmChannel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }



    }
}