using Diplom_F2_.BaseDate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Diplom_F2_;
using System.Data.Entity;

namespace Diplom_F2_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (var db = new DiplomEntities1())
            {
                var users = db.User.ToList();

                foreach (var user in users)
                {
                    if (!string.IsNullOrWhiteSpace(user.Password) && user.Password.Length < 40) // чтобы не хешировать повторно
                    {
                        user.Password = PasswordHelper.ComputeSha256Hash(user.Password);
                    }
                }

                db.SaveChanges();
            }

        }
        public static class SessionManager
        {
            public static User CurrentUser { get; set; }
            public static int? CurrentDepartmentId { get; set; }

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
            string nickname = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;
            string password2 = PasswordBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(nickname) && (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(password2)))
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }
           
            string hashedPassword = PasswordHelper.ComputeSha256Hash(password);
            string hashed2Password = PasswordHelper.ComputeSha256Hash(password2);

            using (var db = new DiplomEntities1())
            {
                var user = db.User   
                    .FirstOrDefault(u => u.Nickname == nickname && (u.Password == hashedPassword || u.Password == hashed2Password));
                
                if (user != null)
                {
                    SessionManager.CurrentUser = user;
                    SessionManager.CurrentDepartmentId = user.Departament_id;
                    MessageBox.Show($"Добро пожаловать, {user.Name}!");

                    switch (user.Role_id)
                    {
                        case 1:
                            // Обычный пользователь
                            var userWindow = new UserPanel.Window1();
                            userWindow.Show();
                            this.Close();
                            break;

                        case 2:
                            // Администратор
                            var adminWindow = new AdminPanel.Window1();
                            adminWindow.Show();
                            this.Close();
                            break;
                        case 3:
                            var AddTaskWindow = new AddTaskpanel.Window1();
                            AddTaskWindow.Show();
                            this.Close();
                            break;

                        default:
                            MessageBox.Show("Неизвестная роль пользователя.");
                            break;
                    }

                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
        }


        private void UsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private bool _isPasswordVisible = false;
        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {

                PasswordBox2.Text = PasswordBox.Password;
                PasswordBox2.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                EyesPassword.Content = "!👁";

            }
            else
            {
                PasswordBox.Password = PasswordBox2.Text;
                PasswordBox2.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                EyesPassword.Content = "👁";

            }
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                PasswordBox2.Text = PasswordBox.Password;
            }
        }
      
    }
}
