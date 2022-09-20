namespace TelegramBot.Models;

public class Backup
{
    public Backup(int regNumber, List<Team> teams, List<AppUser> appUsers)
    {
        RegNumber = regNumber;
        Teams = teams;
        Users = appUsers;
    }
    public int RegNumber { get; set; }
    public List<Team> Teams { get; set; }
    public List<AppUser> Users { get; set; }
}