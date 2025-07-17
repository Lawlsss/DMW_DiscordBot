using Discord;
using Discord.Commands;
using MePagueOQueDeve.Class;
using MePagueOQueDeve.Class.Objects;

namespace MePagueOQueDeve.Commands
{
	public class NotificationsCommand : ModuleBase<SocketCommandContext>
	{
		[Command("listanotificacoes")]
		public async Task NotificacoesInfo()
		{
			var notifications = CRUD.ListNotifications(Context.Guild.Id);
			if(notifications.Count == 0)
			{
				await ReplyAsync("Não existem notificações.");
				return;
			}

			var embed = new EmbedBuilder()
			.WithTitle("Listagem Notificações")
			.WithDescription(NotifyTable(notifications))
			.WithColor(Color.Orange)
			.WithThumbnailUrl(GlobalSettings.Logo)
			.Build();

			await ReplyAsync(embed: embed);
		}

		[Command("notificacoes")]
		public async Task NotificacoesAsync(){
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}

			bool notify = CRUD.Notifications(Context.Guild.Id, Context.User.Id);

			if (notify)
			{
				await ReplyAsync("Tem as notificações ativas, para desativar digite: !notificacoesOff");
			} else {
				await ReplyAsync("Tem as notificações desativas, para ativar digite: !notificacoesOn");
			}
		}

		[Command("notificacoesOn")]
		public async Task NotificacoesOnAsync()
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}

			CRUD.ToggleUserNotifications(Context.Guild.Id, Context.User.Id, true);
			await ReplyAsync("Ativou as suas notificações!");
		}

		[Command("notificacoesOff")]
		public async Task NotificacoesOffAsync()
		{
			if (!CRUD.isUserRegistered(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync("Não se encontra registrado. Digite: !registrar <nome dentro do jogo>");
				return;
			}

			CRUD.ToggleUserNotifications(Context.Guild.Id, Context.User.Id, true);
			await ReplyAsync("Desativou as suas notificações!");
		}

		[Command("notificacoesAdicionar")]
		public async Task AdicionarNotificacao(string nome = null, string horasStr = null, string som = null)
		{
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}
			if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(horasStr) || string.IsNullOrEmpty(som))
			{
				await ReplyAsync($"Exemplo: !notificacoesAdicionar <nome> <horas ex: 18:00> <nome som>");
				return;
			}

			TimeSpan horas = TimeSpan.Zero;
			if (TimeSpan.TryParse(horasStr, out horas)){ }
			else
			{
				await ReplyAsync($"Exemplo: !notificacoesAdicionar <nome> <horas ex: 18:00> <nome som>");
				return;
			}

			int soundId = CRUD.SoundId(Context.Guild.Id, som);
			if (!CRUD.SoundNameUse(Context.Guild.Id, som) || soundId == 0)
			{
				await ReplyAsync($"O som: {som} não existe!");
				return;
			}

			CRUD.CreateNotification(Context.Guild.Id, nome, horas, soundId);
			await ReplyAsync("Notificação criada com sucesso!");
		}

		[Command("notificacoesRemover")]
		public async Task RemoverNotificacao(string idStr)
		{
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}
			if (string.IsNullOrEmpty(idStr))
			{
				await ReplyAsync($"Exemplo: !notificacoesRemover < id >");
				return;
			}
			int id = 0;
			if(Int32.TryParse(idStr, out id)) { }
			else
			{
				await ReplyAsync($"Exemplo: !notificacoesRemover < id >");
				return;
			}

			CRUD.RemoveNotification(Context.Guild.Id, id);
			await ReplyAsync("Notificação removida com sucesso!");
		}

		[Command("ativanotificacao")]
		public async Task AtivarNotification(string idStr)
		{
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}
			if (string.IsNullOrEmpty(idStr))
			{
				await ReplyAsync($"Exemplo: !ativarnotificacao < id >");
				return;
			}
			int id = 0;
			if (Int32.TryParse(idStr, out id)) { }
			else
			{
				await ReplyAsync($"Exemplo: !ativarnotificacao < id >");
				return;
			}

			CRUD.ToggleNotification(Context.Guild.Id, id, true);
			await ReplyAsync("Notificação ativada com sucesso!");
		}

		[Command("desativanotificacao")]
		public async Task DesativarNotification(string idStr)
		{
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}
			if (string.IsNullOrEmpty(idStr))
			{
				await ReplyAsync($"Exemplo: !desativarnotificacao < id >");
				return;
			}
			int id = 0;
			if (Int32.TryParse(idStr, out id)) { }
			else
			{
				await ReplyAsync($"Exemplo: !desativarnotificacao < id >");
				return;
			}

			CRUD.ToggleNotification(Context.Guild.Id, id, false);
			await ReplyAsync("Notificação desativada com sucesso!");
		}

		[Command("notificacoesMudaAudio")]
		public async Task MudaAudioNotification(string idStr, string som)
		{
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}

			if (string.IsNullOrEmpty(idStr) || string.IsNullOrEmpty(som))
			{
				await ReplyAsync($"Exemplo: !notificacoesMudaAudio < id > < som >");
				return;
			}

			int id = 0;
			if (Int32.TryParse(idStr, out id)) { }
			else
			{
				await ReplyAsync($"Exemplo: !notificacoesMudaAudio < id > < som >");
				return;
			}
			int soundId = CRUD.SoundId(Context.Guild.Id, som);
			if (soundId == 0)
			{
				await ReplyAsync($"O som: {som} não existe!");
				return;
			}

			CRUD.ChangeAudioNotification(Context.Guild.Id, id, soundId);
			await ReplyAsync("Notificação atualizada com sucesso!");
		}

		private static string NotifyTable(List<NotificationObj> notifications)
		{
			string tabela = string.Empty;
			string header = "| ID | Nome | Horas | Som | Ativo |";
			tabela = "```\n" + header + "\n" + new string('-', header.Length);

			foreach(var notification in notifications)
			{
				string atv = "ativo";
				if (!notification.active) atv = "desativo";
				string row = $"| {notification.id} | {notification.name} | {notification.time.ToString(@"hh\:mm")} | {notification.sound} | {atv} |";
				tabela += "\n" + row;
			}

			return tabela;
		}
	}
}
