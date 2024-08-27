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
    private static string Command1Name = "Первая";
    private static string Command2Name = "Вторая";
    private static bool isTeam1Turn = false;
    private static bool pickStage = false;
    private static int banCount = 0;
    private static int pickCount = 0;

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
                        $"{(isTeam1Turn ? Command1Name : Command2Name)} бан {update.CallbackQuery.Data}. \n Стадия банов завершена! \n Введите команду /pick для стадии пиков"
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"{(isTeam1Turn ? Command1Name : Command2Name)} бан {update.CallbackQuery.Data} \n {(isTeam1Turn ? Command2Name : Command1Name)} банит следующей.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes())
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
                        $"{(isTeam1Turn ? team1Picks.Count : team2Picks.Count)} герой для команды " +
                        $"{(isTeam1Turn ? Command1Name : Command2Name)} это {update.CallbackQuery.Data}! \n {(isTeam1Turn ? Command2Name : Command1Name)} выбирает следующая!",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes())
                    );
                }

                isTeam1Turn = !isTeam1Turn;
            }

            if (banCount == 4 && pickCount == 12)
            {
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    "Баны и пики завершены."
                );
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    $"Забаненные герои:\n{string.Join("\n", bannedHeroes)}"
                );
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    $"Команда 3:\n{string.Join("\n", team1Picks.Select(chel => $"{chel.Hero}"))}"
                );
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    $"Команда 6:\n{string.Join("\n", team2Picks.Select(chel => $"{chel.Hero}"))}"
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
                        $"Добро пожаловать в турнир по Deadlock! Команда {Command1Name}, напишите '1'. Команда {Command2Name}, напишите '2'."
                    );
                    break;

                case "1":
                    if (!team1.Contains(userName))
                    {
                        team1.Add(userName);
                        await botClient.SendTextMessageAsync(chatId, $"{userName} добавлен в команду {Command1Name}.");
                        Console.WriteLine($"{userName} в комманде {Command1Name}");

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
                        await botClient.SendTextMessageAsync(chatId, $"{userName} добавлен в команду {Command2Name}.");
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
                    var stringCoin = CoinFlip();
                    var replyMarkup = new InlineKeyboardMarkup(GetHeroes());
                    await botClient.SendTextMessageAsync(chatId,
                        stringCoin,
                        replyMarkup: replyMarkup);
                    break;

                case "/pick":
                    pickStage = true;
                    var stringPickCoin = CoinFlip();
                    await botClient.SendTextMessageAsync(
                        chatId,
                        stringPickCoin,
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes())
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
        
        return $"Начинаем {word1} героев. \n Команда {Command1Name} выбросила ({team1Random}) " +
               $"\n Команда {Command2Name} выбросила ({team2Random}) " +
               $"\n {(isTeam1Turn ? $"Команда {Command1Name}" : $"Команда {Command2Name}")} начинает {word2} первой.";
    }
}