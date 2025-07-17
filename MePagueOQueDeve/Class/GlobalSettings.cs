using Newtonsoft.Json;
using System.Reflection;

namespace MePagueOQueDeve.Class
{
	public static class GlobalSettings
	{
		public static string SoundPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Sounds";
		public static string Logo;
		public static string DiscordToken;
		public static int NotificationTimer;
		public static string MySQLServer;
		public static string MySQLUser;
		public static string MySQLPassword;
		public static string MySQLSchema;


		public static void LoadSettings()
		{
			string JsonPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\config.json";
			if (File.Exists(JsonPath))
			{
				string json = File.ReadAllText(JsonPath);
				var config = JsonConvert.DeserializeObject<JsonConfig>(json);
				if (config == null) throw new Exception("config.json format wrong!");

				Logo = config.Logo;
				DiscordToken = config.DiscordToken;
				NotificationTimer = config.NotificationTimer;
				MySQLServer = config.MySQLServer;
				MySQLUser = config.MySQLUser;
				MySQLPassword = config.MySQLPassword;
				MySQLSchema = config.MySQLSchema;
			}
			else
			{
				throw new Exception("config.json missing!");
			}
		}

		private class JsonConfig
		{
			public string Logo { get; set; }
			public string DiscordToken { get; set; }
			public int NotificationTimer { get; set; }
			public string MySQLServer { get; set; }
			public string MySQLUser { get; set; }
			public string MySQLPassword { get; set; }
			public string MySQLSchema { get; set; }
		}
	}
}
