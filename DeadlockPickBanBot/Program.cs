using DeadlockPickBanBot.Models;
using DeadlockPickBanBot.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

var botConfigSection = builder.Configuration.GetSection("BotConfiguration");
builder.Services.Configure<BotConfiguration>(botConfigSection);
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient<ITelegramBotClient>(
    httpClient => new TelegramBotClient(botConfigSection.Get<BotConfiguration>()!.BotToken, httpClient));
builder.Services.AddSingleton<UpdateHandler>();
builder.Services.ConfigureTelegramBotMvc();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();