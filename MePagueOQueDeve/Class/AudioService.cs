using Discord;
using Discord.Audio;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace MePagueOQueDeve.Class
{
	public class AudioService
	{
		public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, IVoiceChannel target, string path)
		{
			if (!File.Exists(path))
			{
				if(channel != null)
					await channel.SendMessageAsync("File does not exist.");
				return;
			}
			var audioClient = await target.ConnectAsync();
			using (var ffmpeg = CreateProcess(path))
			using (var stream = audioClient.CreatePCMStream(AudioApplication.Music))
			{
				try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
				catch (Exception ex) { Console.WriteLine(ex.Message); }
				finally { await stream.FlushAsync(); }
			}

			await audioClient.StopAsync();
		}

		private Process CreateProcess(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/ffmpeg.exe",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true
			});
		}
	}
}
