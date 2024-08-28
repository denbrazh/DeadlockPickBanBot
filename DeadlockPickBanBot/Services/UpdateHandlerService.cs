using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DeadlockPickBanBot.Services;

public class UpdateHandlerService
{
    private static List<string> heroes = new List<string>
    {
        "Abrams", "Ivy", "Kelvin",
        "McGinnes", "Wraith", "Haze",
        "Bebop", "Dynamo", "Grey Talon",
        "Infernus", "Lady Geist", "Lash",
        "Paradox", "Pocket", "Seven",
        "Yamato", "Warden", "Vindicta",
        "Viscous", "Mo Krill", "Заточка"
    };

    class Chelik
    {
        public string Nickname { get; set; }
        public string Hero { get; set; }
    }

    private static List<string> bannedHeroes = new List<string>();
    private static List<Chelik> team1Picks = new List<Chelik>();
    private static List<Chelik> team2Picks = new List<Chelik>();
    private static List<string> team1 = new List<string>();
    private static List<string> team2 = new List<string>();
    private static string Command1Name = "GOYDA";
    private static string Command2Name = "Предупреждаем";
    private static bool isTeam1Turn = false;
    private static bool pickStage = false;
    private static int banCount = 0;
    private static int pickCount = 0;

    private static bool randomStart = true;
    private static string stringCoin;

    public async Task HandleUpdateAsync(Update update, ITelegramBotClient botClient)
    {
        if (update.Type == UpdateType.CallbackQuery && CheckPermiss(update.CallbackQuery.From.Username))
        {
            if (banCount < 4)
            {
                heroes.Remove(update.CallbackQuery.Data);
                bannedHeroes.Add(update.CallbackQuery.Data);
                banCount++;
                if (banCount == 4)
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"<b>{(isTeam1Turn ? Command1Name : Command2Name)}</b> забанили <b>{update.CallbackQuery.Data}</b>." +
                        $"\nСтадия банов завершена!" +
                        $"\n\nВведите команду /pick для стадии пиков",
                        parseMode: ParseMode.Html
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"<b>{(isTeam1Turn ? Command1Name : Command2Name)}</b> забанили <b>{update.CallbackQuery.Data}</b>" +
                        $"\n\n<b>{(isTeam1Turn ? Command2Name : Command1Name)}</b> банит героя.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Html
                    );
                }

                isTeam1Turn = !isTeam1Turn;
            }
            else if (pickCount < 12 & pickStage)
            {
                pickCount++;
                heroes.Remove(update.CallbackQuery.Data);
                if (isTeam1Turn)
                {
                    team1Picks.Add(new Chelik
                    {
                        Nickname = update.CallbackQuery.From.Username,
                        Hero = update.CallbackQuery.Data
                    });
                }
                else
                {
                    team2Picks.Add(new Chelik
                    {
                        Nickname = update.CallbackQuery.From.Username,
                        Hero = update.CallbackQuery.Data
                    });
                }
                
                if (pickCount != 12)
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"{(isTeam1Turn ? team1Picks.Count : team2Picks.Count)}-й герой для команды <b>{(isTeam1Turn ? Command1Name : Command2Name)}</b> это <b>{update.CallbackQuery.Data}</b>!" +
                        $"\n\nКоманда <b>{(isTeam1Turn ? Command2Name : Command1Name)}</b> выбирает героя",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Html
                    );
                }

                isTeam1Turn = !isTeam1Turn;
            }

            if (banCount == 4 && pickCount == 12)
            {
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    "<b>СТАДИЯ ВЫБОРА ЗАВЕРШЕНА!</b>\n\n" +
                    $"Забаненные герои:\n{string.Join("\n", bannedHeroes)}\n\n" +
                    $"Команда <b>{Command1Name}</b>:\n- {string.Join("\n- ", team1Picks.Select(chel => $"<i>{chel.Hero}</i>"))}\n\n" +
                    $"Команда <b>{Command2Name}</b>:\n- {string.Join("\n- ", team2Picks.Select(chel => $"<i>{chel.Hero}</i>"))}\n\n" +
                    "<b>GL  HF</b>",
                    parseMode: ParseMode.Html
                );
            }
        }

        if (update.Type == UpdateType.Message)
        {
            var chatId = update.Message.Chat.Id;
            var userName = update.Message.From.Username;
            var message = update.Message.Text;

            switch (message)
            {
                case "/start":
                    var res = await botClient.SendTextMessageAsync(
                        chatId, 
                        $"Добро пожаловать в турнир по Deadlock! Команда <b>{Command1Name}</b>, напишите '1'. Команда <b>{Command2Name}</b>, напишите '2'.",
                            parseMode: ParseMode.Html
                    );
                    break;

                case "1":
                    if (!team1.Contains(userName))
                    {
                        team1.Add(userName);
                        await botClient.SendTextMessageAsync(
                            chatId, 
                            $"<b>{userName}</b> добавлен в команду <b>{Command1Name}</b>.",
                            parseMode: ParseMode.Html
                        );
                        Console.WriteLine($"{userName} в команде {Command1Name}");

                        if (team1.Count == 1 && team2.Count == 1)
                        {
                            goto case "/ban";
                        }
                    }

                    break;

                case "2":
                    if (!team2.Contains(userName))
                    {
                        team2.Add(userName);
                        await botClient.SendTextMessageAsync(
                            chatId,
                            $"<b>{userName}</b> добавлен в команду <b>{Command2Name}</b>.",
                            parseMode: ParseMode.Html
                        );
                        Console.WriteLine($"{userName} в комманде {Command2Name}");

                        if (team1.Count == 1 && team2.Count == 1)
                        {
                            goto case "/ban";
                        }
                    }

                    break;

                case "/ban":
                    if (team1.Count != 1 | team2.Count != 1)
                    {
                        break;
                    }
                    
                    if (randomStart) { stringCoin = CoinFlip(); }
                    else
                    {
                        stringCoin = 
                            $"Начинаем банить героев!" +
                            $"\n\n{(isTeam1Turn ? $"Команда <b>{Command1Name}</b>" : $"Команда <b>{Command2Name}</b>")} банит первой.";
                    }
                    
                    var replyMarkup = new InlineKeyboardMarkup(GetHeroes());
                    await botClient.SendTextMessageAsync(
                        chatId,
                        stringCoin,
                        replyMarkup: replyMarkup,
                        parseMode: ParseMode.Html
                    );
                    break;

                case "/pick":
                    pickStage = true;
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"{(isTeam1Turn ? $"Команда <b>{Command1Name}</b>" : $"Команда <b>{Command2Name}</b>")} выбирает героя.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Html
                    );
                    break;
            }
        }
    }

    public static List<List<InlineKeyboardButton>> GetHeroes()
    {
        List<List<InlineKeyboardButton>> markup = [];
        var counter = 0;

        for (var index = 0; index < heroes.Count; index += 3)
        {
            var row = new List<InlineKeyboardButton>();
            for (int i = index; i < index + 3; i++)
            {
                if (heroes.Count - 1 >= i)
                {
                    row.Add(InlineKeyboardButton.WithCallbackData(heroes[i], heroes[i]));
                }
            }

            markup.Add(row);
        }

        return markup;
    }

    public static bool CheckPermiss(string user)
    {
        if (isTeam1Turn & team1.Contains(user))
        {
            return true;
        }

        if (!isTeam1Turn & team2.Contains(user))
        {
            return true;
        }

        return false;
    }

    public static string CoinFlip()
    {
        var team1Random = 1;
        var team2Random = 1;

        
        while (team1Random == team2Random)
        {
            team1Random = new Random().Next(0, 100);
            team2Random = new Random().Next(0, 100);
        }

        isTeam1Turn = team1Random > team2Random;

        var word1 = "бан";
        var word2 = "банить";
        
        if (pickStage)
        {
            word1 = "пик";
            word2 = "пикать";
        }
        
        return $"Начинаем {word1} героев.\n" +
               $"\nКоманда <b>{Command1Name}</b> выбросила (<b>{team1Random}</b>)" +
               $"\nКоманда <b>{Command2Name}</b> выбросила (<b>{team2Random}</b>)" +
               $"\n\n{(isTeam1Turn ? $"Команда <b>{Command1Name}</b>" : $"Команда <b>{Command2Name}</b>")} начинает {word2} первой.";
    }
}