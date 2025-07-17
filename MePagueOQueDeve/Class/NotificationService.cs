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
						var GuildNotifications = notifications.FindAll(x => x.guild_id == guild_id);
						if (GuildNotifications.Count == 0) continue;

						//#TODO right now lastrun is set on voice method, should change it so it is only set after running voice and message
						Thread threadVoice = new Thread(() => RunVoiceNotifications(guild_id, GuildNotifications));
						threadVoice.Start();

						Thread threadMessage = new Thread(() => RunMessageNotifications(guild_id, GuildNotifications));
						threadMessage.Start();
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public async Task RunVoiceNotifications(ulong guildId, List<NotificationObj> notifications)
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

		public async Task RunMessageNotifications(ulong guildId, List<NotificationObj> notifications)
		{
			var guild = _client.GetGuild(guildId);
			if (guild == null) return;

			bool notifyActive = true;
			var userIds = CRUD.ListUsersNotify(guildId, notifyActive);
			if (userIds.Count == 0) return;

			string mentions = string.Empty;
			foreach(ulong userId in userIds)
			{
				var user = guild.GetUser(userId);
				if (user == null) continue;
				mentions += $"<@{user.Id}>";
			}
			if (string.IsNullOrEmpty(mentions)) return;

			//#TODO add bot-notify name to GlobalSettings, so it's configurable
			var channel = guild.TextChannels.FirstOrDefault(x => x.Name == "bot-notify");

			//#TODO add method to create channel?, or atleast notify server owner that one doesnt exist
			if (channel == null) return;

			foreach(var notification in notifications)
			{
				await channel.SendMessageAsync($"Atenção o {notification.name} vai começar em breve! \n ||{mentions}||");
			}
		}
	}
}
