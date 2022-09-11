using Telegram.Bot.Types;

namespace TelegramBot.Models;

public class AppUser
{
    public AppUser(User userInfo, int regNumber, Message? message)
    {
        UserInfo = userInfo;
        Messages = new List<Message?> {message};
        RegNumber = regNumber;
    }
    public User UserInfo { get; set; }

    public long? TeamId { get; set; }

    public int RegNumber { get; set; }

    public List<Message?> Messages { get; set; }
}