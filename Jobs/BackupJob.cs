using Newtonsoft.Json;
using Quartz;
using TelegramBot.Controllers;
using TelegramBot.Models;

namespace TelegramBot.Jobs;

public class BackupJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        //Log.Debug("JOB STARTED");
        var backup = new Backup(AppController.RegNumber, AppController.Teams, AppController.Users);
        var jsonString = JsonConvert.SerializeObject(backup);
        // TODO: Create dir if it's not exist
        await File.WriteAllTextAsync(@"Backup/backup.json", jsonString);
        //Log.Debug("JOB FINISHED");
    }
}