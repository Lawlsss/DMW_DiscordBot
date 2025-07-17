namespace MePagueOQueDeve.Class.Objects
{
	public class LobbyObj
	{
		public ulong OwnerId { get; set; }
		public string OwnerName { get; set; }
		public ulong GuildId { get; set; }
		public List<ulong> Members { get; set; } = new List<ulong>();
		public string LobbyName { get; set; }

		public void AddMember(ulong memberId)
		{
			if (!Members.Contains(memberId))
			{
				Members.Add(memberId);
			}
		}
		public void RemoveMember(ulong memberId)
		{
			if (Members.Contains(memberId))
			{
				Members.Remove(memberId);
			}
		}
	}
}
