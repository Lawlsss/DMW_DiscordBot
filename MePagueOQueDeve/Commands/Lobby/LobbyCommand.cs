using Discord;
using Discord.Commands;
using MePagueOQueDeve.Class;
using MePagueOQueDeve.Class.Objects;
using System.Reactive;

namespace MePagueOQueDeve.Commands.Lobby
{
	public class LobbyCommand : ModuleBase<SocketCommandContext>
	{
		private static Dictionary<string, LobbyObj> _lobbies;
		public LobbyCommand() { if(_lobbies == null) _lobbies = new Dictionary<string, LobbyObj>(); }

		[Command("crialobby")]
		public async Task CriarLobby(string nome = null)
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync($"Exemplo: !criarlobby < nome >");
				return;
			}
			if (_lobbies.ContainsKey(nome))
			{
				await ReplyAsync($"Lobby '{nome}' já existe!");
				return;
			}

			var lobby = new LobbyObj
			{
				OwnerId = Context.User.Id,
				OwnerName = Context.User.GlobalName,
				GuildId = Context.Guild.Id,
				LobbyName = nome
			};
			lobby.Members.Add(Context.User.Id);
			_lobbies[nome] = lobby;
			await ReplyAsync($"Lobby '{nome}' criado com sucesso!");
		}

		[Command("fecharlobby")]
		public async Task FecharLobby(string nome = null)
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync("Por favor, forneça o nome do lobby que deseja fechar. Exemplo: !fecharlobby < nome >");
				return;
			}
			if (!_lobbies.ContainsKey(nome))
			{
				await ReplyAsync($"Não existe um lobby com o nome '{nome}'.");
				return;
			}
			var lobby = _lobbies[nome];
			if (lobby.OwnerId != Context.User.Id && !CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não tem permissão para fechar o lobby, pois você não é o criador.");
				return;
			}
			_lobbies.Remove(nome);
			await ReplyAsync($"Lobby '{nome}' foi fechado com sucesso.");
		}

		[Command("entrarlobby")]
		public async Task EntrarLobby(string nome = null)
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync("Por favor, forneça o nome do lobby que deseja entrar. Exemplo: !entrarlobby < nome >");
				return;
			}
			if (!_lobbies.ContainsKey(nome))
			{
				await ReplyAsync($"Não existe um lobby com o nome '{nome}'.");
				return;
			}
			var lobby = _lobbies[nome];
			if (lobby.Members.Contains(Context.User.Id))
			{
				await ReplyAsync($"Já faz parte do lobby '{lobby.LobbyName}'.");
				return;
			}
			lobby.Members.Add(Context.User.Id);
			await ReplyAsync($"User {Context.User.GlobalName} adicionado ao lobby '{nome}'.");
		}

		[Command("sairlobby")]
		public async Task SairLobby(string nome = null)
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync("Por favor, forneça o nome do lobby que deseja sair. Exemplo: !entrarlobby < nome >");
				return;
			}
			if (!_lobbies.ContainsKey(nome))
			{
				await ReplyAsync($"Não existe um lobby com o nome '{nome}'.");
				return;
			}
			var lobby = _lobbies[nome];
			if(lobby.OwnerId == Context.User.Id)
			{
				await ReplyAsync("Não pode sair do seu próprio lobby. Para fechar: !fecharlobby < nome >");
				return;
			}
			if (!lobby.Members.Contains(Context.User.Id))
			{
				await ReplyAsync($"Não faz parte do lobby '{lobby.LobbyName}'.");
				return;
			}
			lobby.Members.Remove(Context.User.Id);
			await ReplyAsync($"User {Context.User.GlobalName} removido do lobby '{nome}'.");
		}

		[Command("listalobby")]
		public async Task ListaLobby()
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}

			var embed = new EmbedBuilder()
			.WithTitle("Listagem Lobby/s")
			.WithDescription(LobbyTable(Context.Guild.Id))
			.WithColor(Color.Orange)
			.WithThumbnailUrl(GlobalSettings.Logo)
			.Build();

			await ReplyAsync(embed: embed);
		}

		[Command("membroslobby")]
		public async Task MembrosLobby(string nome = null)
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync("Por favor, forneça o nome do lobby que deseja listar. Exemplo: !membroslobby < nome >");
				return;
			}
			if (!_lobbies.ContainsKey(nome))
			{
				await ReplyAsync($"Não existe um lobby com o nome '{nome}'.");
				return;
			}

			var embed = new EmbedBuilder()
			.WithTitle("Listagem Membro/s")
			.WithDescription(MembrosTable(nome, Context.Guild.Id))
			.WithColor(Color.Orange)
			.WithThumbnailUrl(GlobalSettings.Logo)
			.Build();

			await ReplyAsync(embed: embed);
		}


		private string LobbyTable(ulong guildId)
		{
			string tabela = string.Empty;
			string header = "| Lobby | Owner | Membros |";
			tabela = "```\n" + header + "\n" + new string('-', header.Length);

			foreach (var lobby in _lobbies.Values.Where(lobby => lobby.GuildId == guildId).ToList())
			{
				string row = $"| {lobby.LobbyName} | {lobby.OwnerName} | {lobby.Members.Count} |";
				tabela += "\n" + row;
			}

			return tabela;
		}

		private string MembrosTable(string lobbyName, ulong guildId)
		{
			string tabela = string.Empty;
			string header = "| MembroId | Membro |";
			tabela = "```\n" + header + "\n" + new string('-', header.Length);

			foreach (var memberId in _lobbies[lobbyName].Members)
			{

				string row = $"| {memberId} | {CRUD.IngameUserName(guildId, memberId)} |";
				tabela += "\n" + row;
			}

			return tabela;
		}
	}
}
