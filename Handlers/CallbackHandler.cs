using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Constants;
using TelegramBot.Extensions;
using TelegramBot.Models;
using File = System.IO.File;

namespace TelegramBot.Handlers;

public class CallbackHandler
{
    private const int STAGE_COUNT = 3; // Number of floors in University
    private const int MEMBERS_PER_TEAM = 1;

    public static async Task HandleCallback(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        if (callbackQuery == null) return;
        switch (callbackQuery.Data)
        {
            case "team":
                await HandleTeamCallback(botClient, callbackQuery, cancellationToken);
                break;
            case "map":
                await HandleMapCallback(botClient, callbackQuery, cancellationToken);
                break;
            case "faq":
                await HandleFaqCallback(botClient, callbackQuery, cancellationToken);
                break;
            default:
                await HandleTeamScoreCallback(botClient, callbackQuery, cancellationToken);
                break;
                //throw new ArgumentException("No such callback data");
        }
    }

    private static async Task HandleTeamScoreCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data;
        if (data == null) return;
        if (data.Contains('t'))
        { // return new inline keyboard
            if (callbackQuery.Message == null) return;
            var teamId = data.MySubstring(data.IndexOf('t') + 1, data.Length);
            InlineKeyboardMarkup inlineKeyboard = new(GetTeamScoreKeyboard(teamId));
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat,
                text: $"Очки для команды \\#*{teamId}*",
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        else if (data.Contains('-'))
        {
            if (callbackQuery.Message == null) return;

            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('#') + 1, data.IndexOf('-')));
            var points = Convert.ToInt32(data.MySubstring(data.IndexOf('-') + 1, data.Length));
            var team = Program.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return; // GG ???
            team.Score -= points;

            var textResult = $"Вы забрали у команды \\#{teamId}\\: *{points}* очков\nТекущий результат: {team.Score}";
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat,
                textResult,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }
        else if (data.Contains('+'))
        {
            if (callbackQuery.Message == null) return;

            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('#') + 1, data.IndexOf('+')));
            var points = Convert.ToInt32(data.MySubstring(data.IndexOf('+') + 1, data.Length));
            var team = Program.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null) return; // GG ???
            team.Score += points;

            var textResult = $"Вы добавили команде \\#{teamId}\\: *{points}* очков\nТекущий результат: {team.Score}";
            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat,
                textResult,
                parseMode: ParseMode.MarkdownV2,
                cancellationToken: cancellationToken);
        }
        else if (data.Contains('m'))
        {
            if (callbackQuery.Message == null) return;

            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('m') + 1, data.Length));
            var team = Program.Teams.FirstOrDefault(t => t.Id == teamId);
            
            if (team != null)
            {
                var curUserNumber = 1;
                var text = team.Members.Aggregate($"MEMBERS OF TEAM \\#{teamId}\\:\n", (current, member) => 
                    current + $"\\#{curUserNumber++}\\. *[{member.UserInfo.Username}]* {member.UserInfo.FirstName} {member.UserInfo.LastName}\n");
                await botClient.SendTextMessageAsync(
                    chatId: callbackQuery.Message.Chat,
                    text,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);
            }
        }
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetTeamScoreKeyboard(string teamId)
    {
        var pointsToAdd = new List<InlineKeyboardButton>();
        var pointsToRemove = new List<InlineKeyboardButton>();
        for (var i = 0; i < 5; i++)
        {
            pointsToAdd.Add(InlineKeyboardButton.WithCallbackData(text: $"+{i + 1}", callbackData: $"#{teamId}+{i + 1}"));
            pointsToRemove.Add(InlineKeyboardButton.WithCallbackData(text: $"-{i + 1}", callbackData: $"#{teamId}-{i + 1}"));
        }

        var infoButton = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(text: "Members", callbackData: $"m{teamId}")
        };
        return new List<List<InlineKeyboardButton>> {pointsToAdd, pointsToRemove, infoButton};
    }

    public static async Task HandleMapCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null) return;
        
        var mapPictures = new List<InputMediaPhoto>();
        for (var i = 0; i < STAGE_COUNT; i++)
        {
            var fileName = $"stage_{i}.jpg";
            var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);
            var fileStream = File.OpenRead(filePath);
            mapPictures.Add(new InputMediaPhoto(new InputMedia(fileStream, $"Stage #{i + 1}")));
        }

        await botClient.SendMediaGroupAsync(
            callbackQuery.Message.Chat,
            media: new IAlbumInputMedia[]
            {
                mapPictures[0], mapPictures[1], mapPictures[2]
            },
            cancellationToken: cancellationToken);
    }

    public static async Task HandleFaqCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null) return;
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                                new KeyboardButton[] { Questions.Question_1 },
                                new KeyboardButton[] { Questions.Question_2 },
                                new KeyboardButton[] { Questions.Question_3 },
                                new KeyboardButton[] { Questions.Question_4 },
                                new KeyboardButton[] { Questions.Question_5 },
                                new KeyboardButton[] { Questions.Question_6 },
                                new KeyboardButton[] { Questions.Question_7 },
                            })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat,
            "Выбери свой вопрос",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    public static async Task HandleTeamCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null) return;

        var user = Program.Users.FirstOrDefault(u => u.UserInfo.Id == callbackQuery.From.Id);

        if (user == null) return; // Throw an exception

        var teamId = user.RegNumber / MEMBERS_PER_TEAM + 1;
        user.TeamId ??= teamId;
        var team = Program.Teams.FirstOrDefault(t => t.Id == teamId);
        // Create new team if it is not exist and add user to it
        if (team == null)
        {
            Program.Teams.Add(new Team(teamId, user));
        }
        else
        {
            // Add user to team if it is exists and he is not already there
            if (team.Members.FirstOrDefault(m => m.RegNumber == user.RegNumber) == null)
            {
                team.Members.Add(user);
            }
        }
        
        
        var message = await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat,
            $"Номер твой команды: {user.TeamId}",
            disableNotification: false,
            replyToMessageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);

        await botClient.PinChatMessageAsync(callbackQuery.From.Id, message.MessageId, cancellationToken: cancellationToken);

        Log(user);
    }

    private static void Log(AppUser user)
    {
        var dirPath = Path.Combine(Environment.CurrentDirectory, @"Logs/");
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        var date = DateTime.Now.Date;
        var time = DateTime.Now.TimeOfDay;
        var dateTime = $"{date.Day}-{date.Month}-{date.Year}_{time.Hours}-{time.Minutes}-{time.Seconds}";

        using var writer = new StreamWriter(Path.Combine(dirPath, $"log_{dateTime}.txt"));
        writer.WriteLine($"{user.UserInfo.Id} {user.UserInfo.Username} {user.UserInfo.FirstName} {user.UserInfo.LastName} {user.RegNumber} {user.TeamId}");
        writer.WriteLine("MESSAGES:");
        var i = 1;
        foreach (var message in user.Messages)
        {
            writer.WriteLine($"Message #{i++}: {message?.Text}");
        }
        writer.WriteLine();
    }
}