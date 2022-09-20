using Telegram.Bot.Types;

namespace TelegramBot.Models;

public class AppUser
{
    public AppUser(User userInfo)
    {
        UserInfo = userInfo;
    }
    public User UserInfo { get; set; }

    public long? TeamId { get; set; }
}