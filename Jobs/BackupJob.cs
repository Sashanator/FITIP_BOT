using Newtonsoft.Json;
using Quartz;
using TelegramBot.Controllers;
using TelegramBot.Models;

namespace TelegramBot.Jobs;

public class BackupJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var backup = new Backup(AppController.RegNumber, AppController.Teams, AppController.Users);
        var jsonString = JsonConvert.SerializeObject(backup);

        await File.WriteAllTextAsync(BackupController.BACKUP_FILE_PATH, jsonString);
    }
}