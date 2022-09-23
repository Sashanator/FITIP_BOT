using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Constants;
using TelegramBot.Controllers;

namespace TelegramBot.Handlers;

public static class MessageHandler
{
    public static async Task HandleMessages(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        var text = message.Text;
        if (text == null)
        {
            Log.Warning(string.Format(LogConstants.LogFormat,
                "MessageHandler", "HandleMessages", "Message.Text", ""));
            return;
        }

        var answer = text switch
        {
            Questions.Question_1 => Answers.Answer_1,
            Questions.Question_2 => Answers.Answer_2,
            Questions.Question_3 => Answers.Answer_3,
            Questions.Question_4 => Answers.Answer_4,
            Questions.Question_5 => Answers.Answer_5,
            Questions.Question_6 => Answers.Answer_6,
            Questions.Question_7 => Answers.Answer_7,
            Questions.Question_8 => Answers.Answer_8,
            Questions.Question_9 => Answers.Answer_9,
            Questions.Question_10 => Answers.Answer_10,
            Questions.Question_11 => Answers.Answer_11,
            Questions.Question_12 => Answers.Answer_12,
            Questions.Question_13 => Answers.Answer_13,
            Questions.Question_14 => Answers.Answer_14,
            Questions.Question_15 => Answers.Answer_15,
            Questions.Question_16 => Answers.Answer_16,
            Questions.Question_17 => Answers.Answer_17,
            Questions.Question_18 => Answers.Answer_18,
            Questions.Question_19 => Answers.Answer_19,
            Questions.Question_20 => Answers.Answer_20,
            Questions.Question_21 => Answers.Answer_21,
            Questions.Question_22 => Answers.Answer_22,
            Questions.Question_23 => Answers.Answer_23,
            Questions.Question_24 => Answers.Answer_24,
            Questions.Question_25 => Answers.Answer_25,
            _ => $"Я тебя не понял, *{message.From?.FirstName}*\\!"
        };

        await botClient.SendTextMessageAsync(
            message.Chat,
            answer,
            parseMode: ParseMode.MarkdownV2,
            disableNotification: false,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);

        var random = new Random();
        var randomStickerId = AppController.StickerIds[random.Next(0, AppController.StickerIds.Count)];
        await botClient.SendStickerAsync(
            message.Chat,
            sticker: randomStickerId,
            cancellationToken: cancellationToken);

        Log.Information(FormatLogFromMessage(message));
    }

    private static string FormatLogFromMessage(Message message)
    {
        if (message.From == null)
        {
            Log.Warning(string.Format(LogConstants.LogFormat,
                "MessageHandler", "FormatLogFromMessage", "Message.From", ""));
            return string.Empty;
        }
        var user = AppController.Users.FirstOrDefault(u => u.UserInfo.Id == message.From.Id);
        return user == null ? string.Empty : $"[{user.UserInfo.Id}] {user.UserInfo.FirstName} {user.UserInfo.LastName} ({user.UserInfo.Username}): {message.Text}";
    }
}