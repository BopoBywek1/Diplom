using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;



public class TelegramService
    {
        private readonly TelegramBotClient _bot;

        public TelegramService(string token)
        {
            _bot = new TelegramBotClient(token);
        }

        public async Task SendCodeAsync(string chatId, string code)
        {
            await _bot.SendTextMessageAsync(chatId, $"Ваш код восстановления: {code}");
        }
    }

