using Newtonsoft.Json;
using TelegramBot.Models;

namespace TelegramBot.Controllers;

public static class BackupController
{
    public const string BACKUP_FILE_PATH = @"Backup/backup.json";

    public static void CreateBackup()
    {
        var backup = new Backup(AppController.RegNumber, AppController.Teams, AppController.Users);
        var jsonString = JsonConvert.SerializeObject(backup);
        // TODO: Create dir if it's not exist
        File.WriteAllText(BACKUP_FILE_PATH, jsonString);
    }

    public static void Restore()
    {
        // TODO: Check if dir does not exist
        var jsonString = File.ReadAllText(BACKUP_FILE_PATH);
        var backup = JsonConvert.DeserializeObject<Backup>(jsonString);
        if (backup == null) throw new Exception("Backup was null");

        AppController.RegNumber = backup.RegNumber;
        AppController.Teams = backup.Teams;
        AppController.Users = backup.Users;
    }
}