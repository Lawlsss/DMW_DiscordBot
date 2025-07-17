namespace MePagueOQueDeve.Class.Objects
{
	public class User
	{
		public ulong id { get; set; }
		public ulong guild_id { get; set; }
		public string name { get; set; }
		public string ingame_name { get; set; }
		public bool notifications { get; set; }
		public bool admin { get; set; }
	}
}
