using Telegram.Bot.Types;
using TelegramBot.Controllers;
using TelegramBot.Models;
using File = System.IO.File;

namespace TelegramBot.Helpers;

public static class AppHelper
{
    public static List<long> GetIds(string fileName)
    {
        var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);

        return (from line in File.ReadLines(filePath) where !string.IsNullOrEmpty(line) select long.Parse(line)).ToList();
    }

    public static void AddNewUser(User user, Message message)
    {
        if (!AppController.Users.Exists(u => u.UserInfo.Id == user.Id))
            AppController.Users.Add(new AppUser(user, message));
    }
}