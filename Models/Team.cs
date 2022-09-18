namespace TelegramBot.Models;

public class Team
{
    public Team(int id, AppUser user)
    {
        Id = id;
        Members = new List<AppUser> { user };
        Score = 0;
    }
    public int Id { get; set; }

    public int Score { get; set; }

    public List<AppUser> Members { get; set; }

    public List<string> History { get; set; }

    //History.Add($"{user.Id}: +5")
}