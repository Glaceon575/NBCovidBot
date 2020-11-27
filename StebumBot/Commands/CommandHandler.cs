﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StebumBot.Commands
{
    public class CommandHandler : IDisposable
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public CommandService Commands { get; }

        public CommandHandler(
            ILogger<CommandHandler> logger,
            IConfiguration configuration,
            DiscordSocketClient client,
            CommandService commands,
            IServiceProvider services)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _services = services;

            Commands = commands;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public void Dispose()
        {
            _client.MessageReceived -= HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            _logger.LogTrace("Received message: " + message);

            if (!(message is SocketUserMessage userMessage)) return;

            var argPos = 0;

            var prefix = _configuration["commands:prefix"];

            if (string.IsNullOrWhiteSpace(prefix)) return;

            if (!userMessage.HasStringPrefix(prefix, ref argPos) || message.Author.IsBot)
                return;
            
            var context = new SocketCommandContext(_client, userMessage);

            await Commands.ExecuteAsync(context, argPos, _services);
        }
    }
}
