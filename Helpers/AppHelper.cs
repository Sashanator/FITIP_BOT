using Serilog;
using Serilog.Events;
using Telegram.Bot.Types;
using TelegramBot.Constants;
using TelegramBot.Controllers;
using TelegramBot.Models;
using File = System.IO.File;

namespace TelegramBot.Helpers;

public static class AppHelper
{
    private static List<long> GetUserIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);

        return File.ReadLines(filePath).Where(line => !string.IsNullOrEmpty(line)).Select(long.Parse).ToList();
    }

    private static List<string> GetStickerIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);

        return File.ReadLines(filePath).Where(line => !string.IsNullOrEmpty(line)).ToList();
    }

    public static void AddNewUser(User user)
    {
        if (!AppController.Users.Exists(u => u.UserInfo.Id == user.Id))
            AppController.Users.Add(new AppUser(user));
    }

    /// <summary>
    ///     Create empty files for case when user forgot to create it
    /// </summary>
    private static void CreateResources()
    {
        Directory.CreateDirectory("Resources");
        File.Create(@"Resources/adminIds.txt").Dispose();
        File.Create(@"Resources/participantIds.txt").Dispose();
        File.Create(@"Resources/stickerIds.txt").Dispose();
        File.Create(@"Resources/stage_0.jpg").Dispose();
        File.Create(@"Resources/stage_1.jpg").Dispose();
        File.Create(@"Resources/stage_2.jpg").Dispose();
    }

    public static void InitializeApplication()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            //.WriteTo.Console()
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information).WriteTo.File($@"Logs/Information_{DateTime.Now:dd/MM/yyyy}.log"))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error).WriteTo.File($@"Logs/Errors_{DateTime.Now:dd/MM/yyyy}.log"))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning).WriteTo.File($@"Logs/Warnings_{DateTime.Now:dd/MM/yyyy}.log"))
            .WriteTo.Logger(l => l.Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug).WriteTo.File($@"Logs/Debug_{DateTime.Now:dd/MM/yyyy}.log"))
            .CreateLogger();

        if (File.Exists(BackupController.BACKUP_FILE_PATH)) // Check if backup file exists
        {
            Console.WriteLine("Start restoring the system...");
            BackupController.Restore(); // if exists then invoke Restore function
            Console.WriteLine("System restoring is finished");
        }

        if (!Directory.Exists("Resources"))
        {
            Console.WriteLine("Directory 'Resources' was not found!");
            Console.WriteLine("Creating default directory 'Resources'...");
            CreateResources();
        }

        if (!Directory.Exists("Backup"))
        {
            Directory.CreateDirectory("Backup");
        }

        AppController.ParticipantIds = GetUserIds(FilePaths.ParticipantIds);
        AppController.AdminIds = GetUserIds(FilePaths.AdminIds);
        AppController.StickerIds = GetStickerIds(FilePaths.StickerIds);
    }
}