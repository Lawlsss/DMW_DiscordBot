using Discord;
using Discord.Commands;
using Discord.WebSocket;
using MePagueOQueDeve.Class;
using MePagueOQueDeve.Class.Objects;

namespace MePagueOQueDeve.Commands.Sound
{
	public class SoundCommand : ModuleBase<SocketCommandContext>
	{
		private readonly AudioService _service;

		public SoundCommand(AudioService service)
		{
			_service = service;
		}

		[Command("sonsInfo")]
		public async Task SonsInfo()
		{
			var sounds = CRUD.ListSounds(Context.Guild.Id);
			if (sounds.Count == 0)
			{
				await ReplyAsync("Não existem sons.");
				return;
			}

			var embed = new EmbedBuilder()
			.WithTitle("Listagem Sons")
			.WithDescription(SoundTable(sounds))
			.WithColor(Color.Orange)
			.WithThumbnailUrl(GlobalSettings.Logo)
			.Build();

			await ReplyAsync(embed: embed);
		}

		[Command("somAdicionar")]
		public async Task SonsAdicionar(string nome = null)
		{
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync($"Exemplo: !somAdicionar <nome do som>");
				return;
			}
			if(!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}
			if (CRUD.SoundNameUse(Context.Guild.Id, nome))
			{
				await ReplyAsync($"O nome já se encontra em uso!");
				return;
			}

			var attachs = Context.Message.Attachments;
			var mp3 = attachs.FirstOrDefault(a => a.Filename.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase));
			if (attachs.Count == 0 || mp3 == null)
			{
				await ReplyAsync($"Precisa de dar attach num mp3 para o som!");
				return;
			}
			string filename = $"{Guid.NewGuid().ToString()}.mp3";
			string dirPath = GlobalSettings.SoundPath;
			Directory.CreateDirectory(dirPath);
			var path = Path.Combine(GlobalSettings.SoundPath, filename);

			using (HttpClient client = new HttpClient())
			{
				try
				{
					byte[] fileBytes = await client.GetByteArrayAsync(mp3.Url);
					await File.WriteAllBytesAsync(path, fileBytes);

					CRUD.CreateSound(Context.Guild.Id, nome, filename);
					await ReplyAsync("Som adicionado com sucesso! para previsualizar faça !somplay < nome >");
				}
				catch(Exception ex)
				{
					await ReplyAsync($"Erro ao baixar o arquivo: {ex.Message}");
				}
			}
		}

		[Command("somRemover")]
		public async Task RemoverSom(string nome = null)
		{
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync($"Exemplo: !somRemover <nome do som>");
				return;
			}
			if (!CRUD.isUserAdmin(Context.Guild.Id, Context.User.Id))
			{
				await ReplyAsync($"Não tem permissões suficientes!");
				return;
			}

			string soundExe = CRUD.SoundExe(Context.Guild.Id, nome);
			if (!string.IsNullOrEmpty(soundExe))
			{
				string soundPath = GlobalSettings.SoundPath + "\\" + soundExe;

				if (File.Exists(soundPath)) File.Delete(soundPath);
			}
			CRUD.RemoveSound(Context.Guild.Id, nome);

			await ReplyAsync("Som removido com sucesso!");
		}

		[Command("somplay", RunMode = RunMode.Async)]
		public async Task SomPlay(string nome)
		{
			if (string.IsNullOrEmpty(nome))
			{
				await ReplyAsync($"Exemplo: !somAdicionar <nome do som>");
				return;
			}
			var user = Context.User as SocketGuildUser;
			if(user == null || user.VoiceChannel == null)
			{
				await ReplyAsync($"Precisa de estar num voice channel!");
				return;
			}

			string soundExe = CRUD.SoundExe(Context.Guild.Id, nome);
			if (string.IsNullOrEmpty(soundExe))
			{
				await ReplyAsync("Som inexistente.");
				return;
			}
			string soundPath = GlobalSettings.SoundPath + "\\" + soundExe;
			if (!File.Exists(soundPath))
			{
				await ReplyAsync("Erro ao localizar o mp3. Por favor deia upload com o comando !somupload < nome >");
				return;
			}

			await _service.SendAudioAsync(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, soundPath);
		}

		[Command("somRandom", RunMode = RunMode.Async)]
		public async Task SomRandom()
		{
			var user = Context.User as SocketGuildUser;
			if (user == null || user.VoiceChannel == null)
			{
				await ReplyAsync($"Precisa de estar num voice channel!");
				return;
			}
			string soundExe = CRUD.SoundExeRandom(Context.Guild.Id);
			if (string.IsNullOrEmpty(soundExe))
			{
				await ReplyAsync("Som inexistente.");
				return;
			}
			string soundPath = GlobalSettings.SoundPath + "\\" + soundExe;
			if (!File.Exists(soundPath))
			{
				await ReplyAsync("Erro ao localizar o mp3. Por favor deia upload com o comando !somupload < nome >");
				return;
			}

			await _service.SendAudioAsync(Context.Guild, Context.Channel, (Context.User as IVoiceState).VoiceChannel, soundPath);
		}

		private static string SoundTable(List<SoundObj> sounds)
		{
			string tabela = string.Empty;
			string header = "| ID | Nome | exe |";
			tabela = "```\n" + header + "\n" + new string('-', header.Length);

			foreach (var sound in sounds)
			{
				string row = $"| {sound.id} | {sound.name} | {sound.exe} |";
				tabela += "\n" + row;
			}

			return tabela;
		}
	}
}
