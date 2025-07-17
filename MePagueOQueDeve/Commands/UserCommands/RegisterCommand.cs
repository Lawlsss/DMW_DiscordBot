using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MePagueOQueDeve.Class;

namespace MePagueOQueDeve.Commands.UserCommands
{
	public class RegisterCommand : ModuleBase<SocketCommandContext>
	{
		[Command("registrar")]
		[Summary("")]
		public async Task ExecuteAsync(string ingame_name = null)
		{
			if (string.IsNullOrEmpty(ingame_name))
			{
				await ReplyAsync($"Exemplo: !registrar <nome dentro do jogo>");
				return;
			}

			if(CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Já se encontra registrado.");
				return;
			}

			CRUD.RegisterUser(Context.Guild.Id, Context.User.Id, Context.User.GlobalName, ingame_name);
			await ReplyAsync($"{ingame_name} registrado com sucesso!");

			try
			{
				var user = Context.User as SocketGuildUser;
				if (user == null) throw new Exception("Erro ao aceder o utilizador");
				await user.ModifyAsync(properties => properties.Nickname = ingame_name);
			}
			catch(Exception ex)
			{
				await ReplyAsync($"Ocorreu um erro ao tentar mudar o apelido: {ex.Message}");
			}
		}
	}
}
