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
        if (text == null) return;

        var answer = text switch
        {
            Questions.Question_1 => Answers.Answer_1,
            Questions.Question_2 => Answers.Answer_2,
            Questions.Question_3 => Answers.Answer_3,
            Questions.Question_4 => Answers.Answer_4,
            Questions.Question_5 => Answers.Answer_5,
            Questions.Question_6 => Answers.Answer_6,
            Questions.Question_7 => Answers.Answer_7,
            _ => $"Я тебя не понял, *{message.From?.FirstName}*\\!"
        };

        await botClient.SendTextMessageAsync(
            message.Chat,
            answer,
            parseMode: ParseMode.MarkdownV2,
            disableNotification: false,
            replyToMessageId: message.MessageId,
            cancellationToken: cancellationToken);

        Log.Information(FormatLogFromMessage(message));
    }

    private static string FormatLogFromMessage(Message message)
    {
        if (message.From == null) return string.Empty;
        var user = AppController.Users.FirstOrDefault(u => u.UserInfo.Id == message.From.Id);
        return user == null ? string.Empty : $"[{user.UserInfo.Id}] {user.UserInfo.FirstName} {user.UserInfo.LastName} ({user.UserInfo.Username}): {message.Text}";
    }
}