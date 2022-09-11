using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Constants;
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
                throw new ArgumentException("No such callback data");
        }
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
        
        user.TeamId ??= user.RegNumber / MEMBERS_PER_TEAM + 1;
        
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