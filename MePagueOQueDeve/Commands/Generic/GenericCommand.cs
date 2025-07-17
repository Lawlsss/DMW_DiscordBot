using Discord.Commands;

namespace MePagueOQueDeve.Commands.Generic
{
	public class GenericCommand : ModuleBase<SocketCommandContext>
	{
		[Command("pague")]
		[Summary("")]
		public async Task ExecuteAsync()
		{
			await ReplyAsync("BLZIN ME PAGUE O QUE DEVE!");
		}
	}
}
