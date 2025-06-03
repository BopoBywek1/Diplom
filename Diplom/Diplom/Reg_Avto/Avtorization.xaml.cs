using Diplom;
using Diplom.BaseData;
using Diplom.Reg_Avto;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Diplom
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Avtorization : Window
    {
        public Avtorization()
        {
            InitializeComponent();
            

        }
     
        // Для проверки с базой данной
        public class AuthService
        {
            public bool Login(string nickname, string password)
            {
                var Password = PasswordHelper.ComputeSha256Hash(password);

                using (var db = new DiplomEntities1())
                {
                    var user = db.User.FirstOrDefault(u => u.Nickname == nickname && u.Password == password);
                    return user != null;
                }
            }
        }

        // Для проверки входа и перехода на новую страницу
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string nickname = Nickname.Text.Trim();
            string password = Password.Password;
            string password2 = VisiblePassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(nickname) && (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(password2)))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }

            string hashedPassword = PasswordHelper.ComputeSha256Hash(password);
            string hashed2Password = PasswordHelper.ComputeSha256Hash(password2);

            using (var db = new DiplomEntities1())
            {
                var user = db.User.FirstOrDefault(u => u.Nickname == nickname && (u.Password == hashedPassword || u.Password == hashed2Password));

                if (user != null)
                {
                    MessageBox.Show($"Добро пожаловать, {user.Name}!");
                    var MainWindow = new AfterRegAvto.Window1(nickname);
                    MainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
        }
        

        // Для показа пароля
        private bool _isPasswordVisible = false;

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {

                VisiblePassword.Text = Password.Password;
                VisiblePassword.Visibility = Visibility.Visible;
                Password.Visibility = Visibility.Collapsed;
                ToggleButton.Content = "Скрыть пароль";
            }
            else
            {
                Password.Password = VisiblePassword.Text;
                VisiblePassword.Visibility = Visibility.Collapsed;
                Password.Visibility = Visibility.Visible;
                ToggleButton.Content = "Показать пароль";
            }
        }

        // Для пустого никнейма
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Nickname.Text = string.Empty; 
        }

        private void Nickname_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //Регистрация
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var secondWindow = new Diplom.Reg_Avto.Window1();
            secondWindow.Show();
            this.Close();
        }

        //
        //  Забыл пароль 
        //
       

        private void Label_Click(object sender, MouseButtonEventArgs e)
        {
            
        }

    }

}
