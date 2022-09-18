using Telegram.Bot.Types;

namespace TelegramBot.Models;

public class AppUser
{
    public AppUser(User userInfo, Message? message)
    {
        UserInfo = userInfo;
        Messages = new List<Message?> {message};
    }
    public User UserInfo { get; set; }

    public long? TeamId { get; set; }

    public List<Message?> Messages { get; set; }
}