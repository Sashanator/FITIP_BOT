using Telegram.Bot.Types;
using TelegramBot.Controllers;
using TelegramBot.Models;
using File = System.IO.File;

namespace TelegramBot.Helpers;

public static class AppHelper
{
    public static List<long> GetUserIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);

        return File.ReadLines(filePath).Where(line => !string.IsNullOrEmpty(line)).Select(long.Parse).ToList();
    }

    public static List<string> GetStickerIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);

        return File.ReadLines(filePath).Where(line => !string.IsNullOrEmpty(line)).ToList();
    }

    public static void AddNewUser(User user, Message? message)
    {
        if (!AppController.Users.Exists(u => u.UserInfo.Id == user.Id))
            AppController.Users.Add(new AppUser(user, message));
    }
}