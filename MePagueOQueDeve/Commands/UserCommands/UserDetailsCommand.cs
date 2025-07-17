using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MePagueOQueDeve.Class;

namespace MePagueOQueDeve.Commands.UserCommands
{
	public class UserDetailsCommand : ModuleBase<SocketCommandContext>
	{
		[Command("mudarnome")]
		[Summary("")]
		public async Task MudarNomeAsync(string ingame_name = null)
		{
			if (string.IsNullOrEmpty(ingame_name))
			{
				await ReplyAsync($"Exemplo: !mudarnome <nome dentro do jogo>");
				return;
			}

			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}

			CRUD.ChangeName(Context.Guild.Id, Context.User.Id, ingame_name);
			await ReplyAsync($"Nome mudado com sucesso!");

			try
			{
				var user = Context.User as SocketGuildUser;
				if (user == null) throw new Exception("Erro ao aceder o utilizador");
				await user.ModifyAsync(properties => properties.Nickname = ingame_name);
			}
			catch (Exception ex)
			{
				await ReplyAsync($"Ocorreu um erro ao tentar mudar o apelido: {ex.Message}");
			}
		}
	}
}
