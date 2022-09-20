using Serilog;
using Serilog.Events;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramBot.Constants;
using TelegramBot.Controllers;
using TelegramBot.Handlers;
using TelegramBot.Helpers;

namespace TelegramBot;

internal class Program
{
    public static ITelegramBotClient Bot = new TelegramBotClient("5508993832:AAGe23sZBG8hR2N0oHbkTsBOQPcy0QrJvGs");

    public static void Main(string[] args)
    {
        try
        { // Check how to break it ?
            if (args.Length == 0 || Convert.ToInt32(args[0]) <= 0)
            {
                Console.WriteLine("Invalid argument for FITIP_BOT!\nEnter team count > 0");
                return;
            }

            AppController.TeamsCount = Convert.ToInt32(args[0]);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File($@"Logs/Information_{DateTime.Now:dd/MM/yyyy}.log"))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File($@"Logs/Errors_{DateTime.Now:dd/MM/yyyy}.log"))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File($@"Logs/Warnings_{DateTime.Now:dd/MM/yyyy}.log"))
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