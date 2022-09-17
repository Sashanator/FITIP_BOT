using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Handlers;

public static class CommandHandler
{
    private const int TEAMS_IN_ROW = 8;
    private static async Task SendMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Team", callbackData: "team"),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Map", callbackData: "map"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "FAQ", callbackData: "faq") 
            }
        });
        await botClient.SendTextMessageAsync(
            chatId: message.Chat,
            text: "Добро пожаловать на посвящение в первокурсники! Главное меню:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private static async Task SendTeamsMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(GetInlineKeyboard(Program.Teams.Count));
        await botClient.SendTextMessageAsync(
            chatId: message.Chat,
            text: "Выберите команду",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetInlineKeyboard(int teamsCount)
    {
        var result = new List<List<InlineKeyboardButton>>();
        var rowCount = (teamsCount - 1)  / TEAMS_IN_ROW + 1;
        var curTeam = 1;
        for (var i = 0; i < rowCount; i++)
        {
            var newRow = new List<InlineKeyboardButton>();

            for (var j = 0; j < TEAMS_IN_ROW; j++)
            {
                if (curTeam > teamsCount)
                {
                    for (var k = j; k < TEAMS_IN_ROW; k++)
                    {
                        newRow.Add(InlineKeyboardButton.WithCallbackData(text: " ", callbackData: "gg"));
                    }
                    break;
                }

                newRow.Add(InlineKeyboardButton.WithCallbackData(text: $"#{curTeam}", callbackData: $"t{curTeam}"));
                curTeam++;
            }
            
            result.Add(newRow);
        }

        return result;
    }

    private static async Task SendScoreMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var teams = Program.Teams.OrderBy(r => r.Score).ToList();
        var result = teams.Aggregate("", (current, team) => current + $"Team \\#{team.Id}: *{team.Score}*\n");
        await botClient.SendTextMessageAsync(
            message.Chat,
            result.Length > 0 ? result : "ERROR :)",
            parseMode: ParseMode.MarkdownV2,
            disableNotification: false,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);
    }

    public static async Task HandleCommands(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        switch (message.Text?.ToLower())
        {
            case "/start":
                await SendMainMenu(botClient, message, cancellationToken);
                break;
            case "/teams":
                if (message.From?.Id == null) return;
                if (Program.AdminIds.Contains(message.From.Id)) 
                    await SendTeamsMenu(botClient, message, cancellationToken);
                else
                    await botClient.SendTextMessageAsync(
                        message.Chat,
                        "У вас недостаточно прав!",
                        parseMode: ParseMode.MarkdownV2,
                        disableNotification: false,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                break;
            case "/score":
                await SendScoreMessage(botClient, message, cancellationToken);
                break;
            default:
                await botClient.SendTextMessageAsync(
                    message.Chat,
                    $"Я тебя не понял, *{message.From?.FirstName}*\\!",
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: false,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
                break;
        }
    }
}