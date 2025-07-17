namespace MePagueOQueDeve.Class.Objects
{
	public class NotificationObj
	{
		public int id { get; set; }
		public ulong guild_id { get; set; }
		public string name { get; set; }
		public TimeSpan time { get; set; }
		public bool active { get; set; }
		public string sound { get; set; }
	}
}
