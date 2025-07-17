using MePagueOQueDeve.Class.Objects;
using MePagueOQueDeve.DB;
using System;
using System.Data;

namespace MePagueOQueDeve.Class
{
	public static class CRUD
	{
		public static bool isServerUp()
		{
			bool up = false;
			MySQL mySQL = new MySQL();
			try 
			{
				if (mySQL.State() == ConnectionState.Open) up = true;
			}
			catch { up = false; }
			mySQL.Close();
			return up;
		}
		public static bool isGuildAuth(ulong guildId)
		{
			bool auth = false;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT ativo FROM Guilds WHERE id = '{guildId}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					auth = mySQL.ReadBool(lst.GetValue("ativo"));
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return auth;
		}

		public static bool isUserRegistered(ulong guildId, ulong userId)
		{
			bool registered = false;
			MySQL mySQL = new MySQL();

			try
			{
				string sql = $@"SELECT id FROM Users WHERE guild_id = '{guildId}' and id = '{userId}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					registered = true;
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return registered;
		}

		public static bool isUserAdmin(ulong guildId, ulong userId)
		{
			bool isAdmin = false;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT admin FROM Users WHERE guild_id = '{guildId}' AND id = '{userId}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					isAdmin = mySQL.ReadBool(lst.GetValue("admin"));
				}
				lst.Close();
			}
			catch { }
			return isAdmin;
		}

		public static void RegisterUser(ulong guildId, ulong userId, string user_name, string ingame_name)
		{
			if (string.IsNullOrEmpty(ingame_name)) return;
			if (string.IsNullOrEmpty(user_name)) return;

			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"INSERT INTO Users (id, guild_id, name, ingame_name)
							VALUES('{userId}', '{guildId}', '{user_name}', '{ingame_name}')";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static void ChangeName(ulong guildId, ulong userId, string new_name)
		{
			if (string.IsNullOrEmpty(new_name)) return;

			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"UPDATE Users SET ingame_name = '{new_name}' WHERE id = '{userId}' AND guild_id = '{guildId}'";

				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static bool Notifications(ulong guildId, ulong userId)
		{
			bool notify = false;
			MySQL mySQL = new MySQL();

			try
			{
				string sql = $"SELECT notifications FROM Users WHERE id = '{userId}' AND guild_id = '{guildId}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					notify = mySQL.ReadBool(lst.GetValue("notifications"));
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return notify;
		}

		public static void ToggleUserNotifications(ulong guildId, ulong userId, bool notify)
		{
			MySQL mySQL = new MySQL();
			try
			{
				int notifyInt = 0;
				if (notify) notifyInt = 1;
				string sql = $@"UPDATE Users SET notifications = {notifyInt} WHERE id = '{userId}' AND guild_id = '{guildId}'";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static List<NotificationObj> ListNotifications(ulong guildId)
		{
			List<NotificationObj> list = new List<NotificationObj>();	
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT
									nf.id,
									nf.name,
									nf.time,
									nf.active,
									s.name as soundname
								FROM Notifications nf
								LEFT JOIN Sounds s on s.id = nf.sound
								WHERE nf.guild_id = '{guildId}'
								ORDER by nf.time";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					while (lst.Read())
					{
						NotificationObj notification = new NotificationObj();
						notification.id = mySQL.ReadInt(lst.GetValue("id"));
						notification.name = mySQL.ReadString(lst.GetValue("name"));
						notification.time = mySQL.ReadTimeSpan(lst.GetValue("time"));
						notification.active = mySQL.ReadBool(lst.GetValue("active"));
						notification.sound = mySQL.ReadString(lst.GetValue("soundname"));

						list.Add(notification);
					}
				}
			}
			catch { }
			mySQL.Close();
			return list;
		}

		public static List<NotificationObj> ListNotifications(ulong guildId, bool active)
		{
			List<NotificationObj> list = new List<NotificationObj>();
			MySQL mySQL = new MySQL();
			try
			{
				int activeInt = 0;
				if (active) activeInt = 1;
				string sql = $@"SELECT
									nf.id,
									nf.name,
									nf.time,
									nf.active,
									s.name as soundname
								FROM Notifications nf
								LEFT JOIN Sounds s on s.id = nf.sound
								WHERE nf.guild_id = '{guildId}' AND nf.active = {active}
								ORDER by nf.time";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					while (lst.Read())
					{
						NotificationObj notification = new NotificationObj();
						notification.id = mySQL.ReadInt(lst.GetValue("id"));
						notification.name = mySQL.ReadString(lst.GetValue("name"));
						notification.time = mySQL.ReadTimeSpan(lst.GetValue("time"));
						notification.active = mySQL.ReadBool(lst.GetValue("active"));
						notification.sound = mySQL.ReadString(lst.GetValue("soundname"));

						list.Add(notification);
					}
				}
			}
			catch { }
			mySQL.Close();
			return list;
		}

		public static List<NotificationObj> ListNotificationsReadyRun()
		{
			List<NotificationObj> list = new List<NotificationObj>();
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT
									nf.id,
									nf.guild_id,
									nf.name,
									nf.time,
									nf.active,
									s.name as soundname
								FROM Notifications nf
								LEFT JOIN Sounds s on s.id = nf.sound
								WHERE CONCAT(DATE(CURDATE()), ' ', nf.time) BETWEEN DATE_SUB(NOW(), INTERVAL {GlobalSettings.NotificationTimer} MINUTE) AND DATE_ADD(NOW(), INTERVAL {GlobalSettings.NotificationTimer} MINUTE)
								AND (nf.lastRun IS NULL OR nf.lastRun NOT BETWEEN DATE_SUB(NOW(), INTERVAL {GlobalSettings.NotificationTimer} MINUTE) AND DATE_ADD(NOW(), INTERVAL {GlobalSettings.NotificationTimer} MINUTE))
								AND CONCAT(DATE(CURDATE()), ' ', nf.time) > NOW()";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					while (lst.Read())
					{
						NotificationObj notification = new NotificationObj();
						notification.id = mySQL.ReadInt(lst.GetValue("id"));
						notification.guild_id = ulong.Parse(mySQL.ReadString(lst.GetValue("guild_id")));
						notification.name = mySQL.ReadString(lst.GetValue("name"));
						notification.time = mySQL.ReadTimeSpan(lst.GetValue("time"));
						notification.active = mySQL.ReadBool(lst.GetValue("active"));
						notification.sound = mySQL.ReadString(lst.GetValue("soundname"));

						list.Add(notification);
					}
				}
			}
			catch { }
			mySQL.Close();
			return list;

		}

		public static void CreateNotification(ulong guildId, string name, TimeSpan horas, int soundId)
		{
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"INSERT INTO Notifications (guild_id, name, time, sound)
								VALUES ('{guildId}', '{name}', '{horas.ToString(@"hh\:mm")}', {soundId})";

				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}
		
		public static void RemoveNotification(ulong guildId, int id)
		{
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"DELETE FROM Notifications WHERE guild_id = '{guildId}' AND id = '{id}'";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}
	
		public static void ToggleNotification(ulong guildId, int id, bool active)
		{
			MySQL mySQL = new MySQL();
			try
			{
				int activeInt = 0;
				if (active) activeInt = 1;
				string sql = $"UPDATE Notifications SET active = {activeInt} WHERE guild_id = '{guildId}' AND id = {id}";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static void ChangeAudioNotification(ulong guildId, int id, int soundId)
		{
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $"UPDATE Notifications SET sound = {soundId} WHERE guild_id = '{guildId}' AND id = {id}";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static void SetLastRunNotification(ulong guildId, int id)
		{
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $"UPDATE Notifications SET lastRun = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE id = {id} and guild_id = '{guildId}'";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static List<SoundObj> ListSounds(ulong guildId)
		{
			List<SoundObj> list = new List<SoundObj>();
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT
									id,
									name,
									exe
								FROM Sounds	
								WHERE guild_id = '{guildId}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					while (lst.Read())
					{
						SoundObj sound = new SoundObj();
						sound.id = mySQL.ReadInt(lst.GetValue("id"));
						sound.name = mySQL.ReadString(lst.GetValue("name"));
						sound.exe = mySQL.ReadString(lst.GetValue("exe"));

						list.Add(sound);
					}
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return list;
		}

		public static void CreateSound(ulong guildId, string name, string exe)
		{
			MySQL mySQL = new MySQL();
			try
			{
				if (string.IsNullOrEmpty(name)) throw new Exception("");
				if (string.IsNullOrEmpty(exe)) throw new Exception("");
				string sql = $@"INSERT INTO Sounds (guild_id, name, exe) VALUES('{guildId}', '{name}', '{exe}')";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static void RemoveSound(ulong guildId, string name)
		{
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"DELETE FROM Sounds WHERE guild_id = '{guildId}' AND name = '{name}'";
				mySQL.ExecuteCommand(sql);
			}
			catch { }
			mySQL.Close();
		}

		public static bool SoundNameUse(ulong guildId, string name)
		{
			bool inUse = false;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT id FROM Sounds WHERE guild_id = '{guildId}' AND name = '{name}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					inUse = true;
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return inUse;
		}

		public static int SoundId(ulong guildId, string name)
		{
			int Id = 0;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT id FROM Sounds WHERE guild_id = '{guildId}' AND name = '{name}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					Id = mySQL.ReadInt(lst.GetValue("id"));
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return Id;
		}

		public static string SoundExe(ulong guildId, string name)
		{
			string exe = string.Empty;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT exe FROM Sounds WHERE guild_id = '{guildId}' AND name = '{name}'";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					exe = mySQL.ReadString(lst.GetValue("exe"));
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return exe;
		}

		public static string SoundExeRandom(ulong guildId)
		{
			string exe = string.Empty;
			MySQL mySQL = new MySQL();
			try
			{
				string sql = $@"SELECT exe FROM Sounds WHERE guild_id = '{guildId}' ORDER BY RAND() LIMIT 1;";
				var lst = mySQL.ReadData(sql);
				if (lst.HasRows)
				{
					lst.Read();
					exe = mySQL.ReadString(lst.GetValue("exe"));
				}
				lst.Close();
			}
			catch { }
			mySQL.Close();
			return exe;
		}
	}
}
