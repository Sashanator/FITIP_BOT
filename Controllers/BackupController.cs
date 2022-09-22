using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using TelegramBot.Jobs;
using TelegramBot.Models;

namespace TelegramBot.Controllers;

public static class BackupController
{
    public const string BACKUP_FILE_PATH = @"Backup/backup.json";

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

    public static async Task ScheduleBackupJob()
    {
        // construct a scheduler factory using defaults
        var factory = new StdSchedulerFactory();

        // get a scheduler
        var scheduler = await factory.GetScheduler();
        await scheduler.Start();

        // define the job and tie it to our HelloJob class
        var job = JobBuilder.Create<BackupJob>()
            .WithIdentity("myJob", "group1")
            .Build();

        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(10)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}