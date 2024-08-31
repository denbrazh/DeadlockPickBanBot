using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DeadlockPickBanBot.Services.MessageServices;

public class OnMessageHandler(ITelegramBotClient bot, ILogger<OnMessageHandler> logger)
{
    private static string lobbyStage = "Registration";
    // private static string LobbyStage = "Registration";
    
    private static List<string> team1 = new List<string>();
    private static List<string> team2 = new List<string>();
    private static string Command1Name = "Breaking Bad";
    private static string Command2Name = "6 SexHunters";
    public async Task OnMessage(Message msg)
    {
        logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
            return;
        Message sentMessage;
        if (lobbyStage == "Registration")
        {
            sentMessage = await RegistrationStage(msg);
            
        }
        else
        {
            sentMessage = await (messageText.Split(' ')[0] switch
            {
                // "/photo" => SendPhoto(msg),
                // "/inline_buttons" => SendInlineKeyboard(msg),
                // "/keyboard" => SendReplyKeyboard(msg),
                // "/remove" => RemoveKeyboard(msg),
                // "/request" => RequestContactAndLocation(msg),
                // "/inline_mode" => StartInlineQuery(msg),
                // "/poll" => SendPoll(msg),
                // "/poll_anonymous" => SendAnonymousPoll(msg),
                // "/throw" => FailingHandler(msg),
                _ => Usage(msg)
            });
        }
        
        
        logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);
    }

    private async Task<Message> Usage(Message msg)
    {
        const string usage = """
                                 <b><u>Bot menu</u></b>:
                                 /photo          - send a photo
                                 /inline_buttons - send inline buttons
                                 /keyboard       - send keyboard buttons
                                 /remove         - remove keyboard buttons
                                 /request        - request location or contact
                                 /inline_mode    - send inline-mode results list
                                 /poll           - send a poll
                                 /poll_anonymous - send an anonymous poll
                                 /throw          - what happens if handler fails
                             """;
        return await bot.SendTextMessageAsync(msg.Chat, usage, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> RegistrationStage(Message msg)
    {
        if (msg.Text is not { } messageText)
            return new Message();
        Message sentMessage = await (messageText.Split("@")[0] switch
        {
            "/EndRegistration" => RegistrationEnd(msg),
            _ => CommandRegistration(msg)
        });
        return sentMessage;
    }

    async Task<Message> RegistrationEnd(Message msg)
    {
        const string text = """
                            Регистрация окончена
                                    блять

                            """;
        lobbyStage = "ReadyToPicks";
        return await bot.SendTextMessageAsync(msg.Chat, text, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }

    async Task<Message> AddToTeam(Message msg)
    {
        var command = msg.Text;
        var userName = msg.From.Username;
        if (command == "1")
        {
            if (!team1.Contains(userName))
            {
                team1.Add(userName);
                return await bot.SendTextMessageAsync(
                    msg.Chat, 
                    $"<b>{userName}</b> добавлен в команду <b>{Command1Name}</b>. \n Игроков в команде - {team1.Count}",
                    parseMode: ParseMode.Html
                );
            }

            return await bot.SendTextMessageAsync(
                msg.Chat, 
                $"<b>{userName}</b> уже есть в команде <b>{Command1Name}</b>.",
                parseMode: ParseMode.Html
            );
        }
        else
        {
            if (!team2.Contains(userName))
            {
                team2.Add(userName);
                return await bot.SendTextMessageAsync(
                    msg.Chat, 
                    $"<b>{userName}</b> добавлен в команду <b>{Command2Name}</b>. \n Игроков в команде - {team2.Count}",
                    parseMode: ParseMode.Html
                );
            }

            return await bot.SendTextMessageAsync(
                msg.Chat, 
                $"<b>{userName}</b> уже есть в команде <b>{Command2Name}</b>.",
                parseMode: ParseMode.Html
            );
        }
    }

    async Task<Message> DefaultMessage(Message msg)
    {
        var text = $"Комманда {msg.Text} в данный момент не валидна";
        return await bot.SendTextMessageAsync(msg.Chat, text, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
    async Task<Message> CommandRegistration(Message msg)
    {
        var sentMessage = await (msg.Text switch
        {
            "1" => AddToTeam(msg),
            "2" => AddToTeam(msg),
            _ => DefaultMessage(msg)
        });

        return sentMessage;
    }
    
    
}