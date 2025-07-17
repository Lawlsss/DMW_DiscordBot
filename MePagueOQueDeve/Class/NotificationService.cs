using Discord;
using Discord.WebSocket;
using MePagueOQueDeve.Class.Objects;

namespace MePagueOQueDeve.Class
{
	public class NotificationService
	{
		private readonly DiscordSocketClient _client;
		private Timer _timer;
		private readonly AudioService _service;

		public NotificationService(DiscordSocketClient client)
		{
			_client = client;
			_service = new AudioService();
		}

		public void StartTimer()
		{
			//Making sure that the timer starts without any second's delay
			var currentTime = DateTime.Now;
			var nextMinute = currentTime.AddMinutes(1).AddSeconds(-currentTime.Second);
			var timeToNextMinute = nextMinute - currentTime;
			_timer = new Timer(ExecuteBackgroundTask, null, timeToNextMinute, TimeSpan.FromMinutes(1));
		}
			
		public async void ExecuteBackgroundTask(object state)
		{
			try
			{
				var notifications = CRUD.ListNotificationsReadyRun();

				if (notifications.Count > 0)
				{
					List<ulong> UniqueGuilds = notifications.Select(x => x.guild_id).Distinct().ToList();

					if (UniqueGuilds.Count == 0) throw new Exception("Erro ao separar as Guilds");

					foreach(ulong guild_id in UniqueGuilds)
					{
						Thread thread = new Thread(() => RunNotifications(guild_id, notifications.FindAll(x => x.guild_id == guild_id)));
						thread.Start();
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public async Task RunNotifications(ulong guildId, List<NotificationObj> notifications)
		{
			var guild = _client.GetGuild(guildId);
			if (guild == null) return;

			foreach(var notification in notifications)
			{
				string soundExe = CRUD.SoundExe(guildId, notification.sound);
				if (string.IsNullOrEmpty(soundExe)) continue;
				string soundPath = GlobalSettings.SoundPath + "\\" + soundExe;
				foreach (var channel in guild.VoiceChannels)
				{
					if (channel.ConnectedUsers.Count == 0) continue;
					try { await _service.SendAudioAsync(guild, null, channel, soundPath); } catch { }
				}

				CRUD.SetLastRunNotification(guildId, notification.id);
			}
		}
	}
}
