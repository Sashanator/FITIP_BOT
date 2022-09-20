using TelegramBot.Models;

namespace TelegramBot.Controllers;

public static class AppController
{
    public static List<AppUser> Users { get; set; } = new();
    public static List<Team> Teams { get; set; } = new();
    public static List<long> ParticipantIds { get; set; } = new();
    public static List<long> AdminIds { get; set; } = new();
    public static List<string> StickerIds { get; set; } = new();
    public static int TeamsCount { get; set; } = 0;
    public static int RegNumber { get; set; } = 0;
}