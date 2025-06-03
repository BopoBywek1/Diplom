using Diplom.BaseData;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Diplom.Reg_Avto
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переменные
            string name = Name.Text.Trim();
            string surname = Surname.Text.Trim();
            string nickname = Nickname.Text.Trim();
            string ageText = Age.Text.Trim();
            string password = Password.Password;
            string confirmPassword = ConfirmPassword.Password;
            
         
            // Объявление БД
            using (var db = new DiplomEntities1())
            {
                // Выбор пола
                int? genderId = null;

                if (Male.IsChecked == true)
                    genderId = 1;
                else if (Female.IsChecked == true)
                    genderId = 2;

                if (genderId == null)
                {
                    MessageBox.Show("Выберите пол.");
                    return;
                }

                var genderEntity = db.Gender.FirstOrDefault(g => g.Id_gender == genderId.Value);

                if (genderEntity == null)
                {
                    MessageBox.Show("Неверный выбор пола.");
                    return;
                }

                //Выбор Роли(админ и сотрудники)
                var defaultRole = db.Role.FirstOrDefault(r => r.Rank == "Сотрудник");

                if (defaultRole == null)
                {
                    MessageBox.Show("Роль 'Сотрудник' не найдена в базе данных.");
                    return;
                }
                // Валидация
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(ageText) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword)  ||
                genderId == null)
                {
                    MessageBox.Show("Заполните все поля и выберите пол.");
                    return;
                }

                // Проверка возраста
                if (!int.TryParse(ageText, out int age) || age <= 0)
                {
                    MessageBox.Show("Введите корректный возраст.");
                    return;
                }

                // Проверка на соотвествия пароля 
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают.");
                    return;
                }
                // Проверка на совпадение никнейма
                 bool userExists = db.User.Any(u => u.Nickname == nickname);
                 if (userExists)
                    {
                        MessageBox.Show("Пользователь с таким никнеймом уже существует.");
                        return;
                    }
                
                 // Хеширования пароля
                 string passwordHash = PasswordHelper.ComputeSha256Hash(password);

                 // Ввод данных в БД
                 var newUser = new User
                 {
                 Name = name,
                 Surname = surname,
                 Nickname = nickname,
                 Age = age,                    
                 Gender_id = genderEntity.Id_gender,                      
                 Password = passwordHash,
                 Role_id = defaultRole.Id_role,                
                 Department_id = null,                          
                 };    
                
                    db.User.Add(newUser);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно!");                
                
                // Переход на окно входа 
                var FirstWindow = new Diplom.Avtorization();
                 FirstWindow.Show();
                 this.Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var FirstWindow = new Diplom.Avtorization();
            FirstWindow.Show();
            this.Close();
        }
    }
}
 
