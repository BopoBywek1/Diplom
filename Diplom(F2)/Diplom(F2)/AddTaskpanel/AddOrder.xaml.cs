using Diplom_F2_.BaseDate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Diplom_F2_.MainWindow;

namespace Diplom_F2_.AddTaskpanel
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private bool _isEditing = false;
        private DiplomEntities1 _db = new DiplomEntities1();
        public Window1()
        {
            InitializeComponent();

            StatusComboBox.ItemsSource = _db.Status_order.ToList();

            var currentDepartamentId = SessionManager.CurrentUser.Departament_id;
            var currentDepartament = _db.Departament.FirstOrDefault(d => d.Id_departament == currentDepartamentId);
            if (currentDepartament != null)
            {
                DepartmentComboBox.ItemsSource = new List<Departament> { currentDepartament };
                DepartmentComboBox.SelectedValue = currentDepartament.Id_departament;
                DepartmentComboBox.IsEnabled = false;
            }

            // Фильтруем адреса по департаменту пользователя
            var addresses = _db.Locate_order
                .Where(lo => lo.Departament_id == currentDepartamentId)
                .ToList();

            AddressComboBox.ItemsSource = addresses;
            AddressComboBox.DisplayMemberPath = "AddressO";
            AddressComboBox.SelectedValuePath = "Id_locate_order";

            OrderDatePicker.SelectedDate = DateTime.Now;

            var inProgressStatus = _db.Status_order.FirstOrDefault(s => s.Name_status == "В процессе");
            if (inProgressStatus != null)
                StatusComboBox.SelectedValue = inProgressStatus.Id_status;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
            string.IsNullOrWhiteSpace(MessageTextBox.Text) ||
            !OrderDatePicker.SelectedDate.HasValue ||
            StatusComboBox.SelectedValue == null ||
            string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
            string.IsNullOrWhiteSpace(CabinetTextBox.Text) ||
            AddressComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля!");
                return;  // Выходим из метода, не сохраняя
            }
            try
            {
                var currentDepartamentId = SessionManager.CurrentUser.Departament_id;

                // Находим пользователя с Role == 1 в том же департаменте
                var userWithRole1 = _db.User.FirstOrDefault(u => u.Departament_id == currentDepartamentId && u.Role_id == 1);

                if (userWithRole1 == null)
                {
                    MessageBox.Show("Пользователь с ролью 1 в вашем департаменте не найден.");
                    return;
                }

                var newOrder = new Order
                {
                    Title = TitleTextBox.Text,
                    Message = MessageTextBox.Text,
                    Order_date = OrderDatePicker.SelectedDate ?? DateTime.Now,
                    Status_id = (int)(StatusComboBox.SelectedValue ?? 0),
                    Number = PhoneTextBox.Text,
                    Cabinet = CabinetTextBox.Text,
                    Address_id = (int)(AddressComboBox.SelectedValue ?? 0),
                    User_id = userWithRole1.Id_user
                };
              
                // Добавляем заказ
                _db.Order.Add(newOrder);
                _db.SaveChanges();

                MessageBox.Show("Заказ успешно добавлен!");

                TitleTextBox.Clear();
                MessageTextBox.Clear();
                PhoneTextBox.Clear();
                CabinetTextBox.Clear();
                AddressComboBox.SelectedIndex = -1;
                StatusComboBox.SelectedIndex = -1;
                OrderDatePicker.SelectedDate = DateTime.Now;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void OrderDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;

            // Получаем TextBox из шаблона DatePicker
            var textBox = (TextBox)datePicker.Template.FindName("PART_TextBox", datePicker);
            if (textBox != null)
            {
                // Запретить редактирование (readonly)
                textBox.IsReadOnly = true;
            }
            var button = (Button)datePicker.Template.FindName("PART_Button", datePicker);
            if (button != null)
            {
                button.Visibility = Visibility.Collapsed;
            }
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d+]");


        }
        private void PhoneTextBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            string phone = PhoneTextBox.Text;
            string digits = Regex.Replace(phone, @"\D", "");

            if (digits.Length != 11 || !digits.StartsWith("7"))
            {
                MessageBox.Show("Введите корректный номер телефона в формате +7 (XXX) XXX-XX-XX");

                // Отменяем потерю фокуса, чтобы поле осталось в фокусе
                e.Handled = true;
            }
        }
        private void PhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isEditing) return;  // Предотвращаем рекурсию
            _isEditing = true;

            var textBox = sender as TextBox;
            string digits = Regex.Replace(textBox.Text, @"[^\d]", "");  // Оставляем только цифры

            if (digits.StartsWith("7"))
                digits = "+" + digits;
            else if (!digits.StartsWith("+7"))
                digits = "+7" + digits; // По умолчанию добавляем +7, если нет

            // Форматируем строку: +7 (123) 456-78-90
            if (digits.Length > 2)
            {
                string formatted = "+7 ";
                if (digits.Length > 3)
                    formatted += $"({digits.Substring(2, Math.Min(3, digits.Length - 2))}) ";
                else if (digits.Length > 2)
                    formatted += $"({digits.Substring(2)}) ";

                // Следующие 3 цифры
                if (digits.Length > 5)
                    formatted += digits.Substring(5, Math.Min(3, digits.Length - 5));

                // Следующие 2 цифры с дефисом
                if (digits.Length > 7)
                    formatted += "-" + digits.Substring(8, Math.Min(2, digits.Length - 8));

                // Последние 2 цифры с дефисом
                if (digits.Length > 9)
                    formatted += "-" + digits.Substring(10, Math.Min(2, digits.Length - 10));


                textBox.Text = formatted;
            }
            else
            {
                textBox.Text = digits;
            }

            // Ставим курсор в конец
            textBox.CaretIndex = textBox.Text.Length;

            _isEditing = false;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var MainWindow = new Diplom_F2_.MainWindow();
            MainWindow.Show();
            this.Close();
        }
    }
}
