using Discord;
using Discord.Commands;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    [Summary("Distribution Pool Module")]
    public class EggPoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;
        private string? _lastInitialLetter; // Keep this class-level field

        private string GetPokemonEggInitialLetter(T pokemon)
        {
            return pokemon.FileName[0].ToString().ToUpper(); // Assuming the Pokémon's name is accessible via a Name property
        }

        [Command("mysteryegg")]
        [Alias("me")]
        [Summary("Gives a random Pokémon from the EggTrade Pool.")]
        [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
        public async Task MysteryPokemonAsync()
        {


            var eggTradePool = Info.Hub.LedyEgg.EggTradePool;
            if (eggTradePool.Count == 0)
            {
                var eggTradePoolEmpty = $"The EggTrade Pool is empty.";
                var embedEggtradePoolEmpty = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = Context.User.Username,
                        IconUrl = Context.User.GetAvatarUrl()
                    },
                    Color = Color.Blue
                }
                .WithDescription(eggTradePoolEmpty)
                .WithThumbnailUrl("https://sysbots.net/wp-content/uploads/2023/09/logosys.png")
                .WithCurrentTimestamp()
                    .Build();
                await ReplyAsync(null, false, embedEggtradePoolEmpty).ConfigureAwait(false);
                return;
            }

            T pk;
            List<T> filteredPool = eggTradePool.Where(p => GetPokemonEggInitialLetter(p) != _lastInitialLetter).ToList();

            if (filteredPool.Count == 0) // fallback to the complete pool if the filtered list is empty
            {
                filteredPool = eggTradePool;
            }

            var randomIndex = new Random().Next(filteredPool.Count);
            pk = filteredPool[randomIndex];

            _lastInitialLetter = GetPokemonEggInitialLetter(pk); // Update the last initial letter

            var code = Info.GetRandomTradeCode();
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToMysteryEggQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Mystery, Context.User).ConfigureAwait(false);

        }

    }
}