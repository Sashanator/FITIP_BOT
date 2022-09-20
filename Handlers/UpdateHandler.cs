using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBot.Constants;
using TelegramBot.Controllers;
using TelegramBot.Helpers;

namespace TelegramBot.Handlers;

public static class UpdateHandler
{
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message == null)
                    {
                        Log.Error(string.Format(LogConstants.LogFormat,
                            "UpdateHandler", "HandleUpdateAsync", "Update.Message", ""));
                        return;
                    }

                    await HandleUpdateTypeMessage(botClient, update.Message, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    AppHelper.AddNewUser(update.CallbackQuery!.From, update.CallbackQuery.Message);
                    await CallbackHandler.HandleCallback(botClient, update, cancellationToken);
                    break;
                case UpdateType.Unknown:
                    break;
                case UpdateType.InlineQuery:
                    break;
                case UpdateType.ChosenInlineResult:
                    break;
                case UpdateType.EditedMessage:
                    break;
                case UpdateType.ChannelPost:
                    break;
                case UpdateType.EditedChannelPost:
                    break;
                case UpdateType.ShippingQuery:
                    break;
                case UpdateType.PreCheckoutQuery:
                    break;
                case UpdateType.Poll:
                    break;
                case UpdateType.PollAnswer:
                    break;
                case UpdateType.MyChatMember:
                    break;
                case UpdateType.ChatMember:
                    break;
                case UpdateType.ChatJoinRequest:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    private static async Task HandleUpdateTypeMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "UpdateHandler", "HandleUpdateTypeMessage", "Message.From", ""));
            return;
        }

        if (message.Text == null)
        {
            Log.Error(string.Format(LogConstants.LogFormat,
                "UpdateHandler", "HandleUpdateTypeMessage", "Message.Text", "MESSAGE FROM BOT"));
            return;
        }
        
        var userId = message.From.Id;

        if (!AppController.ParticipantIds.Contains(userId) &&
            !AppController.AdminIds.Contains(userId) &&
            !message.From.IsBot)
        {
            await botClient.SendTextMessageAsync(
                message.Chat,
                "Прошу нас простить, вы не можете пользоваться системой",
                cancellationToken: cancellationToken
            );

            return;
        }
        
        AppHelper.AddNewUser(message.From, message);

        if (message.Text.ToLower().StartsWith('/'))
            await CommandHandler.HandleCommands(botClient, message, cancellationToken);
        else
            await MessageHandler.HandleMessages(botClient, message, cancellationToken);
    }
}