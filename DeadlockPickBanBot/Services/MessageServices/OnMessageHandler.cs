using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DeadlockPickBanBot.Services.MessageServices;

public class OnMessageHandler(ITelegramBotClient bot, ILogger<OnMessageHandler> logger)
{
    private static string lobbyStage = "Registration";
    // private static string LobbyStage = "Registration";
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

    async Task<Message> RegistrationStage(Message msg)
    {
        if (msg.Text is not { } messageText)
            return new Message();
        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            "/EndRegitration" => RegistrationEnd(msg),
            _ => Usage(msg)
        });
        return sentMessage;
    }

    async Task<Message> RegistrationEnd(Message msg)
    {
        const string text = """
                            Регистрация окончена
                                    блять

                            """;
        return await bot.SendTextMessageAsync(msg.Chat, text, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
    }
    
    async Task<Message> Usage(Message msg)
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
    
    
}