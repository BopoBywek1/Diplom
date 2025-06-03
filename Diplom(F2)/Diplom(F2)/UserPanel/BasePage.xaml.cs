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
using System.Windows.Threading;
using static Diplom_F2_.MainWindow;
using System.Data.Entity;
using Diplom_F2_.UserPanel;




namespace Diplom_F2_.UserPanel
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Queue<Order> _orderQueue = new Queue<Order>();
        private DispatcherTimer _timer;
        private int _currentUserId = SessionManager.CurrentUser.Id_user;
        private int CurrentDepartmentId = SessionManager.CurrentUser.Departament_id;
        public Window1()
        {
            InitializeComponent();


            
            ShowNickname();
            LoadOrders(); // загрузка данных из БД
            DisplayAllOrders();
            LoadDepartments();
            LoadStatuses();
            
        }
        private void RefreshMyOrders()
        {
            MessagesPanel.Children.Clear();
            _orderQueue.Clear();
            LoadMyOrders();
            DisplayAllOrders();
        }
        private void LoadOrders()
        {
            MessagesPanel.Children.Clear();   // Очистим панель
            _orderQueue.Clear();              // Очистим очередь

            using (var db = new DiplomEntities1())
            {

                // Получаем ID статуса "В процессе"
                int inProgressStatusId = db.Status_order
                    .FirstOrDefault(s => s.Name_status == "В процессе")?.Id_status ?? 0;

                int departmentId = SessionManager.CurrentUser.Departament_id;
                // Загружаем поручения текущего пользователя со статусом "В процессе"
                var orders = db.Order
                    .Where(o => o.Status_id == inProgressStatusId &&
                db.Locate_order.Any(l => l.Id_locate_order == o.Address_id &&
                                         l.Departament_id == CurrentDepartmentId))
                                  
                .Include(o => o.Status_order)
                .Include(o => o.Locate_order.Departament)                    
                .ToList();


                int ordersCount = orders.Count;
                TotalAmountTextBlock.Text = $"Моих поручений (в процессе): {ordersCount}";

                foreach (var order in orders)
                    _orderQueue.Enqueue(order);
            }

        }

        private void DisplayAllOrders(bool onlyOpenButton = false)
        {
            while (_orderQueue.Count > 0)
            {
                var order = _orderQueue.Dequeue();
                AddOrderToPanel(order, onlyOpenButton);
            }
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_orderQueue.Count > 0)
            {
                AddOrderToPanel(_orderQueue.Dequeue());
            }
            else
            {
                _timer.Stop();
            }
        }

        private void AddOrderToPanel(Order order, bool onlyOpenButton = false)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock { Text = $"Заголовок: {order.Title}", FontWeight = FontWeights.Bold });
            stack.Children.Add(new TextBlock { Text = $"Сообщение: {order.Message}" });
            stack.Children.Add(new TextBlock { Text = $"Дата: {order.Order_date.ToShortDateString()}" });


            

            // 🟡 Статус
            stack.Children.Add(new TextBlock
            {
                Text = $"Статус: {order.Status_order?.Name_status ?? "Неизвестно"}",
                FontStyle = FontStyles.Italic,
                Foreground = Brushes.DarkSlateGray,
                Margin = new Thickness(0, 5, 0, 0)
            });

            if ((order.User_id == _currentUserId) && onlyOpenButton )
            {
                var openButton = new Button
                {
                    Content = "Открыть поручение",
                    Tag = order.Id_order,
                    Background = Brushes.LightBlue,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                openButton.Click += OpenOrder_Click;

                stack.Children.Add(openButton);
            }


            else if (order.User_id == _currentUserId)
            {
                // Это моё поручение — показываем нужные кнопки
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                // ✅ Выполнить
                var completeButton = new Button
                {
                    Content = "Выполнено",
                    Tag = order.Id_order,
                    Background = Brushes.LightGreen,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                completeButton.Click += MarkCompleted_Click;

                // 🚫 Невозможно выполнить
                var impossibleButton = new Button
                {
                    Content = "Невозможно выполнить",
                    Tag = order.Id_order,
                    Background = Brushes.LightGray,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                impossibleButton.Click += MarkImpossible_Click;

                // 🔎 Открыть поручение
                var openButton = new Button
                {
                    Content = "Открыть поручение",
                    Tag = order.Id_order,
                    Background = Brushes.LightBlue
                };
                openButton.Click += OpenOrder_Click;

                buttonPanel.Children.Add(completeButton);
                buttonPanel.Children.Add(impossibleButton);
                buttonPanel.Children.Add(openButton);

                stack.Children.Add(buttonPanel);
            }

            var border = new Border
            {
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                Child = stack
            };

            MessagesPanel.Children.Add(border);
        }
        private void OpenOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                using (var db = new DiplomEntities1())
                {
                    var order = db.Order
                                  .Include(o => o.Status_order)
                                  .FirstOrDefault(o => o.Id_order == orderId);

                    if (order != null)
                    {
                        var detailsWindow = new OrderDetailsWindow(order);
                        detailsWindow.ShowDialog();
                    }
                }
            }
        }
        private void MarkCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                using (var db = new DiplomEntities1())
                {
                    var order = db.Order.FirstOrDefault(o => o.Id_order == orderId && o.User_id == _currentUserId);
                    if (order != null)
                    {
                        int completedStatusId = db.Status_order.FirstOrDefault(s => s.Name_status == "Выполнено")?.Id_status ?? 3;
                        order.Status_id = completedStatusId;
                        db.SaveChanges();

                        MessageBox.Show("Поручение отмечено как выполненное.");
                        RefreshMyOrders();
                    }
                }
            }
        }



        private void MarkImpossible_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                string reason = ShowReasonDialog("Причина невозможности", "Укажите причину, по которой невозможно выполнить поручение:");

                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Причина не указана. Операция отменена.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                using (var db = new DiplomEntities1())
                {
                    var order = db.Order.FirstOrDefault(o => o.Id_order == orderId && o.User_id == _currentUserId);
                    if (order != null)
                    {
                        int impossibleStatusId = db.Status_order.FirstOrDefault(s => s.Name_status == "Невозможно выполнить")?.Id_status ?? 5;
                        order.Status_id = impossibleStatusId;
                        order.Message += $"\n\nПричина невозможности: {reason}";
                        db.SaveChanges();

                        MessageBox.Show("Поручение отмечено как невозможное для выполнения.");
                        RefreshMyOrders();
                    }
                }
            }
        }

        private string ShowReasonDialog(string title, string message)
        {
            // Создаём окно
            var window = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Application.Current.MainWindow,
                WindowStyle = WindowStyle.ToolWindow
            };

            // Основной стек
            var stack = new StackPanel { Margin = new Thickness(10) };

            // Текст сообщения
            stack.Children.Add(new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            });

            // Текстбокс для ввода
            var inputBox = new TextBox { Height = 60, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap };
            stack.Children.Add(inputBox);

            // Кнопки
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };

            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };

            string result = null;

            okButton.Click += (s, e) =>
            {
                result = inputBox.Text;
                window.DialogResult = true;
                window.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                window.DialogResult = false;
                window.Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            stack.Children.Add(buttonPanel);

            window.Content = stack;

            var dialogResult = window.ShowDialog();
            return dialogResult == true ? result : null;
        }




        private void Ellipse_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }

        private void Ellipse_DpiChanged(object sender, DpiChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var MainWindow = new MainWindow();
            MainWindow.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DepartmentComboBox.Visibility = Visibility.Collapsed;
            StatusComboBox.Visibility = Visibility.Collapsed;

            LoadMyOrders();            // также фильтрует по департаменту
            DisplayAllOrders();

        }
        
        private void LoadMyOrders()
        {
            MessagesPanel.Children.Clear();
            _orderQueue.Clear();

            using (var db = new DiplomEntities1())
            {
                int inProgressStatusId = db.Status_order
                    .FirstOrDefault(s => s.Name_status == "В процессе")?.Id_status ?? 0;

                var orders = db.Order
                    .Where(o => o.User_id == _currentUserId &&
                                o.Status_id == inProgressStatusId &&
                                db.Locate_order.Any(l => l.Id_locate_order == o.Address_id &&
                                                         l.Departament_id == CurrentDepartmentId))
                    .Include(o => o.Status_order)
                    .ToList();

                TotalAmountTextBlock.Text = $"Моих поручений (в процессе): {orders.Count}";

                foreach (var order in orders)
                    _orderQueue.Enqueue(order);
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
          

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            DepartmentComboBox.Visibility = Visibility.Visible;
            StatusComboBox.Visibility = Visibility.Visible;

            

            LoadMyFinishedOrders();
            DisplayAllOrders(onlyOpenButton: true);
        }
        private void ShowNickname()
        {
            ViewNick.Text = SessionManager.CurrentUser.Nickname; 
        }
        private void LoadMyFinishedOrders()
        {
            MessagesPanel.Children.Clear();
            _orderQueue.Clear();

            using (var db = new DiplomEntities1())
            {
                var statusIds = db.Status_order
                    .Where(s => s.Name_status == "Выполнено" || s.Name_status == "Невозможно выполнить")
                    .Select(s => s.Id_status)
                    .ToList();

                var orders = db.Order
                    .Where(o => o.User_id == _currentUserId &&                      // 🔹 Только мои поручения
                                statusIds.Contains(o.Status_id))                   // 🔹 Завершённые/невыполнимые
                                 
                    .Include(o => o.Status_order)
                    .ToList();

                TotalAmountTextBlock.Text = $"Завершённых/невыполнимых моих поручений: {orders.Count}";

                foreach (var order in orders)
                    _orderQueue.Enqueue(order);
            }
        }
        private void LoadDepartments()
        {
            using (var db = new DiplomEntities1())
            {

                var departments = db.Locate_order
            .Include(l => l.Departament)
            .Select(l => l.Departament)
            .Distinct()
            .ToList();

                DepartmentComboBox.ItemsSource = departments;
            }
        }
        private void LoadStatuses()
        {
            using (var db = new DiplomEntities1())
            {
                var statuses = db.Status_order
                    .Where(s => s.Name_status == "Выполнено" || s.Name_status == "Невозможно выполнить")
                    .ToList();

                StatusComboBox.ItemsSource = statuses;
            }
        }

        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }
        private void FilterOrders()
        {
            if (DepartmentComboBox.SelectedItem is Departament selectedDepartment &&
                StatusComboBox.SelectedItem is Status_order selectedStatus)
            {
                int selectedDeptId = selectedDepartment.Id_departament;
                int selectedStatusId = selectedStatus.Id_status;

                using (var db = new DiplomEntities1())
                {
                    var filteredOrders = db.Order
                        .Where(o => o.User_id == _currentUserId &&
                                    o.Status_id == selectedStatusId &&
                                    db.Locate_order.Any(l => l.Id_locate_order == o.Address_id &&
                                                             l.Departament_id == selectedDeptId))
                        .Include(o => o.Status_order)
                        .ToList();

                    MessagesPanel.Children.Clear();
                    _orderQueue.Clear();

                    foreach (var order in filteredOrders)
                        _orderQueue.Enqueue(order);

                    TotalAmountTextBlock.Text = $"Поручений ({selectedStatus.Name_status}) в департаменте: {selectedDepartment.Name_departament}";
                    DisplayAllOrders(onlyOpenButton: true);
                }
            }
        }
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

    }
}
