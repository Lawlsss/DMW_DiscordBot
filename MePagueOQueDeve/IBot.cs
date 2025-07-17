using Microsoft.Extensions.DependencyInjection;

namespace MePagueOQueDeve
{
	public interface IBot
	{
		Task StartAsync(ServiceProvider services);

		Task StopAsync();
	}
}
