using Serilog;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramBot.Constants;
using TelegramBot.Controllers;
using TelegramBot.Handlers;
using TelegramBot.Helpers;

namespace TelegramBot;

internal class Program
{
    public static ITelegramBotClient Bot = new TelegramBotClient("5439105151:AAF5g0CyPannZPgwe2dtFF905wYXpAA_0QY");

    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(Environment.CurrentDirectory, @"Logs/", "logs.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Console.WriteLine("Start bot " + Bot.GetMeAsync().Result.FirstName);

        AppController.ParticipantIds = AppHelper.GetUserIds(FilePaths.ParticipantIds);
        AppController.AdminIds = AppHelper.GetUserIds(FilePaths.AdminIds);
        AppController.StickerIds = AppHelper.GetStickerIds(FilePaths.StickerIds);

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }, // receive all update types
        };
        Bot.StartReceiving(
            UpdateHandler.HandleUpdateAsync,
            ErrorHandler.HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );
        Console.ReadLine();
    }
}