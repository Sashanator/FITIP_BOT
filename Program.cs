using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Handlers;
using TelegramBot.Helper;
using AppUser = TelegramBot.Models.AppUser;

namespace TelegramBot;

internal class Program
{
    public static ITelegramBotClient Bot = new TelegramBotClient("5439105151:AAF5g0CyPannZPgwe2dtFF905wYXpAA_0QY");
    public static List<AppUser> Users { get; set; } = new();
    private static int _registrationCount = 0;

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var validTelegramIds = TelegramIdHelper.GetIds();
        
        if (update.Message != null && update.Message.From != null && !validTelegramIds.Contains(update.Message.From.Id))
        {
            await botClient.SendTextMessageAsync(
                update.Message.Chat,
                "Прошу нас простить, вы не можете пользоваться системой",
                cancellationToken: cancellationToken
            );
            
            return;
        }
        
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        var message = update.Message;
        switch (update.Type)
        {
            case UpdateType.Message:
                if (message == null) return;
                // Add user when he sends his first message
                var userInfo = message.From;
                if (userInfo != null && !Users.Exists(u => u.UserInfo.Id == userInfo.Id))
                    Users.Add(new AppUser(userInfo, _registrationCount++, message));
                
                if (message.Text == null) return;

                if (message.Text.ToLower().StartsWith('/'))
                    await CommandHandler.HandleCommands(botClient, message, cancellationToken);
                else
                    await MessageHandler.HandleMessages(botClient, message, cancellationToken);

                break;
            case UpdateType.CallbackQuery:
                await CallbackHandler.HandleCallback(botClient, update, cancellationToken);
                break;
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }


    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(Environment.CurrentDirectory, @"Logs/", "logs.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Console.WriteLine("Start bot " + Bot.GetMeAsync().Result.FirstName);
        

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }, // receive all update types
        };
        Bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );
        Console.ReadLine();
    }

    //public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    //{
    //    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
    //    var message = update.Message;
    //    switch (update.Type)
    //    {
    //        case UpdateType.Message:
    //            if (message == null) return;

    //            // Add user when he sends his first message
    //            var userInfo = message.From;
    //            if (userInfo != null && !Users.Exists(u => u.UserInfo.Id == userInfo.Id))
    //            {
    //                Users.Add(new AppUser(userInfo, _registrationCount++, message));
    //            }

    //            switch (message.Text?.ToLower())
    //            {
    //                case "/start":
    //                    await CommandHandler.SendMainMenu(botClient, message, cancellationToken);
    //                    break;
    //                case "/test":
    //                    {
    //                        InlineKeyboardMarkup inlineKeyboard = new(new[]
    //                        {
    //                        // first row
    //                        new []
    //                        {
    //                            InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
    //                            InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
    //                        },
    //                        // second row
    //                        new []
    //                        {
    //                            InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
    //                            InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
    //                        },
    //                    });
    //                        await botClient.SendTextMessageAsync(
    //                            chatId: message.Chat,
    //                            text: "A message with an inline keyboard markup",
    //                            replyMarkup: inlineKeyboard,
    //                            cancellationToken: cancellationToken);
    //                        break;
    //                    }
    //                case "/keyboard":
    //                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
    //                    {
    //                        new KeyboardButton[] { "My Team", "Map" },
    //                        new KeyboardButton[] { "FAQ" },
    //                    })
    //                    {
    //                        ResizeKeyboard = true
    //                    };
    //                    await botClient.SendTextMessageAsync(
    //                        message.Chat,
    //                        "Choose answer",
    //                        replyMarkup: replyKeyboardMarkup,
    //                        cancellationToken: cancellationToken);
    //                    break;
    //                case "/sticker":
    //                    await botClient.SendStickerAsync(
    //                        message.Chat,
    //                        sticker: new InputOnlineFile("CAACAgEAAxkBAAMuYxtadoAkk4sJ7mZxNUYxStZGYLgAAiEAAxaMEy3VcXlGmQwnKSkE"),
    //                        cancellationToken: cancellationToken);
    //                    break;
    //                case "/photo":
    //                    {
    //                        var filePath = "15.png";
    //                        var fullPath = Path.Combine(Environment.CurrentDirectory, @"Resources/", filePath);

    //                        await using var stream = File.OpenRead(fullPath);
    //                        await botClient.SendPhotoAsync(
    //                            message.Chat,
    //                            photo: new InputOnlineFile(stream),
    //                            cancellationToken: cancellationToken);
    //                        break;
    //                    }
    //                case "/voice":
    //                    {
    //                        var filePath = "MyVoice.ogg";
    //                        var fullPath = Path.Combine(Environment.CurrentDirectory, @"Resources/", filePath);

    //                        await using var stream = File.OpenRead(fullPath);
    //                        await botClient.SendVoiceAsync(
    //                            message.Chat,
    //                            voice: stream!,
    //                            duration: 1,
    //                            cancellationToken: cancellationToken);
    //                        break;
    //                    }
    //                case "/all":
    //                    {
    //                        var audioPath = "15.png";
    //                        var fullAudioPath = Path.Combine(Environment.CurrentDirectory, @"Resources/", audioPath);
    //                        await using var audioStream = File.OpenRead(fullAudioPath);

    //                        var photoPath = "15.png";
    //                        var fullPhotoPath = Path.Combine(Environment.CurrentDirectory, @"Resources/", photoPath);
    //                        await using var photoStream = File.OpenRead(fullPhotoPath);

    //                        await botClient.SendMediaGroupAsync(
    //                            message.Chat,
    //                            media: new IAlbumInputMedia[]
    //                            {
    //                            new InputMediaPhoto(new InputMedia(photoStream, "Photo #1")),
    //                            new InputMediaPhoto(new InputMedia(audioStream, "Audio #1"))
    //                            },
    //                            cancellationToken: cancellationToken);
    //                        break;
    //                    }
    //                case "/location":
    //                    await botClient.SendLocationAsync(
    //                        message.Chat,
    //                        latitude: 59.95736004063536,
    //                        longitude: 30.308389064569138,
    //                        cancellationToken: cancellationToken);
    //                    break;
    //                default:
    //                    await botClient.SendTextMessageAsync(
    //                        message.Chat,
    //                        $"Я тебя не понял, *{message.From?.FirstName}*\\!",
    //                        parseMode: ParseMode.MarkdownV2,
    //                        disableNotification: false,
    //                        replyToMessageId: message.MessageId,
    //                        cancellationToken: cancellationToken);
    //                    break;
    //            }

    //            break;
    //        case UpdateType.CallbackQuery:
    //            await CallbackHandler.HandleCallback(botClient, update, cancellationToken);
    //            break;
    //    }
    //}
}