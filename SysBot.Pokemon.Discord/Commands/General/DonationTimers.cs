using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace SysBot.Pokemon.Discord.Commands.General
{
    public class DonationTimer : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private Timer? _myTimer; // Make _myTimer nullable

        public DonationTimer(DiscordSocketClient client)
        {
            _client = client;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            // Set up the timer for 30 minutes
            _myTimer = new Timer(30 * 60 * 1000); // 30 minutes in milliseconds
            _myTimer.Elapsed += OnTimedEvent;
            _myTimer.AutoReset = true;
            _myTimer.Enabled = true;
        }

        private async void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            await SendMessageToChannel();
        }

        public async Task SendMessageToChannel()
        {
            var str = $"Here's the donation link! Thank you for your support :3 {SysCordSettings.Settings.DonationLink}";
            await ReplyAsync(str).ConfigureAwait(false);
        }
    }
}
