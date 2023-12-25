﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Queues new Link Code trades")]
    public class TradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

        [Command("tradeList")]
        [Alias("tl")]
        [Summary("Prints the users in the trade queues.")]
        [RequireSudo]
        public async Task GetTradeListAsync()
        {
            string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
            var embed = new EmbedBuilder();
            embed.AddField(x =>
            {
                x.Name = "Pending Trades";
                x.Value = msg;
                x.IsInline = false;
            });
            await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the provided Pokémon file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public Task TradeAsyncAttach([Summary("Trade Code")] int code)
        {
            var sig = Context.User.GetFavor();
            return TradeAsyncAttach(code, sig, Context.User);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var template = AutoLegalityWrapper.GetTemplate(set);
            if (set.InvalidLines.Count != 0)
            {
                var unableToParseSet = $"Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
                var embedUnableToParseSet = new EmbedBuilder()
                {
                    Color = Color.Blue
                }
                .WithDescription(unableToParseSet)
                .WithThumbnailUrl("https://sysbots.net/wp-content/uploads/2023/09/logosys.png")
                .WithCurrentTimestamp()
                .Build();
                await ReplyAsync(null, false, embedUnableToParseSet).ConfigureAwait(false);
                return;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                bool pla = typeof(T) == typeof(PA8);

                string targetNickname = "egg";
                string pokemonNickname = pkm.Nickname.ToLower(); // Convert the Pokémon's nickname to lowercase

                if (!pla && pokemonNickname == targetNickname) // Compare lowercase Pokémon nickname with lowercase target word
                {
                    if (Breeding.CanHatchAsEgg(pkm.Species))
                    {
                        TradeExtensions<T>.EggTrade(pkm, template);
                    }
                }

                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                bool memes = Info.Hub.Config.Trade.Memes && await TradeAdditionsModule<T>.TrollAsync(Context, pkm is not T || !la.Valid, pkm).ConfigureAwait(false);
                if (memes)
                    return;

                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result == "Timeout" ? $"That {spec} set took too long to generate." : result == "VersionMismatch" ? "Request refused: PKHeX and Auto-Legality Mod version mismatch." : $"I wasn't able to create a {spec} from that set.";
                    var imsg = $"Oops! ** {reason} **";

                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";

                    var author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    };
                    var oopsEmbed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = Color.Red
                    }
                    .WithDescription(imsg)
                    .WithThumbnailUrl("https://sysbots.net/wp-content/uploads/2023/09/logosys.png")
                    .WithCurrentTimestamp()
                    .Build();


                    await ReplyAsync(null, false, oopsEmbed).ConfigureAwait(false);
                    return;
                }
                pk.ResetPartyStats();

                var sig = Context.User.GetFavor();
                await AddTradeToQueueAsync(code, Context.User.Username, pk, sig, Context.User).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var author = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username,
                    IconUrl = Context.User.GetAvatarUrl()
                };
                var oopsEmbed = new EmbedBuilder()
                {
                    Author = author,
                    Color = Color.Red
                }
                .WithDescription($"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```")
                .WithThumbnailUrl("https://sysbots.net/wp-content/uploads/2023/09/logosys.png")
                .WithCurrentTimestamp()
                .Build();

                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                await ReplyAsync(null, false, oopsEmbed).ConfigureAwait(false);
            }
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you a Pokémon converted from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task TradeAsync([Summary("Showdown Set")][Remainder] string content)
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsync(code, content).ConfigureAwait(false);
        }

        [Command("trade")]
        [Alias("t")]
        [Summary("Makes the bot trade you the attached file.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task Tra()
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsyncAttach(code).ConfigureAwait(false);
        }


        [Command("banTrade")]
        [Alias("bt")]
        [RequireQueueRole(nameof(DiscordManager.FavoredRoles))]
        public async Task BanTradeAsync([Summary("Online ID")] ulong nnid, string comment)
        {
            SysCordSettings.HubConfig.TradeAbuse.BannedIDs.AddIfNew(new[] { GetReference(nnid, comment) });
            await ReplyAsync("Done.").ConfigureAwait(false);
        }

        private RemoteControlAccess GetReference(ulong id, string comment) => new()
        {
            ID = id,
            Name = id.ToString(),
            Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss} ({comment})",
        };

        [Command("tradeUser")]
        [Alias("tu", "tradeOther")]
        [Summary("Makes the bot trade the mentioned user the attached file.")]
        [RequireQueueRole(nameof(DiscordManager.FavoredRoles))]
        public async Task TradeAsyncAttachUser([Summary("Trade Code")] int code, [Remainder] string _)
        {
            if (Context.Message.MentionedUsers.Count > 1)
            {
                await ReplyAsync("Too many mentions. Queue one user at a time.").ConfigureAwait(false);
                return;
            }

            if (Context.Message.MentionedUsers.Count == 0)
            {
                await ReplyAsync("A user must be mentioned in order to do this.").ConfigureAwait(false);
                return;
            }

            var usr = Context.Message.MentionedUsers.ElementAt(0);
            var sig = usr.GetFavor();
            await TradeAsyncAttach(code, sig, usr).ConfigureAwait(false);
        }

        [Command("tradeUser")]
        [Alias("tu", "tradeOther")]
        [Summary("Makes the bot trade the mentioned user the attached file.")]
        [RequireQueueRole(nameof(DiscordManager.FavoredRoles))]
        public async Task TradeAsyncAttachUser([Remainder] string _)
        {
            var code = Info.GetRandomTradeCode();
            await TradeAsyncAttachUser(code, _).ConfigureAwait(false);
        }

        [Command("tradeuser")]
        [Alias("tu", "tradeother")]
        [Summary("Makes the bot trade the mentioned user from the provided Showdown Set.")]
        [RequireQueueRole(nameof(DiscordManager.FavoredRoles))]
        public async Task TradeAsyncAttatchUser([Summary("Trade Code")] int code, [Summary("Showdown Set")][Remainder] string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var template = AutoLegalityWrapper.GetTemplate(set);
              if (Context.Message.MentionedUsers.Count > 1)
            {
                await ReplyAsync("Too many mentions. Queue one user at a time.").ConfigureAwait(false);
                return;
            }

            if (Context.Message.MentionedUsers.Count == 0)
            {
                await ReplyAsync("A user must be mentioned in order to do this.").ConfigureAwait(false);
                return;
            }
            
            
            
            if (set.InvalidLines.Count != 0)
            {
                var msg = $"Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
                await ReplyAsync(msg).ConfigureAwait(false);
                return;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                bool pla = typeof(T) == typeof(PA8);

                if (!pla && pkm.Nickname.ToLower() == "egg" && Breeding.CanHatchAsEgg(pkm.Species))
                    TradeExtensions<T>.EggTrade(pkm, template);

                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                bool memes = Info.Hub.Config.Trade.Memes && await TradeAdditionsModule<T>.TrollAsync(Context, pkm is not T || !la.Valid, pkm).ConfigureAwait(false);
                if (memes)
                    return;

                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result == "Timeout" ? $"That {spec} set took too long to generate." : result == "VersionMismatch" ? "Request refused: PKHeX and Auto-Legality Mod version mismatch." : $"I wasn't able to create a {spec} from that set.";
                    var imsg = $"Oops! {reason}";
                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                    await ReplyAsync(imsg).ConfigureAwait(false);
                    return;
                }
                pk.ResetPartyStats();

                var sig = Context.User.GetFavor();
                await AddTradeToQueueAsync(code, Context.User.Username, pk, sig, Context.User).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                await ReplyAsync(msg).ConfigureAwait(false);
            }
        }

        [Command("tradeUser")]
        [Alias("tu", "tradeOther")]
        [Summary("Trades a Showdown Set to the mentioned user.")]
        [RequireQueueRole(nameof(DiscordManager.FavoredRoles))]
        public async Task TradeUserSetAsync(SocketUser mentionedUser, [Summary("Showdown Set")][Remainder] string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var template = AutoLegalityWrapper.GetTemplate(set);
            if (set.InvalidLines.Count != 0)
            {
                var msg = $"Unable to parse Showdown Set:\n{string.Join("\n", set.InvalidLines)}";
                await ReplyAsync(msg).ConfigureAwait(false);
                return;
            }
            try
            {
                // Get the ITrainerInfo instance from the appropriate source (modify as needed)
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>(); // Modify the arguments as needed

                // Get the mentioned user's signature (assuming you have a method to get this)
                var sig = mentionedUser.GetFavor();

                var pkm = sav.GetLegal(template, out var result);
                bool pla = typeof(T) == typeof(PA8);

                string targetNickname = "egg";
                string pokemonNickname = pkm.Nickname.ToLower(); // Convert the Pokémon's nickname to lowercase

                if (!pla && pokemonNickname == targetNickname) // Compare lowercase Pokémon nickname with lowercase target word
                {
                    if (Breeding.CanHatchAsEgg(pkm.Species))
                    {
                        TradeExtensions<T>.EggTrade(pkm, template);
                    }
                }
                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                bool memes = Info.Hub.Config.Trade.Memes && await TradeAdditionsModule<T>.TrollAsync(Context, pkm is not T || !la.Valid, pkm).ConfigureAwait(false);
                if (memes)
                    return;

                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result == "Timeout" ? $"That {spec} set took too long to generate." : result == "VersionMismatch" ? "Request refused: PKHeX and Auto-Legality Mod version mismatch." : $"I wasn't able to create a {spec} from that set.";
                    var imsg = $"Oops! **{reason}**";
                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";

                    await ReplyAsync(imsg).ConfigureAwait(false);
                    return;
                }
                pk.ResetPartyStats();

                // Get a Link Trade code
                int code = Info.GetRandomTradeCode();

                // Add the egg to the Link Trade queue with the mentioned user's name
                await AddTradeToQueueAsync(code, mentionedUser.Username, pk, sig, mentionedUser).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                await ReplyAsync(msg).ConfigureAwait(false);
            }
        }

        private async Task TradeAsyncAttach(int code, RequestSignificance sig, SocketUser usr)
        {
            var attachment = Context.Message.Attachments.FirstOrDefault();
            if (attachment == default)
            {
                await ReplyAsync("No attachment provided!").ConfigureAwait(false);
                return;
            }

            var settings = SysCord<T>.Runner.Hub.Config.Legality;
            var defTrainer = new SimpleTrainerInfo()
            {
                OT = settings.GenerateOT,
                TID16 = settings.GenerateTID16,
                SID16 = settings.GenerateSID16,
                Language = (int)settings.GenerateLanguage,
            };

            var att = await NetUtil.DownloadPKMAsync(attachment, defTrainer).ConfigureAwait(false);
            var pk = GetRequest(att);
            if (pk == null)
            {
                await ReplyAsync("Attachment provided is not compatible with this module!").ConfigureAwait(false);
                return;
            }

            await AddTradeToQueueAsync(code, usr.Username, pk, sig, usr).ConfigureAwait(false);
        }

        private static T? GetRequest(Download<PKM> dl)
        {
            if (!dl.Success)
                return null;
            return dl.Data switch
            {
                null => null,
                T pk => pk,
                _ => EntityConverter.ConvertToType(dl.Data, typeof(T), out _) as T,
            };
        }

        private async Task AddTradeToQueueAsync(int code, string trainerName, T pk, RequestSignificance sig, SocketUser usr)
        {
            if (!pk.CanBeTraded())
            {
                await ReplyAsync("Provided Pokémon content is blocked from trading!").ConfigureAwait(false);
                return;
            }
            
            var la = new LegalityAnalysis(pk);

            if (!la.Valid && la.Results.Any(m => m.Identifier is CheckIdentifier.Memory))
            {
                var clone = (T)pk.Clone();

                clone.HT_Name = pk.OT_Name;
                clone.HT_Gender = pk.OT_Gender;

                if (clone is PK8 or PA8 or PB8 or PK9)
                    ((dynamic)clone).HT_Language = (byte)pk.Language;

                clone.CurrentHandler = 1;
                
                la = new LegalityAnalysis(clone);

                if (la.Valid) pk = clone;
            }

            if (!la.Valid)
            {
                await ReplyAsync($"{typeof(T).Name} attachment is not legal, and cannot be traded!").ConfigureAwait(false);
                return;
            }

            await QueueHelper<T>.AddToQueueAsync(Context, code, trainerName, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, usr).ConfigureAwait(false);
        }
    }
}
