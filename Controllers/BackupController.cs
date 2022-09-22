using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Serilog;
using TelegramBot.Jobs;
using TelegramBot.Models;

namespace TelegramBot.Controllers;

public static class BackupController
{
    public const string BACKUP_FILE_PATH = @"Backup/backup.json";

    public static void Restore()
    {
        // TODO: Check if dir does not exist
        if (!Directory.Exists("Backup"))
        {
            Log.Error("Backup Directory does not exist");
            Console.WriteLine("Restoring system failed!");
            return;
        }

        var jsonString = File.ReadAllText(BACKUP_FILE_PATH);
        var backup = JsonConvert.DeserializeObject<Backup>(jsonString);
        if (backup == null) throw new Exception("Backup file was empty");

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
            .WithIdentity("Backup Job", "Backup")
            .Build();

        // Trigger the job to run now, and then every 40 seconds
        var trigger = TriggerBuilder.Create()
            .WithIdentity("Backup Job", "Backup")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(60)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}