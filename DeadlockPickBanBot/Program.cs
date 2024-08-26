using DeadlockPickBanBot.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// var botToken = builder.Configuration["BotToken"];
var botToken = "7091543006:AAHiJQiOz6KoqBdroX6KrFF6BMZcmQ4SZFc";
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
    new TelegramBotClient(botToken));

builder.Services.AddSingleton<BotLongPollingService>();
builder.Services.AddSingleton<UpdateHandlerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var telegramBotService = app.Services.GetRequiredService<BotLongPollingService>();
var cancellationToken = new CancellationToken();
await telegramBotService.StartAsync(cancellationToken: cancellationToken);

app.UseAuthorization();

app.MapControllers();

app.Run();