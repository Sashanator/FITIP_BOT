using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Handlers;

public static class CommandHandler
{
    public static async Task SendMainMenu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Team", callbackData: "team"),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Map", callbackData: "map"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "FAQ", callbackData: "faq") 
            }
        });
        await botClient.SendTextMessageAsync(
            chatId: message.Chat,
            text: "Добро пожаловать на посвящение в первокурсники! Главное меню:",
            replyMarkup: inlineKeyboard,
            cancellationToken: cancellationToken);
    }

    public static async Task HandleCommands(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        switch (message.Text?.ToLower())
        {
            case "/start":
                await SendMainMenu(botClient, message, cancellationToken);
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