using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramBot.Controllers;
using TelegramBot.Handlers;
using TelegramBot.Helpers;

namespace TelegramBot;

internal class Program
{
    public static ITelegramBotClient Bot = new TelegramBotClient("5439105151:AAF5g0CyPannZPgwe2dtFF905wYXpAA_0QY");

    public static async Task Main(string[] args)
    {
        try
        { // TODO: Check how to break it ?
            if (args.Length == 0 || Convert.ToInt32(args[0]) <= 0)
            {
                Console.WriteLine("Invalid argument for FITIP_BOT!\nEnter team count > 0");
                return;
            }
            AppController.TeamsCount = Convert.ToInt32(args[0]);

            // Initialize start state of application
            AppHelper.InitializeApplication();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        await BackupController.ScheduleBackupJob();

        Console.WriteLine("Start bot " + Bot.GetMeAsync().Result.FirstName);

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