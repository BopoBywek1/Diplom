using Diplom_F2_.BaseDate;
using System;
using System.Collections.Generic;
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
using Diplom_F2_.UserPanel;

namespace Diplom_F2_.UserPanel
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class OrderDetailsWindow : Window
    {

        private Order _order;

        public OrderDetailsWindow(Order order)
        {
            InitializeComponent();
            _order = order;

            // Заполняем поля
            TitleText.Text = $"Заголовок: {_order.Title}";
            MessageText.Text = $"Сообщение: {_order.Message}";
            DateText.Text = $"Дата создания: {_order.Order_date.ToShortDateString()}";
            StatusText.Text = $"Статус: {_order.Status_order?.Name_status ?? "Неизвестно"}";

            if (_order.Address_id != null)
            {
                PhoneText.Text = $"Телефон: {_order.Number}";
                CabinetText.Text = $"Кабинет: {_order.Cabinet}";
                AddressText.Text = $"Адрес: {_order.Locate_order.AddressO}";
                DepartmentText.Text = $"Департамент: {_order.Locate_order.Departament?.Name_departament?? "Неизвестно"}";
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }    
}
