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
    private static bool isTeam1Turn = false;
    private static bool pickStage = false;
    private static int banCount = 0;
    private static int pickCount = 0;
    List<string> teamNames = new List<string> { "Ember", "Sapphire" };
    private string team1Name, team2Name;

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
                        $"Герой **{update.CallbackQuery.Data}** забанен. Стадия банов завершена! \n Введите команду /pick для стадии пиков",
                        parseMode: ParseMode.Markdown
                    );
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"Герой **{update.CallbackQuery.Data}** забанен. **{(isTeam1Turn ? team2Name : team1Name)}** следующая.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Markdown
                    );
                }

                isTeam1Turn = !isTeam1Turn;
            }
            else if (pickCount < 12 & pickStage)
            {
                pickCount++;
                heroes.Remove(update.CallbackQuery.Data);
                if (pickCount != 12)
                {
                    await botClient.SendTextMessageAsync(
                        update.CallbackQuery.Message.Chat.Id,
                        $"Герой **{update.CallbackQuery.Data}** выбран. **{(isTeam1Turn ? team2Name : team1Name)}** следующая.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Markdown
                    );
                }
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
                    $"Забаненные герои:\n{string.Join(", ", bannedHeroes)}"
                );
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    $"Команда 1:\n{string.Join("\n", team1Picks.Select(chel => $"**{chel.Nickname}** - **{chel.Hero}**"))}",
                    parseMode: ParseMode.Markdown
                );
                await botClient.SendTextMessageAsync(
                    update.CallbackQuery.Message.Chat.Id,
                    $"Команда 2:\n{string.Join("\n", team2Picks.Select(chel => $"**{chel.Nickname}** - **{chel.Hero}**"))}",
                    parseMode: ParseMode.Markdown
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
                        "Добро пожаловать в турнир по Deadlock! Команда 3, напишите '3'. Команда 6, напишите '6'."
                    );
                    break;

                case "3":
                    if (!team1.Contains(userName))
                    {
                        team1.Add(userName);
                        await botClient.SendTextMessageAsync(
                            chatId, 
                            $"**{userName}** добавлен в команду 3.",
                            parseMode: ParseMode.Markdown
                            );
                        Console.WriteLine($"{userName}");
                    }

                    break;

                case "6":
                    if (!team2.Contains(userName))
                    {
                        team2.Add(userName);
                        await botClient.SendTextMessageAsync(
                            chatId, 
                            $"**{userName}** добавлен в команду 6.",
                            parseMode: ParseMode.Markdown
                            );
                    }

                    break;

                case "/ban":
                    // team1 - команда 3
                    // team2 - команда 6
                    int teamCoin = new Random().Next(teamNames.Count());
                    team1Name = teamNames[teamCoin];
                    teamNames.RemoveAt(teamCoin);
                    team2Name = teamNames[0];
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"Команда 3 - **{team1Name}**\nКоманда 6 - **{team2Name}**",
                        parseMode: ParseMode.Markdown
                        );
                    
                    if (team1.Count == 1 && team2.Count == 1)
                    {
                        isTeam1Turn = new Random().Next(2) == 0;
                        var replyMarkup = new InlineKeyboardMarkup(GetHeroes());
                        await botClient.SendTextMessageAsync(
                            chatId,
                            $"Начинаем бан героев. **{(isTeam1Turn ? team1Name : team2Name)}** начинает.",
                            replyMarkup: replyMarkup,
                            parseMode: ParseMode.Markdown
                            );
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Не все игроки зарегистрированы.");
                    }

                    break;

                case "/pick":
                    pickStage = true; 
                    isTeam1Turn = new Random().Next(2) == 0;
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"Начинается стадия пиков! **{(isTeam1Turn ? team2Name : team1Name)}** выбирает первой!.",
                        replyMarkup: new InlineKeyboardMarkup(GetHeroes()),
                        parseMode: ParseMode.Markdown
                    );
                    isTeam1Turn = !isTeam1Turn;
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
}
