using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Constants;
using TelegramBot.Controllers;
using TelegramBot.Extensions;
using TelegramBot.Models;
using File = System.IO.File;

namespace TelegramBot.Handlers;

public static class CallbackHandler
{
    private const int STAGE_COUNT = 3; // Number of floors in University
    private const int TEAMS_IN_ROW = 8;
    private const int MEMBERS_PER_TEAM = 1;
    private static int _regNumber;

    public static async Task HandleCallback(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        if (callbackQuery == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleCallback", "Update.CallbackQuery"));
            return;
        }
        switch (callbackQuery.Data)
        {
            case "team":
                await HandleTeamCallback(botClient, callbackQuery, cancellationToken);
                break;
            case "map":
                await HandleMapCallback(botClient, callbackQuery, cancellationToken);
                break;
            case "faq":
                await HandleFaqCallback(botClient, callbackQuery, cancellationToken);
                break;
            case "teams":
                await SendTeamsMenu(botClient, callbackQuery, cancellationToken);
                break;
            case "score":
                await SendScoreMessage(botClient, callbackQuery, cancellationToken);
                break;
            case "history":
                // TODO: Make history function
                break;
            case "gg":
                break;
            default:
                await HandleTeamsCallback(botClient, callbackQuery, cancellationToken);
                break;
        }
    }

    private static async Task SendScoreMessage(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "SendScoreMessage", "CallbackQuery.Message"));
            return;
        }
        var teams = AppController.Teams.OrderBy(r => r.Score).ToList();
        var result = teams.Aggregate("", (current, team) => current + $"Team \\#{team.Id}: *{team.Score}*\n");
        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat,
            result.Length > 0 ? result : "Zero Teams",
            parseMode: ParseMode.MarkdownV2,
            disableNotification: false,
            replyToMessageId: callbackQuery.Message.MessageId,
            cancellationToken: cancellationToken);
    }

    private static async Task SendTeamsMenu(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "SendTeamsMenu", "CallbackQuery.Message"));
            return;
        }
        InlineKeyboardMarkup inlineKeyboard = new(GetTeamsInlineKeyboard(AppController.Teams.Count));
        await botClient.SendTextMessageAsync(
            chatId: callbackQuery.Message.Chat,
            text: "Выберите команду",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetTeamsInlineKeyboard(int teamsCount)
    {
        var result = new List<List<InlineKeyboardButton>>();
        var rowCount = (teamsCount - 1) / TEAMS_IN_ROW + 1;
        var curTeam = 1;
        for (var i = 0; i < rowCount; i++)
        {
            var newRow = new List<InlineKeyboardButton>();

            for (var j = 0; j < TEAMS_IN_ROW; j++)
            {
                if (curTeam > teamsCount)
                {
                    for (var k = j; k < TEAMS_IN_ROW; k++)
                    {
                        newRow.Add(InlineKeyboardButton.WithCallbackData(text: " ", callbackData: "gg"));
                    }
                    break;
                }

                newRow.Add(InlineKeyboardButton.WithCallbackData(text: $"#{curTeam}", callbackData: $"t{curTeam}"));
                curTeam++;
            }

            result.Add(newRow);
        }

        return result;
    }

    private static async Task HandleTeamsCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var data = callbackQuery.Data;
        var message = callbackQuery.Message;
        if (data == null || message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleTeamsCallback", "CallbackQuery.Data || CallbackQuery.Message"));
            return;
        }
        if (data.Contains('t'))
        {
            var teamId = data.MySubstring(data.IndexOf('t') + 1, data.Length);
            InlineKeyboardMarkup inlineKeyboard = new(GetTeamScoreKeyboard(teamId));
            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                text: $"Очки для команды \\#*{teamId}*",
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
        else if (data.Contains('-'))
        { // TODO: Refactor it
            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('#') + 1, data.IndexOf('-')));
            var points = Convert.ToInt32(data.MySubstring(data.IndexOf('-') + 1, data.Length));
            var team = AppController.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                Log.Error(string.Format(LogConstants.LogFormat,
                    "CallbackHandler", "HandleTeamsCallback", "Team"));
                return;
            } // GG ???
            team.Score = team.Score - points > 0 ? team.Score - points : 0;
        }
        else if (data.Contains('+'))
        { // TODO: Refactor it
            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('#') + 1, data.IndexOf('+')));
            var points = Convert.ToInt32(data.MySubstring(data.IndexOf('+') + 1, data.Length));
            var team = AppController.Teams.FirstOrDefault(t => t.Id == teamId);
            if (team == null)
            {
                Log.Error(string.Format(LogConstants.LogFormat,
                    "CallbackHandler", "HandleTeamsCallback", "Team"));
                return;
            } // GG ???
            team.Score += points;
        }
        else if (data.Contains('m'))
        {
            var teamId = Convert.ToInt32(data.MySubstring(data.IndexOf('m') + 1, data.Length));
            var team = AppController.Teams.FirstOrDefault(t => t.Id == teamId);
            
            if (team != null)
            {
                var curUserNumber = 1;
                var text = team.Members.Aggregate($"MEMBERS OF TEAM \\#{teamId}\\:\n", (current, member) => 
                    current + $"\\#{curUserNumber++}\\. *[{member.UserInfo.Username}]* {member.UserInfo.FirstName} {member.UserInfo.LastName}\n");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat,
                    text,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: cancellationToken);
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat,
                "Unknown command!",
                cancellationToken: cancellationToken);
        }
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetTeamScoreKeyboard(string teamId)
    {
        var pointsToAdd = new List<InlineKeyboardButton>();
        var pointsToRemove = new List<InlineKeyboardButton>();
        for (var i = 0; i < 5; i++)
        {
            pointsToAdd.Add(InlineKeyboardButton.WithCallbackData(text: $"+{i + 1}", callbackData: $"#{teamId}+{i + 1}"));
            pointsToRemove.Add(InlineKeyboardButton.WithCallbackData(text: $"-{i + 1}", callbackData: $"#{teamId}-{i + 1}"));
        }

        var infoButton = new List<InlineKeyboardButton>
        {
            InlineKeyboardButton.WithCallbackData(text: "Members", callbackData: $"m{teamId}")
        };
        return new List<List<InlineKeyboardButton>> {pointsToAdd, pointsToRemove, infoButton};
    }

    private static async Task HandleMapCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleMapCallback", "CallbackQuery.Message"));
            return;
        }
        
        var mapPictures = new List<InputMediaPhoto>();
        for (var i = 0; i < STAGE_COUNT; i++)
        {
            var fileName = $"stage_{i}.jpg";
            var filePath = Path.Combine(Environment.CurrentDirectory, @"Resources/", fileName);
            var fileStream = File.OpenRead(filePath);
            mapPictures.Add(new InputMediaPhoto(new InputMedia(fileStream, $"Stage #{i + 1}")));
        }

        await botClient.SendMediaGroupAsync(
            callbackQuery.Message.Chat,
            media: new IAlbumInputMedia[]
            {
                mapPictures[0], mapPictures[1], mapPictures[2]
            },
            cancellationToken: cancellationToken);
    }

    private static async Task HandleFaqCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleFaqCallback", "CallbackQuery.Message"));
            return;
        }
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                            {
                                new KeyboardButton[] { Questions.Question_1 },
                                new KeyboardButton[] { Questions.Question_2 },
                                new KeyboardButton[] { Questions.Question_3 },
                                new KeyboardButton[] { Questions.Question_4 },
                                new KeyboardButton[] { Questions.Question_5 },
                                new KeyboardButton[] { Questions.Question_6 },
                                new KeyboardButton[] { Questions.Question_7 },
                            })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(
            callbackQuery.Message.Chat,
            "Выбери свой вопрос",
            replyMarkup: replyKeyboardMarkup,
            cancellationToken: cancellationToken);
    }

    private static async Task HandleTeamCallback(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Message == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleTeamCallback", "CallbackQuery.Message"));
            return;
        }

        var user = AppController.Users.FirstOrDefault(u => u.UserInfo.Id == callbackQuery.From.Id);

        if (user == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "CallbackHandler", "HandleTeamCallback", "User"));
            return; // Throw an exception
        } 

        if (user.TeamId == null)
        {
            var teamId = _regNumber++ / MEMBERS_PER_TEAM + 1;
            user.TeamId = teamId;
            var team = AppController.Teams.FirstOrDefault(t => t.Id == teamId);
            // Create new team if it is not exist and add user to it
            if (team == null)
            {
                AppController.Teams.Add(new Team(teamId, user));
            }
            else
            {
                // Add user to team if it is exists and he is not already there
                if (team.Members.FirstOrDefault(a => a.UserInfo.Id == user.UserInfo.Id) == null)
                {
                    team.Members.Add(user);
                }
            }

            var message = await botClient.SendTextMessageAsync(
                callbackQuery.Message.Chat,
                $"Номер твой команды: {user.TeamId}",
                disableNotification: false,
                replyToMessageId: callbackQuery.Message.MessageId,
                cancellationToken: cancellationToken);

            await botClient.PinChatMessageAsync(callbackQuery.From.Id, message.MessageId, cancellationToken: cancellationToken);
        }
    }
}