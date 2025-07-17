using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MePagueOQueDeve.Class;

namespace MePagueOQueDeve
{
	public class Bot: IBot
	{
		private ServiceProvider? _serviceProvider;

		private readonly IConfiguration _configuration;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public Bot(IConfiguration configuration)
		{
			_configuration = configuration;

			DiscordSocketConfig config = new()
			{
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
			};

			_client = new DiscordSocketClient(config);
			_commands = new CommandService();
		}

		public async Task StartAsync(ServiceProvider services)
		{
			string discordToken = GlobalSettings.DiscordToken ?? throw new Exception("Missing Discord token");

			_serviceProvider = services;

			await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

			await _client.LoginAsync(TokenType.Bot, discordToken);
			await _client.StartAsync();

			_client.MessageReceived += HandleCommandAsync;
			_client.Ready += Client_Ready;
		}

		public async Task StopAsync()
		{
			if (_client != null)
			{
				await _client.LogoutAsync();
				await _client.StopAsync();
			}
		}

		private async Task HandleCommandAsync(SocketMessage arg)
		{
			if (arg is not SocketUserMessage message || message.Author.IsBot) return;

			int position = 0;
			bool messageIsCommand = message.HasCharPrefix('!', ref position);
			var channel = message.Channel as SocketGuildChannel;
			if (messageIsCommand && channel != null)
			{
				if (!CRUD.isServerUp())
				{
					await message.Channel.SendMessageAsync("O Banco de dados encontra-se offline.");
					return;
				}
				if (!CRUD.isGuildAuth(channel.Guild.Id))
				{
					await message.Channel.SendMessageAsync("Só pode usar comandos em servidores autorizados.");
					return;
				}

				await _commands.ExecuteAsync(
					new SocketCommandContext(_client, message),
					position,
					_serviceProvider);

				return;
			}
		}

		private async Task Client_Ready()
		{
			NotificationService notificationService = new(_client);
			notificationService.StartTimer();
		}
	}
}
