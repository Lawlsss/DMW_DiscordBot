﻿using MePagueOQueDeve.Class;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MePagueOQueDeve
{
    internal class Program
    {
		private static void Main(string[] args) =>
			MainAsync(args).GetAwaiter().GetResult();

		private static async Task MainAsync(string[] args)
        {
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets(Assembly.GetExecutingAssembly())
				.Build();

			var serviceProvider = new ServiceCollection()
				.AddSingleton<IConfiguration>(configuration)
				.AddSingleton<AudioService>()
				.AddScoped<IBot, Bot>()
				.BuildServiceProvider();

			try
			{
				GlobalSettings.LoadSettings();
				Directory.CreateDirectory(GlobalSettings.SoundPath);
				IBot bot = serviceProvider.GetRequiredService<IBot>();

				await bot.StartAsync(serviceProvider);

				Console.WriteLine("Connected to Discord");

				do
				{
					var keyInfo = Console.ReadKey();

					if (keyInfo.Key == ConsoleKey.Q)
					{
						Console.WriteLine("\nShutting down!");

						await bot.StopAsync();
						return;
					}
				} while (true);
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception.Message);
				Environment.Exit(-1);
			}
		}
	}
}
