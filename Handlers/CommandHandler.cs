using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Controllers;

namespace TelegramBot.Handlers;

public static class CommandHandler
{
    private static async Task SendMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new [] { InlineKeyboardButton.WithCallbackData(text: "Team", callbackData: "team"), },
            new [] { InlineKeyboardButton.WithCallbackData(text: "Map", callbackData: "map"), },
            new [] { InlineKeyboardButton.WithCallbackData(text: "FAQ", callbackData: "faq") }
        });
        await botClient.SendTextMessageAsync(
            chatId: message.Chat,
            text: "Добро пожаловать на посвящение в первокурсники! Главное меню:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    private static async Task SendAdminMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            new [] { InlineKeyboardButton.WithCallbackData(text: "Teams", callbackData: "teams"), },
            new [] { InlineKeyboardButton.WithCallbackData(text: "Score", callbackData: "score"), },
            new [] { InlineKeyboardButton.WithCallbackData(text: "History", callbackData: "history") }
        });
        await botClient.SendTextMessageAsync(
            chatId: message.Chat,
            text: "Админская панель",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public static async Task HandleCommands(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        switch (message.Text?.ToLower())
        {
            case "/start":
                if (message.From == null) return;
                if (!AppController.ParticipantIds.Contains(message.From.Id))
                { // Check if user is admin
                    await botClient.SendTextMessageAsync(
                        message.Chat,
                        "У вас недостаточно прав!",
                        disableNotification: false,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await SendMainMenu(botClient, message, cancellationToken);
                }
                break;

            case "/admin":
                if (message.From == null) return;
                if (!AppController.AdminIds.Contains(message.From.Id))
                { // Check if user is admin
                    await botClient.SendTextMessageAsync(
                        message.Chat,
                        "У вас недостаточно прав!",
                        disableNotification: false,
                        replyToMessageId: message.MessageId,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    await SendAdminMenu(botClient, message, cancellationToken);
                }
                break;

            default:
                await botClient.SendTextMessageAsync(
                    message.Chat,
                    $"Я тебя не понял, *{message.From?.FirstName}*\\!",
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: false,
                    replyToMessageId: message.MessageId,
                    cancellationToken: cancellationToken);
                break;
        }
    }
}