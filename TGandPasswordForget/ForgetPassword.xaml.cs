using Diplom.BaseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;



namespace Diplom.Reg_Avto
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            Task.Run(() => StartTelegramBot());
        }
        // Бот
        private async Task StartTelegramBot()
        {
            string token = "7506585343:AAHo_zsewG0zP95D-SujBdVWwlClJknX1Xk";
            var bot = new TelegramBotClient(token);

            // Использование старой формы using для CancellationTokenSource
            CancellationTokenSource cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // получаем все обновления
            };

            // Старт приема сообщений
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await bot.GetMeAsync();
            Console.WriteLine($"Бот запущен: @{me.Username}");

            // Явное освобождение ресурсов CancellationTokenSource
            cts.Dispose();
        }


        static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            // Заменим паттерн-матчинг на стандартные проверки
            if (update.Message == null || update.Message.Text == null) return;


            var chatId = update.Message.Chat.Id;

            await bot.SendTextMessageAsync(
                chatId,
                $"Ваш ChatId: {chatId}\nВведите его в форму восстановления пароля."
            );
        }

        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }


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




        //Отправка кода
        private DiplomEntities1 db = new DiplomEntities1(); // это ваш EDMX-контекст
        private BaseData.User currentUser;

        private async void SendCode_Click(object sender, RoutedEventArgs e)
        {
            string nickname = Nickname_password.Text;
            string codeName = Code_name.Text;

            currentUser = db.User
                .FirstOrDefault(u => u.Nickname == nickname && u.Code_name == codeName);

            if (currentUser == null || string.IsNullOrEmpty(currentUser.ChatId))
            {
                MessageBox.Show("Пользователь не найден или не задан ChatId.");
                return;
            }

            string code = new Random().Next(100000, 999999).ToString();
            currentUser.VerificationCode = code;
            currentUser.CodeExpiresAt = DateTime.UtcNow.AddMinutes(5);

            db.SaveChanges();

            var telegram = new TelegramService("7506585343:AAHo_zsewG0zP95D-SujBdVWwlClJknX1Xk");
            await telegram.SendCodeAsync(currentUser.ChatId, code);

            MessageBox.Show("Код отправлен!");
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = Code.Text;
            string newPassword = NewPassword.Password;

            if (currentUser == null ||
                currentUser.VerificationCode != enteredCode ||
                currentUser.CodeExpiresAt < DateTime.UtcNow)
            {
                MessageBox.Show("Неверный или просроченный код.");
                return;
            }

            currentUser.Password = newPassword; // желательно захешировать!
            currentUser.VerificationCode = null;
            currentUser.CodeExpiresAt = null;

            db.SaveChanges();

            MessageBox.Show("Пароль обновлён!");
        }
    }

}
