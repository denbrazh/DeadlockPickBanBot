using Telegram.Bot;

namespace DeadlockPickBanBot.Services;

public interface IBotLongPollingService
{
    Task StartAsync(CancellationToken cancellationToken);
}

public class BotLongPollingService(ITelegramBotClient botClient, UpdateHandlerService updateHandler): IBotLongPollingService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var offset = 0;
        
        while (true)
        {
            try
            {
                var updates = await botClient.GetUpdatesAsync(offset);
                foreach (var update in updates)
                {
                    await updateHandler.HandleUpdateAsync(update, botClient);
                    offset = update.Id + 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving updates: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1)); // Пауза перед следующим запросом
        }
    }
}