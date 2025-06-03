using Diplom_F2_.AdminPanel.AddPanel;
using Diplom_F2_.BaseDate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
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
using static Diplom_F2_.MainWindow;


namespace Diplom_F2_.AdminPanel
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private int _currentUserId = SessionManager.CurrentUser.Id_user;
        public Window1()
        {
            InitializeComponent();

            LoadLicensesToPanel();
            ShowNickname();
            LoadFilters();

        }
        private void ShowNickname()
        {
            ViewNickA.Text = SessionManager.CurrentUser.Nickname;
        }
        private void LoadLicensesToPanel()
        {
            TotalAmountTextBlockA.Visibility = Visibility.Visible;
            FilterStack.Visibility = Visibility.Collapsed;


            MessagesPanelA.Children.Clear();

            int totalInactive = 0;
            int totalSoon = 0;

            var addButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

            // Кнопка "Добавить ПО"
            var addSoftButton = new Button { Content = "Добавить ПО", Margin = new Thickness(5) };
            addSoftButton.Click += (s, e) =>
            {
                string softName = PromptInputDialog("Введите название ПО:");
                if (!string.IsNullOrWhiteSpace(softName))
                {
                    using (var dbSoft = new DiplomEntities1())
                    {
                        if (!dbSoft.Soft.Any(sft => sft.Name_soft == softName))
                        {
                            dbSoft.Soft.Add(new Soft { Name_soft = softName });
                            dbSoft.SaveChanges();
                            MessageBox.Show("ПО добавлено.");
                            LoadLicensesToPanel();
                        }
                        else
                        {
                            MessageBox.Show("ПО с таким названием уже существует.");
                        }
                    }
                }
            };

            var editSoftButton = new Button { Content = "Редактировать ПО", Margin = new Thickness(5) };
            editSoftButton.Click += (s, e) =>
            {
                using (var dbSoft = new DiplomEntities1())
                {
                    var softList = dbSoft.Soft.ToList();
                    if (softList.Count == 0)
                    {
                        MessageBox.Show("Нет ПО для редактирования.");
                        return;
                    }

                    // Выбор ПО для редактирования через окно выбора (например ComboBox + кнопка)
                    var selectWindow = new Window
                    {
                        Title = "Выберите ПО для редактирования",
                        Width = 300,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize
                    };

                    var panel = new StackPanel { Margin = new Thickness(10) };

                    panel.Children.Add(new TextBlock { Text = "Выберите ПО:" });
                    var combo = new ComboBox
                    {
                        ItemsSource = softList,
                        DisplayMemberPath = "Name_soft",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 5, 0, 10)
                    };
                    panel.Children.Add(combo);

                    var editButton = new Button { Content = "Редактировать", Width = 100, Margin = new Thickness(0, 5, 0, 0) };
                    panel.Children.Add(editButton);

                    selectWindow.Content = panel;

                    editButton.Click += (sender2, e2) =>
                    {
                        var selectedSoft = combo.SelectedItem as Soft;
                        if (selectedSoft != null)
                        {
                            // Ввод нового названия с подсказкой текущего
                            string newName = PromptInputDialog("Введите новое название ПО:", selectedSoft.Name_soft);
                            if (!string.IsNullOrWhiteSpace(newName))
                            {
                                // Проверка на уникальность
                                if (!dbSoft.Soft.Any(sft => sft.Name_soft == newName && sft.Id_soft != selectedSoft.Id_soft))
                                {
                                    selectedSoft.Name_soft = newName;
                                    dbSoft.SaveChanges();
                                    MessageBox.Show("Название ПО обновлено.");
                                    selectWindow.Close();
                                    LoadLicensesToPanel();
                                }
                                else
                                {
                                    MessageBox.Show("ПО с таким названием уже существует.");
                                }
                            }
                        }
                    };

                    selectWindow.ShowDialog();
                }
            };

            var deleteSoftButton = new Button { Content = "Удалить ПО", Margin = new Thickness(5) };
            deleteSoftButton.Click += (s, e) =>
            {
                using (var dbSoft = new DiplomEntities1())
                {
                    var softList = dbSoft.Soft.ToList();
                    if (softList.Count == 0)
                    {
                        MessageBox.Show("Нет ПО для удаления.");
                        return;
                    }

                    var selectWindow = new Window
                    {
                        Title = "Выберите ПО для удаления",
                        Width = 300,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize
                    };

                    var panel = new StackPanel { Margin = new Thickness(10) };

                    panel.Children.Add(new TextBlock { Text = "Выберите ПО:" });
                    var combo = new ComboBox
                    {
                        ItemsSource = softList,
                        DisplayMemberPath = "Name_soft",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 5, 0, 10)
                    };
                    panel.Children.Add(combo);

                    var deleteButton = new Button { Content = "Удалить", Width = 100, Margin = new Thickness(0, 5, 0, 0) };
                    panel.Children.Add(deleteButton);

                    selectWindow.Content = panel;

                    deleteButton.Click += (sender2, e2) =>
                    {
                        var selectedSoft = combo.SelectedItem as Soft;
                        if (selectedSoft != null)
                        {
                            var confirm = MessageBox.Show($"Вы уверены, что хотите удалить ПО '{selectedSoft.Name_soft}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (confirm == MessageBoxResult.Yes)
                            {
                                // Удаляем ПО (можно проверить связи с лицензиями)
                                // Если есть лицензии, можно предупредить или удалить вместе
                                var licensesUsingSoft = dbSoft.Licenses.Any(l => l.Soft_id == selectedSoft.Id_soft);
                                if (licensesUsingSoft)
                                {
                                    MessageBox.Show("Нельзя удалить ПО, так как к нему привязаны лицензии.");
                                    return;
                                }

                                dbSoft.Soft.Remove(selectedSoft);
                                dbSoft.SaveChanges();
                                MessageBox.Show("ПО удалено.");
                                selectWindow.Close();
                                LoadLicensesToPanel();
                            }
                        }
                    };

                    selectWindow.ShowDialog();
                }
            };

            // Кнопка "Добавить лицензию"
            var addLicenseButton = new Button { Content = "Добавить лицензию", Margin = new Thickness(5) };
            addLicenseButton.Click += (s, e) =>
            {
                var window = new Window
                {
                    Title = "Добавить лицензию",
                    Width = 400,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                var panel = new StackPanel { Margin = new Thickness(10) };

                using (var db = new DiplomEntities1())
                {
                    var softList = db.Soft.ToList();
                    var devices = db.Device.Include(d => d.Location).ToList();
                    var statuses = db.Status_licenses.ToList();

                    panel.Children.Add(new TextBlock { Text = "ПО:" });
                    var softCombo = new ComboBox
                    {
                        ItemsSource = softList,
                        DisplayMemberPath = "Name_soft",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    panel.Children.Add(softCombo);

                    panel.Children.Add(new TextBlock { Text = "Устройство:" });
                    var deviceCombo = new ComboBox
                    {
                        ItemsSource = devices,
                        DisplayMemberPath = "Name_device",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    panel.Children.Add(deviceCombo);

                    panel.Children.Add(new TextBlock { Text = "Статус:" });
                    var statusCombo = new ComboBox
                    {
                        ItemsSource = statuses,
                        DisplayMemberPath = "Name_statusL",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    panel.Children.Add(statusCombo);

                    panel.Children.Add(new TextBlock { Text = "Ключ лицензии:" });
                    var keyBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
                    panel.Children.Add(keyBox);

                    panel.Children.Add(new TextBlock { Text = "Дата покупки:" });
                    var purchaseDateBox = new DatePicker { Margin = new Thickness(0, 0, 0, 10) };
                    panel.Children.Add(purchaseDateBox);

                    panel.Children.Add(new TextBlock { Text = "Дата окончания:" });
                    var expirationDateBox = new DatePicker { Margin = new Thickness(0, 0, 0, 10) };
                    panel.Children.Add(expirationDateBox);

                    var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                    var okButton = new Button { Content = "Добавить", Width = 80, Margin = new Thickness(5) };
                    var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
                    buttonPanel.Children.Add(okButton);
                    buttonPanel.Children.Add(cancelButton);
                    panel.Children.Add(buttonPanel);

                    okButton.Click += (sender2, e2) =>
                    {
                        var selectedSoft = softCombo.SelectedItem as Soft;
                        var selectedDevice = deviceCombo.SelectedItem as Device;
                        var selectedStatus = statusCombo.SelectedItem as Status_licenses;

                        var license = new Licenses
                        {
                            Soft_id = selectedSoft?.Id_soft ?? 0,
                            Device_id = selectedDevice?.Id_device ?? 0,
                            Status_id = selectedStatus?.Id_statusL ?? 0,
                            Licenses_key = string.IsNullOrWhiteSpace(keyBox.Text) ? null : keyBox.Text,
                            Purchase_date = purchaseDateBox.SelectedDate,
                            Expiration_date = expirationDateBox.SelectedDate
                        };

                        db.Licenses.Add(license);
                        db.SaveChanges();
                        MessageBox.Show("Лицензия добавлена.");
                        window.Close();
                        LoadLicensesToPanel();
                    };

                    cancelButton.Click += (sender2, e2) =>
                    {
                        window.Close();
                    };

                    window.Content = panel;
                    window.ShowDialog();
                }
            };


            // Добавляем кнопки на панель
            addButtonsPanel.Children.Add(addSoftButton);
            addButtonsPanel.Children.Add(editSoftButton);
            addButtonsPanel.Children.Add(deleteSoftButton);
            addButtonsPanel.Children.Add(addLicenseButton);
            MessagesPanelA.Children.Add(addButtonsPanel);


            using (var db = new DiplomEntities1())
            {
                var licenses = db.Licenses
                    .Include(l => l.Soft)
                    .Include(l => l.Device)
                    .Include(l => l.Device.Location)
                    .Include(l => l.Status_licenses)
                    .ToList();

                foreach (var license in licenses)
                {
                    // Обновляем статус лицензии в зависимости от дат
                    string newStatus = null;

                    bool isPurchaseDateNull = !license.Purchase_date.HasValue;
                    bool isExpirationDateNull = !license.Expiration_date.HasValue;
                    bool isKeyNull = string.IsNullOrEmpty(license.Licenses_key);

                    if (isKeyNull && isPurchaseDateNull && isExpirationDateNull)
                    {
                        newStatus = "Неактивна";
                    }
                    else if (isKeyNull && isExpirationDateNull)
                    {
                        newStatus = "Бесплатная";
                    }
                    else if (isExpirationDateNull)
                    {
                        newStatus = "Бессрочная";
                    }
                    else
                    {
                        var daysLeft = (license.Expiration_date.Value - DateTime.Now).TotalDays;

                        if (daysLeft < 0)
                        {
                            newStatus = "Неактивна";
                        }
                        else if (daysLeft <= 14)
                        {
                            newStatus = "Скоро заканчивается";
                        }
                        else
                        {
                            newStatus = "Активна";
                        }
                    }

                    // Обновляем статус в базе, если нужно
                    if (license.Status_licenses?.Name_statusL != newStatus)
                    {
                        var statusEntry = db.Status_licenses.FirstOrDefault(s => s.Name_statusL == newStatus);
                        if (statusEntry != null)
                        {
                            license.Status_licenses = statusEntry;
                            license.Status_id = statusEntry.Id_statusL; // если есть поле FK
                        }
                    }
                }

                db.SaveChanges();

                // Теперь грузим данные и отображаем

                licenses = licenses
                    .OrderByDescending(l => l.Status_licenses.Name_statusL == "Неактивна")
                    .ThenByDescending(l => l.Status_licenses.Name_statusL == "Скоро заканчивается")
                    .ToList();

                foreach (var license in licenses)
                {
                    var border = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10),
                        Margin = new Thickness(5),
                        CornerRadius = new CornerRadius(5)
                    };

                    var stack = new StackPanel();

                    // Название ПО
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"ПО: {license.Soft?.Name_soft}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 18
                    });

                    var licensesWithSameSoft = db.Licenses
                        .Where(l => l.Soft_id == license.Soft_id)
                        .Include(l => l.Device.Location)
                        .Include(l => l.Status_licenses)
                        .ToList();

                    int total = licensesWithSameSoft.Count;
                    int active = licensesWithSameSoft.Count(d =>
                        d.Status_licenses.Name_statusL == "Активна" ||
                        d.Status_licenses.Name_statusL == "Бессрочная" ||
                        d.Status_licenses.Name_statusL == "Бесплатная");

                    int soon = licensesWithSameSoft.Count(d => d.Status_licenses.Name_statusL == "Скоро заканчивается");
                    int inactive = licensesWithSameSoft.Count(d => d.Status_licenses.Name_statusL == "Неактивна");

                    totalInactive += inactive;
                    totalSoon += soon;

                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Устройств с этой лицензией: {total} (Активных: {active}, Скоро истекают: {soon}, Неактивных: {inactive})",
                        FontSize = 14,
                        Margin = new Thickness(0, 5, 0, 5)
                    });

                    var locations = licensesWithSameSoft
                        .Where(l => l.Device?.Location != null)
                        .Select(l => $"{l.Device.Location.Name_location}, {l.Device.Location.Address}")
                        .Distinct()
                        .ToList();

                    stack.Children.Add(new TextBlock
                    {
                        Text = $"Находятся в: {string.Join("; ", locations)}",
                        FontSize = 14,
                        Width = 500,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    });

                    // Статус лицензии с учётом "Бесплатная" и "Бесконечная"
                    Brush statusColor;
                    string statusText;

                    switch (license.Status_licenses.Name_statusL)
                    {
                        case "Неактивна":
                            statusColor = Brushes.Red;
                            statusText = "Статус: Присутствуют неактивные лицензии";
                            break;
                        case "Скоро закончиться":
                            statusColor = Brushes.Goldenrod;
                            statusText = "Статус: Есть лицензии, которые скоро закончатся";
                            break;
                        case "Бессрочная":
                            statusColor = Brushes.Blue;
                            statusText = "Статус: Лицензии все активны и бессрочны";
                            break;
                        case "Бесплатная":
                            statusColor = Brushes.DarkGreen;
                            statusText = "Статус: Бесплатная лицензия";
                            break;
                        case "Активна":
                        default:
                            statusColor = Brushes.Green;
                            statusText = "Статус: Все лицензии активны";
                            break;
                    }

                    stack.Children.Add(new TextBlock
                    {
                        Text = statusText,
                        Foreground = statusColor,
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 0)
                    });
/*
                    var openButton = new Button
                    {
                        Content = "Открыть",
                        Width = 100,
                        Margin = new Thickness(0, 10, 0, 0),
                        Tag = license.Id_licenses
                    };
                    openButton.Click += OpenLicenseDetails_Click;

                    stack.Children.Add(openButton);
*/
                    border.Child = stack;
                    MessagesPanelA.Children.Add(border);

                    var editAllButton = new Button
                    {
                        Content = "Редактировать лицензии",
                        Margin = new Thickness(5)
                    };
                    editAllButton.Click += (s, e) => OpenEditLicensesBySoft(license.Soft.Name_soft);
                    stack.Children.Add(editAllButton);

                    var deleteButton = new Button
                    {
                        Content = "Удалить",
                        Width = 100,
                        Margin = new Thickness(5, 10, 0, 0),
                        Tag = license.Id_licenses
                    };
                    deleteButton.Click += (s, e) =>
                    {
                        var result = MessageBox.Show("Вы уверены, что хотите удалить эту лицензию?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            using (var dbDelete = new DiplomEntities1())
                            {
                                var id = (int)((Button)s).Tag;
                                var licToDelete = dbDelete.Licenses.Find(id);
                                if (licToDelete != null)
                                {
                                    dbDelete.Licenses.Remove(licToDelete);
                                    dbDelete.SaveChanges();
                                }
                            }

                            LoadLicensesToPanel(); // Обновить интерфейс
                        }
                    };

                    stack.Children.Add(deleteButton);




                    // Можно создать горизонтальную панель для кнопок
                    var buttonStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 10, 0, 0)
                    };


                    stack.Children.Add(buttonStack);
                }

                if (totalInactive > 0 && totalSoon > 0)
                {
                    TotalAmountTextBlockA.Text = $"Внимание! Присутствуют {totalInactive} неактивных лицензий и {totalSoon} лицензий, которые скоро заканчиваются.";
                    TotalAmountTextBlockA.Foreground = Brushes.Red;
                }
                else if (totalInactive > 0)
                {
                    TotalAmountTextBlockA.Text = $"Внимание! Присутствуют {totalInactive} неактивных лицензий.";
                    TotalAmountTextBlockA.Foreground = Brushes.Red;
                }
                else if (totalSoon > 0)
                {
                    TotalAmountTextBlockA.Text = $"Есть {totalSoon} лицензий, которые скоро заканчиваются.";
                    TotalAmountTextBlockA.Foreground = Brushes.Goldenrod;
                }
                else
                {
                    TotalAmountTextBlockA.Text = "Все лицензии активны.";
                    TotalAmountTextBlockA.Foreground = Brushes.Green;
                }
            }
        }
        void OpenEditLicensesBySoft(string softName)
        {
            var window = new Window
            {
                Title = $"Редактирование лицензий: {softName}",
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = stackPanel
            };

            using (var db = new DiplomEntities1())
            {
                var licenses = db.Licenses
                    .Include(l => l.Soft)
                    .Include(l => l.Device)
                    .Include(l => l.Status_licenses)
                    .Where(l => l.Soft.Name_soft == softName)
                    .ToList();

                foreach (var license in licenses)
                {
                    var border = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(5),
                        Padding = new Thickness(10),
                        CornerRadius = new CornerRadius(5)
                    };

                    var itemPanel = new StackPanel();

                    itemPanel.Children.Add(new TextBlock
                    {
                        Text = $"ПО: {license.Soft.Name_soft}, Устройство: {license.Device.Name_device}, Статус: {license.Status_licenses.Name_statusL}",
                        FontWeight = FontWeights.Bold
                    });

                    itemPanel.Children.Add(new TextBlock
                    {
                        Text = $"Ключ: {license.Licenses_key}, Покупка: {license.Purchase_date?.ToShortDateString()}, Окончание: {license.Expiration_date?.ToShortDateString()}",
                        FontSize = 12
                    });

                    var editButton = new Button
                    {
                        Content = "Редактировать",
                        Width = 100,
                        Margin = new Thickness(0, 5, 0, 0)
                    };
                    editButton.Click += (s, e) => EditLicense(license.Id_licenses);

                    itemPanel.Children.Add(editButton);
                    border.Child = itemPanel;
                    stackPanel.Children.Add(border);
                }
            }

            window.Content = scrollViewer;
            window.ShowDialog();
        }

       
        void EditLicense(int licenseId)
        {
            var window = new Window
            {
                Title = "Редактировать лицензию",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(10) };

            using (var db = new DiplomEntities1())
            {
                var license = db.Licenses.Find(licenseId);
                var softList = db.Soft.ToList();
                var devices = db.Device.Include(d => d.Location).ToList();
                var statuses = db.Status_licenses.ToList();

                var softCombo = new ComboBox
                {
                    ItemsSource = softList,
                    DisplayMemberPath = "Name_soft",
                    SelectedItem = license.Soft,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var deviceCombo = new ComboBox
                {
                    ItemsSource = devices,
                    DisplayMemberPath = "Name_device",
                    SelectedItem = license.Device,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var statusCombo = new ComboBox
                {
                    ItemsSource = statuses,
                    DisplayMemberPath = "Name_statusL",
                    SelectedItem = license.Status_licenses,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var keyBox = new TextBox { Text = license.Licenses_key, Margin = new Thickness(0, 0, 0, 10) };
                var purchaseDateBox = new DatePicker { SelectedDate = license.Purchase_date, Margin = new Thickness(0, 0, 0, 10) };
                var expirationDateBox = new DatePicker { SelectedDate = license.Expiration_date, Margin = new Thickness(0, 0, 0, 10) };

                var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                var saveButton = new Button { Content = "Сохранить", Width = 80, Margin = new Thickness(5) };
                var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
                buttonPanel.Children.Add(saveButton);
                buttonPanel.Children.Add(cancelButton);

                panel.Children.Add(new TextBlock { Text = "ПО:" }); panel.Children.Add(softCombo);
                panel.Children.Add(new TextBlock { Text = "Устройство:" }); panel.Children.Add(deviceCombo);
                panel.Children.Add(new TextBlock { Text = "Статус:" }); panel.Children.Add(statusCombo);
                panel.Children.Add(new TextBlock { Text = "Ключ лицензии:" }); panel.Children.Add(keyBox);
                panel.Children.Add(new TextBlock { Text = "Дата покупки:" }); panel.Children.Add(purchaseDateBox);
                panel.Children.Add(new TextBlock { Text = "Дата окончания:" }); panel.Children.Add(expirationDateBox);
                panel.Children.Add(buttonPanel);

                saveButton.Click += (s, e) =>
                {
                    license.Soft_id = (softCombo.SelectedItem as Soft)?.Id_soft ?? 0;
                    license.Device_id = (deviceCombo.SelectedItem as Device)?.Id_device ?? 0;
                    license.Status_id = (statusCombo.SelectedItem as Status_licenses)?.Id_statusL ?? 0;
                    license.Licenses_key = keyBox.Text;
                    license.Purchase_date = purchaseDateBox.SelectedDate;
                    license.Expiration_date = expirationDateBox.SelectedDate;

                    db.SaveChanges();
                    MessageBox.Show("Лицензия обновлена.");
                    window.Close();
                    LoadLicensesToPanel();
                };

                cancelButton.Click += (s, e) => window.Close();

                window.Content = panel;
                window.ShowDialog();
            }
        }


       /* private void OpenLicenseDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is int licenseId)
            {
                using (var db = new DiplomEntities1())
                {
                    var license = db.Licenses
                        .Include(l => l.Soft)
                        .Include(l => l.Device)
                        .Include(l => l.Device.Location)
                        .Include(l => l.Status_licenses)
                        .FirstOrDefault(l => l.Id_licenses == licenseId);

                    if (license != null)
                    {
                        var window = new LicenseDetailsWindow(license);
                        window.Show();
                    }
                }
            }
        }*/

        

        private void HNotifi_Click(object sender, RoutedEventArgs e)
        {
                
        }
        private void Button_Click_HTask(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
        }
        private void Button_Click_User(object sender, RoutedEventArgs e)
        {
            UserViewPanel();
        }
        private void Button_Click_Device(object sender, RoutedEventArgs e)
        {
            LoadDevicesToPanel();
            
        }
        private void Button_Click_Licenses(object sender, RoutedEventArgs e)
        {
            LoadLicensesToPanel();
        }
        private void LoadDevicesToPanel()
        {
            TotalAmountTextBlockA.Visibility = Visibility.Visible;
            FilterStack.Visibility = Visibility.Collapsed;


            MessagesPanelA.Children.Clear();
           

            using (var db = new DiplomEntities1())
            {
                var allStatuses = db.Status_device.ToList();
                var allLocations = db.Location.ToList();
                var allProducers = db.Producer.ToList();

                var allUsers = db.User
                    .Where(u => u.Role_id == 2 || (u.Role_id == 1 && u.Departament_id == 3))
                    .ToList();

                var devices = db.Device
                    .Include(d => d.Location)
                    .Include(d => d.Status_device)
                    .Include(d => d.Producer)
                    .Include(d => d.User)
                    .ToList();

                TotalAmountTextBlockA.Text = $"Всего устройств: {devices.Count}";
                TotalAmountTextBlockA.Foreground = Brushes.Black;


                
                var addButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                var addProducerButton = new Button { Content = "Добавить производителя", Margin = new Thickness(5) };
                var addLocationButton = new Button { Content = "Добавить местоположение", Margin = new Thickness(5) };
                var addDeviceButton = new Button { Content = "Добавить устройство", Margin = new Thickness(5) };
                var addStatusButton = new Button { Content = "Добавить статус", Margin = new Thickness(5) };

                addProducerButton.Click += (s, e) =>
                {
                    string input = PromptInputDialog("Введите имя производителя:");
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        using (var dbNew = new DiplomEntities1())
                        {
                            if (!dbNew.Producer.Any(p => p.Name_producer == input))
                            {
                                var newProducer = new Producer { Name_producer = input };
                                dbNew.Producer.Add(newProducer);
                                dbNew.SaveChanges();
                                MessageBox.Show("Производитель добавлен.");
                                LoadDevicesToPanel();
                            }
                            else
                            {
                                MessageBox.Show("Производитель с таким именем уже существует.");
                            }
                        }
                    }
                };
                addStatusButton.Click += (s, e) =>
                {
                    string input = PromptInputDialog("Введите имя статуса:");
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        using (var dbNew = new DiplomEntities1())
                        {
                            if (!dbNew.Status_device.Any(st => st.Name_statusD == input))
                            {
                                var newStatus = new Status_device { Name_statusD = input };
                                dbNew.Status_device.Add(newStatus);
                                dbNew.SaveChanges();
                                MessageBox.Show("Статус добавлен.");
                                LoadDevicesToPanel();
                            }
                            else
                            {
                                MessageBox.Show("Статус с таким именем уже существует.");
                            }
                        }
                    }
                };

                

                addLocationButton.Click += (s, e) =>
                {
                    string nameInput = PromptInputDialog("Введите имя местоположения:");
                    if (!string.IsNullOrWhiteSpace(nameInput))
                    {
                        string addressInput = PromptInputDialog("Введите адрес местоположения:");
                        if (!string.IsNullOrWhiteSpace(addressInput))
                        {
                            using (var dbNew = new DiplomEntities1())
                            {
                                if (!dbNew.Location.Any(l => l.Name_location == nameInput))
                                {
                                    var newLocation = new Location
                                    {
                                        Name_location = nameInput,
                                        Address = addressInput // замените 'Address' на реальное имя поля в вашей модели
                                    };

                                    try
                                    {
                                        dbNew.Location.Add(newLocation);
                                        dbNew.SaveChanges();
                                        MessageBox.Show("Местоположение добавлено.");
                                        LoadDevicesToPanel();
                                    }
                                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                                    {
                                        var errorMessages = ex.EntityValidationErrors
                                            .SelectMany(x => x.ValidationErrors)
                                            .Select(x => x.ErrorMessage);
                                        var fullErrorMessage = string.Join("\n", errorMessages);
                                        MessageBox.Show("Ошибка при сохранении местоположения:\n" + fullErrorMessage);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Местоположение с таким именем уже существует.");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Адрес не может быть пустым.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Имя местоположения не может быть пустым.");
                    }
                };

                // Добавление обработчика для кнопки "Добавить устройство"
                addDeviceButton.Click += (s, e) =>
                {
                    var inputWindow = new Window
                    {
                        Title = "Добавить устройство",
                        Width = 400,
                        Height = 400,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize
                    };

                    var stack = new StackPanel { Margin = new Thickness(10) };

                    // Название - TextBox
                    stack.Children.Add(new TextBlock { Text = "Название устройства:" });
                    var nameTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
                    stack.Children.Add(nameTextBox); // ✅

                    stack.Children.Add(new TextBlock { Text = "Тип устройства:" });
                    var typeTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
                    stack.Children.Add(typeTextBox); // ✅

                    stack.Children.Add(new TextBlock { Text = "Статус:" });
                    var statusCombo = new ComboBox
                    {
                        ItemsSource = allStatuses,
                        DisplayMemberPath = "Name_statusD",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    stack.Children.Add(statusCombo); // ✅

                    stack.Children.Add(new TextBlock { Text = "Местоположение:" });
                    var locationCombo = new ComboBox
                    {
                        ItemsSource = allLocations,
                        DisplayMemberPath = "Name_location",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    stack.Children.Add(locationCombo); // ✅

                    stack.Children.Add(new TextBlock { Text = "Производитель:" });
                    var producerCombo = new ComboBox
                    {
                        ItemsSource = allProducers,
                        DisplayMemberPath = "Name_producer",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    stack.Children.Add(producerCombo); // ✅

                    stack.Children.Add(new TextBlock { Text = "Пользователь:" });
                    var userCombo = new ComboBox
                    {
                        ItemsSource = allUsers,
                        DisplayMemberPath = "Nickname",
                        SelectedIndex = 0,
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    stack.Children.Add(userCombo);

                    // Кнопки
                    var buttonsPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    var okButton = new Button { Content = "Добавить", Width = 80, Margin = new Thickness(5), IsDefault = true };
                    var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5), IsCancel = true };
                    buttonsPanel.Children.Add(okButton);
                    buttonsPanel.Children.Add(cancelButton);

                    stack.Children.Add(buttonsPanel);
                    inputWindow.Content = stack;

                    okButton.Click += (sender2, e2) =>
                    {
                        if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                        {
                            MessageBox.Show("Название устройства не может быть пустым.");
                            return;
                        }

                        if (statusCombo.SelectedItem == null || locationCombo.SelectedItem == null ||
                            producerCombo.SelectedItem == null || userCombo.SelectedItem == null)
                        {
                            MessageBox.Show("Пожалуйста, выберите все параметры из списков.");
                            return;
                        }

                        using (var dbNew = new DiplomEntities1())
                        {
                            var newDevice = new Device
                            {
                                Name_device = nameTextBox.Text.Trim(),
                                Type = typeTextBox.Text.Trim(),
                                Status_device = statusCombo.SelectedItem as Status_device,
                                Location = locationCombo.SelectedItem as Location,
                                Producer = producerCombo.SelectedItem as Producer,
                                User = userCombo.SelectedItem as User
                            };

                            dbNew.Device.Add(newDevice);
                            dbNew.SaveChanges();
                        }

                        MessageBox.Show("Устройство успешно добавлено.");
                        inputWindow.DialogResult = true;
                        inputWindow.Close();
                        LoadDevicesToPanel();
                    };

                    cancelButton.Click += (sender2, e2) =>
                    {
                        inputWindow.DialogResult = false;
                        inputWindow.Close();
                    };

                    inputWindow.ShowDialog();
                };
                var editProducerButton = new Button { Background = Brushes.Yellow, Content = "Редактировать производителя", Margin = new Thickness(5) };
                editProducerButton.Click += (s, e) =>
                {
                    using (var dbEdit = new DiplomEntities1())
                    {
                        var producers = dbEdit.Producer.ToList();
                        var selectedName = PromptSelectDialog("Выберите производителя для редактирования:", producers.Select(p => p.Name_producer).ToList());
                        if (selectedName != null)
                        {
                            var prodToEdit = producers.FirstOrDefault(p => p.Name_producer == selectedName);
                            string newName = PromptInputDialog($"Введите новое имя для производителя \"{selectedName}\":");
                            if (!string.IsNullOrWhiteSpace(newName))
                            {
                                if (!dbEdit.Producer.Any(p => p.Name_producer == newName))
                                {
                                    prodToEdit.Name_producer = newName;
                                    dbEdit.SaveChanges();
                                    MessageBox.Show("Имя производителя обновлено.");
                                    LoadDevicesToPanel();
                                }
                                else
                                {
                                    MessageBox.Show("Производитель с таким именем уже существует.");
                                }
                            }
                        }
                    }
                };

                // УДАЛЕНИЕ производителя
                var deleteProducerButton = new Button { Background = Brushes.LightPink, Content = "Удалить производителя", Margin = new Thickness(5) };
                deleteProducerButton.Click += (s, e) =>
                {
                    using (var dbDel = new DiplomEntities1())
                    {
                        var producers = dbDel.Producer.ToList();
                        var selectedName = PromptSelectDialog("Выберите производителя для удаления:", producers.Select(p => p.Name_producer).ToList());
                        if (selectedName != null)
                        {
                            var prodToDelete = producers.FirstOrDefault(p => p.Name_producer == selectedName);
                            if (prodToDelete != null)
                            {
                                if (MessageBox.Show($"Удалить производителя \"{selectedName}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    dbDel.Producer.Remove(prodToDelete);
                                    dbDel.SaveChanges();
                                    MessageBox.Show("Производитель удалён.");
                                    LoadDevicesToPanel();
                                }
                            }
                        }
                    }
                };

                var editStatusButton = new Button { Background = Brushes.Yellow, Content = "Редактировать статус", Margin = new Thickness(5) };
                editStatusButton.Click += (s, e) =>
                {
                    using (var dbStatus = new DiplomEntities1())
                    {
                        var statuses = dbStatus.Status_device.ToList();
                        var selectedStatusName = PromptSelectDialog("Выберите статус для редактирования:", statuses.Select(st => st.Name_statusD).ToList());
                        if (selectedStatusName != null)
                        {
                            var statusToEdit = statuses.FirstOrDefault(st => st.Name_statusD == selectedStatusName);
                            if (statusToEdit != null)
                            {
                                string newStatusName = PromptInputDialog($"Введите новое имя для статуса \"{selectedStatusName}\":");
                                if (!string.IsNullOrWhiteSpace(newStatusName))
                                {
                                    if (dbStatus.Status_device.Any(st => st.Name_statusD == newStatusName))
                                    {
                                        MessageBox.Show("Статус с таким именем уже существует.");
                                        return;
                                    }

                                    statusToEdit.Name_statusD = newStatusName;
                                    dbStatus.SaveChanges();
                                    MessageBox.Show("Статус обновлён.");
                                    LoadDevicesToPanel(); // обновляем интерфейс
                                }
                            }
                        }
                    }
                };

                var deleteStatusButton = new Button { Background = Brushes.LightPink, Content = "Удалить статус", Margin = new Thickness(5) };
                deleteStatusButton.Click += (s, e) =>
                {
                    using (var dbStatusDel = new DiplomEntities1())
                    {
                        var statuses = dbStatusDel.Status_device.ToList();
                        var selectedStatusName = PromptSelectDialog("Выберите статус для удаления:", statuses.Select(st => st.Name_statusD).ToList());
                        if (selectedStatusName != null)
                        {
                            var statusToDelete = statuses.FirstOrDefault(st => st.Name_statusD == selectedStatusName);
                            if (statusToDelete != null)
                            {
                                if (MessageBox.Show($"Удалить статус \"{selectedStatusName}\"?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    dbStatusDel.Status_device.Remove(statusToDelete);
                                    dbStatusDel.SaveChanges();
                                    MessageBox.Show("Статус удалён.");
                                    LoadDevicesToPanel(); // обновляем интерфейс
                                }
                            }
                        }
                    }
                };

                var editLocationButton = new Button { Background = Brushes.Yellow, Content = "Редактировать местоположение", Margin = new Thickness(5) };
                editLocationButton.Click += (s, e) =>
                {
                    using (var dbLocation = new DiplomEntities1())
                    {
                        var locations = dbLocation.Location.ToList();
                        var selectedLocationName = PromptSelectDialog("Выберите местоположение для редактирования:", locations.Select(l => l.Name_location).ToList());
                        if (selectedLocationName != null)
                        {
                            var locationToEdit = locations.FirstOrDefault(l => l.Name_location == selectedLocationName);
                            if (locationToEdit != null)
                            {
                                string newLocationName = PromptInputDialog($"Введите новое имя для местоположения \"{selectedLocationName}\":");
                                if (!string.IsNullOrWhiteSpace(newLocationName))
                                {
                                    if (dbLocation.Location.Any(l => l.Name_location == newLocationName && l.Id_location != locationToEdit.Id_location))
                                    {
                                        MessageBox.Show("Местоположение с таким именем уже существует.");
                                        return;
                                    }

                                    string newAddress = PromptInputDialog($"Введите новый адрес для местоположения \"{selectedLocationName}\":");
                                    if (!string.IsNullOrWhiteSpace(newAddress))
                                    {
                                        locationToEdit.Name_location = newLocationName;
                                        locationToEdit.Address = newAddress;  // обновляем адрес
                                        dbLocation.SaveChanges();
                                        MessageBox.Show("Местоположение обновлено.");
                                        LoadDevicesToPanel(); // обновляем интерфейс
                                    }
                                    else
                                    {
                                        MessageBox.Show("Адрес не может быть пустым.");
                                    }
                                }
                            }
                        }
                    }
                };

                var deleteLocationButton = new Button { Background = Brushes.LightPink, Content = "Удалить местоположение", Margin = new Thickness(5) };
                deleteLocationButton.Click += (s, e) =>
                {
                    using (var dbLocationDel = new DiplomEntities1())
                    {
                        var locations = dbLocationDel.Location.ToList();
                        var selectedLocationName = PromptSelectDialog("Выберите местоположение для удаления:", locations.Select(l => l.Name_location).ToList());
                        if (selectedLocationName != null)
                        {
                            var locationToDelete = locations.FirstOrDefault(l => l.Name_location == selectedLocationName);
                            if (locationToDelete != null)
                            {
                                if (MessageBox.Show($"Удалить местоположение \"{selectedLocationName}\"?", "Подтверждение удаления", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    dbLocationDel.Location.Remove(locationToDelete);
                                    dbLocationDel.SaveChanges();
                                    MessageBox.Show("Местоположение удалено.");
                                    LoadDevicesToPanel(); // обновляем интерфейс
                                }
                            }
                        }
                    }
                };

                var mainButtonsPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };

                // Панель для кнопок "Добавить"
                var addButtonsPanelRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                addButtonsPanelRow.Children.Add(addDeviceButton);
                addButtonsPanelRow.Children.Add(addProducerButton);
                addButtonsPanelRow.Children.Add(addLocationButton);            
                addButtonsPanelRow.Children.Add(addStatusButton);


                // Панель для кнопок "Редактировать"
                var editButtonsPanelRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                editButtonsPanelRow.Children.Add(editLocationButton);
                editButtonsPanelRow.Children.Add(editStatusButton);
                editButtonsPanelRow.Children.Add(editProducerButton);

                // Панель для кнопок "Удалить"
                var deleteButtonsPanelRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
                deleteButtonsPanelRow.Children.Add(deleteLocationButton);
                deleteButtonsPanelRow.Children.Add(deleteStatusButton);
                deleteButtonsPanelRow.Children.Add(deleteProducerButton);

                // Добавляем все три панели в основной
                mainButtonsPanel.Children.Add(addButtonsPanelRow);
                mainButtonsPanel.Children.Add(editButtonsPanelRow);
                mainButtonsPanel.Children.Add(deleteButtonsPanelRow);

                // Добавляем основной StackPanel на MessagesPanelA
                MessagesPanelA.Children.Add(mainButtonsPanel);

                // Далее остальной код вывода устройств...
                foreach (var device in devices)
                {
                    var grid = new Grid { Margin = new Thickness(5), Background = Brushes.Azure };

                    var border = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.DarkGray,
                        CornerRadius = new CornerRadius(5),
                        Margin = new Thickness(5),
                        Child = grid
                    };

                    var nameBox = new TextBox { Text = device.Name_device, Width = 200,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    var typeBox = new TextBox { Text = device.Type, Width = 200,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    var statusBox = new ComboBox
                    {
                        Width = 200,
                        ItemsSource = allStatuses,
                        DisplayMemberPath = "Name_statusD",
                        SelectedItem = device.Status_device,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        
                    };

                    var locationBox = new ComboBox
                    {
                        Width = 200,
                        ItemsSource = allLocations,
                        DisplayMemberPath = "Name_location",
                        SelectedItem = device.Location,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    var producerBox = new ComboBox
                    {
                        Width = 200,
                        ItemsSource = allProducers,
                        DisplayMemberPath = "Name_producer",
                        SelectedItem = device.Producer,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    var userBox = new ComboBox
                    {
                        Width = 200,
                        ItemsSource = allUsers,
                        DisplayMemberPath = "Nickname",
                        SelectedItem = device.User,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    var saveButton = new Button { Content = "Сохранить", Margin = new Thickness(5), Tag = device.Id_device };
                    var deleteButton = new Button { Content = "Удалить", Margin = new Thickness(5), Tag = device.Id_device };

                    saveButton.Click += (s, e) =>
                    {
                        int id = (int)((Button)s).Tag;

                        using (var dbNew = new DiplomEntities1())
                        {
                            var dev = dbNew.Device.FirstOrDefault(d => d.Id_device == id);
                            if (dev != null)
                            {
                                dev.Name_device = nameBox.Text;
                                dev.Type = typeBox.Text;

                                // Проверка на null для обязательных ComboBox
                                if (statusBox.SelectedItem is Status_device selectedStatus &&
                                    locationBox.SelectedItem is Location selectedLocation &&
                                    producerBox.SelectedItem is Producer selectedProducer)
                                {
                                    dev.Status_id = selectedStatus.Id_statusD;
                                    dev.Location_id = selectedLocation.Id_location;
                                    dev.Producer_id = selectedProducer.Id_producer;

                                    // Проверка, выбран ли пользователь
                                    if (userBox.SelectedItem is User selectedUser)
                                    {
                                        dev.User_id = selectedUser.Id_user;
                                    }
                                    else
                                    {
                                        dev.User_id = null; // если пользователь не выбран, сохраняем null
                                    }

                                    dbNew.SaveChanges();
                                    MessageBox.Show("Изменения сохранены.");
                                }
                                else
                                {
                                    MessageBox.Show("Пожалуйста, выберите статус, местоположение и производителя.");
                                    return;
                                }
                            }
                        }

                        LoadDevicesToPanel(); // Перезагрузка данных
                    };

                    deleteButton.Click += (s, e) =>
                    {
                        int id = (int)((Button)s).Tag;
                        using (var dbNew = new DiplomEntities1())
                        {
                            var dev = dbNew.Device.FirstOrDefault(d => d.Id_device == id);
                            if (dev != null)
                            {
                                if (MessageBox.Show("Удалить устройство?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    dbNew.Device.Remove(dev);
                                    dbNew.SaveChanges();
                                    LoadDevicesToPanel();
                                }
                            }
                        }
                    };

                    var panel = new StackPanel { Orientation = Orientation.Vertical };
                    panel.Children.Add(new TextBlock { Text = "Название:" }); panel.Children.Add(nameBox);
                    panel.Children.Add(new TextBlock { Text = "Тип:" }); panel.Children.Add(typeBox);
                    panel.Children.Add(new TextBlock { Text = "Статус:" }); panel.Children.Add(statusBox);
                    panel.Children.Add(new TextBlock { Text = "Местоположение:" }); panel.Children.Add(locationBox);
                    panel.Children.Add(new TextBlock { Text = "Производитель:" }); panel.Children.Add(producerBox);
                    panel.Children.Add(new TextBlock { Text = "Пользователь:" }); panel.Children.Add(userBox);

                    var buttons = new StackPanel { Orientation = Orientation.Horizontal };
                    buttons.Children.Add(saveButton);
                    buttons.Children.Add(deleteButton);
                    panel.Children.Add(buttons);

                    grid.Children.Add(panel);
                    MessagesPanelA.Children.Add(border);
                   
                }
            }
        }

        // Вспомогательный метод для ввода строки (псевдо InputBox)
        public string PromptInputDialog(string message, string defaultText = "")
        {
            var inputDialog = new Window
            {
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Title = message,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(10) };
            var textBox = new TextBox { Text = defaultText };
            panel.Children.Add(textBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            var okButton = new Button { Content = "OK", Width = 70, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "Отмена", Width = 70, Margin = new Thickness(5) };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            panel.Children.Add(buttonPanel);

            inputDialog.Content = panel;

            string result = null;

            okButton.Click += (s, e) =>
            {
                result = textBox.Text;
                inputDialog.DialogResult = true;
                inputDialog.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                inputDialog.DialogResult = false;
                inputDialog.Close();
            };

            if (inputDialog.ShowDialog() == true)
                return result;
            else
                return null;
        }


        private void Button_ClickExitA(object sender, RoutedEventArgs e)
        {
            var MainkWindow = new MainWindow();
            MainkWindow.Show();
            this.Close();
        }
        private void UserViewPanel()
        {
            TotalAmountTextBlockA.Visibility = Visibility.Visible;
            FilterStack.Visibility = Visibility.Collapsed;

            

            MessagesPanelA.Children.Clear();


            var addButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

            // --- Создаем панель для формы добавления пользователя (изначально скрыта) ---
            var addUserFormPanel = new StackPanel
            {
                Margin = new Thickness(5),
                Background = Brushes.AliceBlue,
                Visibility = Visibility.Collapsed,
                Width = 350,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Поля формы
            var nameBox2 = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var surnameBox2 = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var nicknameBox2 = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var ageBox2 = new TextBox { Margin = new Thickness(0, 2, 0, 2) };

            var roleCombo2 = new ComboBox { Margin = new Thickness(0, 2, 0, 2), DisplayMemberPath = "role1" };
            var departmentCombo2 = new ComboBox { Margin = new Thickness(0, 2, 0, 2), DisplayMemberPath = "Name_departament" };

            using (var db = new DiplomEntities1())
            {
                roleCombo2.ItemsSource = db.Role.ToList();
                departmentCombo2.ItemsSource = db.Departament.ToList();
            }

            // Добавляем элементы формы
            addUserFormPanel.Children.Add(new TextBlock { Text = "Имя:" });
            addUserFormPanel.Children.Add(nameBox2);

            addUserFormPanel.Children.Add(new TextBlock { Text = "Фамилия:" });
            addUserFormPanel.Children.Add(surnameBox2);

            addUserFormPanel.Children.Add(new TextBlock { Text = "Никнейм:" });
            addUserFormPanel.Children.Add(nicknameBox2);

            addUserFormPanel.Children.Add(new TextBlock { Text = "Возраст:" });
            addUserFormPanel.Children.Add(ageBox2);

            addUserFormPanel.Children.Add(new TextBlock { Text = "Роль:" });
            addUserFormPanel.Children.Add(roleCombo2);

            addUserFormPanel.Children.Add(new TextBlock { Text = "Департамент:" });
            addUserFormPanel.Children.Add(departmentCombo2);

            // Кнопки "Добавить" и "Отмена"
            var formButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            var addUserConfirmButton = new Button { Content = "Добавить", Margin = new Thickness(5) };
            var addUserCancelButton = new Button { Content = "Отмена", Margin = new Thickness(5) };

            formButtonsPanel.Children.Add(addUserConfirmButton);
            formButtonsPanel.Children.Add(addUserCancelButton);

            addUserFormPanel.Children.Add(formButtonsPanel);

            // --- Кнопка "Добавить пользователя" ---
            var addUserButton = new Button { Content = "Добавить пользователя", Margin = new Thickness(5) };          
            addUserButton.Click += (s, e) =>
            {
                var dialogResult = ShowAddUserDialog();
                if (dialogResult == true)
                {
                    UserViewPanel(); // обновить список
                }
            };


            // Обработка кнопки "Добавить" в форме
            addUserConfirmButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameBox2.Text)
                    || string.IsNullOrWhiteSpace(surnameBox2.Text)
                    || string.IsNullOrWhiteSpace(nicknameBox2.Text)
                    || !int.TryParse(ageBox2.Text, out int age)
                    || roleCombo2.SelectedItem == null
                    || departmentCombo2.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно.");
                    return;
                }

                using (var db = new DiplomEntities1())
                {
                    var newUser = new User
                    {
                        Name = nameBox2.Text.Trim(),
                        Surname = surnameBox2.Text.Trim(),
                        Nickname = nicknameBox2.Text.Trim(),
                        Age = age,
                        Role = (Role)roleCombo2.SelectedItem,
                        Role_id = ((Role)roleCombo2.SelectedItem).Id_role,
                        Departament = (Departament)departmentCombo2.SelectedItem,
                        Departament_id = ((Departament)departmentCombo2.SelectedItem).Id_departament
                    };
                    db.User.Add(newUser);
                    db.SaveChanges();
                }

                MessageBox.Show("Пользователь добавлен.");

                // Скрываем форму и очищаем поля
                addUserFormPanel.Visibility = Visibility.Collapsed;
                nameBox2.Text = surnameBox2.Text = nicknameBox2.Text = ageBox2.Text = "";
                roleCombo2.SelectedItem = null;
                departmentCombo2.SelectedItem = null;

                UserViewPanel(); // Обновляем список пользователей
            };

            // Обработка кнопки "Отмена" в форме
            addUserCancelButton.Click += (s, e) =>
            {
                addUserFormPanel.Visibility = Visibility.Collapsed;
                nameBox2.Text = surnameBox2.Text = nicknameBox2.Text = ageBox2.Text = "";
                roleCombo2.SelectedItem = null;
                departmentCombo2.SelectedItem = null;
            };

            // --- Кнопка "Добавить роль" ---
            var addRoleButton = new Button { Content = "Добавить роль", Margin = new Thickness(5) };
            addRoleButton.Click += (s, e) =>
            {
                string roleName = PromptInputDialog("Введите название роли:");
                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    using (var db = new DiplomEntities1())
                    {
                        if (!db.Role.Any(r => r.role1 == roleName))
                        {
                            db.Role.Add(new Role { role1 = roleName });
                            db.SaveChanges();
                            MessageBox.Show("Роль добавлена.");
                            UserViewPanel();
                        }
                        else
                        {
                            MessageBox.Show("Роль с таким названием уже существует.");
                        }
                    }
                }
            };

            var addDepartmentButton = new Button { Content = "Добавить департамент", Margin = new Thickness(5) };
            addDepartmentButton.Click += (s, e) =>
            {
                string deptName = PromptInputDialog("Введите название департамента:");
                if (!string.IsNullOrWhiteSpace(deptName))
                {
                    using (var db = new DiplomEntities1())
                    {
                        if (!db.Departament.Any(d => d.Name_departament == deptName))
                        {
                            db.Departament.Add(new Departament { Name_departament = deptName });
                            db.SaveChanges();
                            MessageBox.Show("Департамент добавлен.");
                            UserViewPanel();
                        }
                        else
                        {
                            MessageBox.Show("Департамент с таким названием уже существует.");
                        }
                    }
                }
            };

            // --- Новая кнопка "Удалить роль" ---
            var deleteRoleButton = new Button { Background = Brushes.LightPink, Content = "Удалить роль", Margin = new Thickness(5) };
            deleteRoleButton.Click += (s, e) =>
            {
                using (var db = new DiplomEntities1())
                {
                    var roles = db.Role.ToList();
                    var selectedRoleName = PromptSelectDialog("Выберите роль для удаления:", roles.Select(r => r.role1).ToList());
                    if (selectedRoleName != null)
                    {
                        var roleToDelete = roles.FirstOrDefault(r => r.role1 == selectedRoleName);
                        if (roleToDelete != null)
                        {
                            // Проверяем, что роль не используется пользователями
                            bool isUsed = db.User.Any(u => u.Role_id == roleToDelete.Id_role);
                            if (isUsed)
                            {
                                MessageBox.Show("Нельзя удалить роль, которая используется пользователями.");
                                return;
                            }
                            if (MessageBox.Show($"Удалить роль \"{roleToDelete.role1}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                db.Role.Remove(roleToDelete);
                                db.SaveChanges();
                                MessageBox.Show("Роль удалена.");
                                UserViewPanel();
                            }
                        }
                    }
                }
            };

            // --- Новая кнопка "Удалить департамент" ---
            var deleteDepartmentButton = new Button { Background = Brushes.LightPink,  Content = "Удалить департамент", Margin = new Thickness(5) };
            deleteDepartmentButton.Click += (s, e) =>
            {
                using (var db = new DiplomEntities1())
                {
                    var departments = db.Departament.ToList();
                    var selectedDeptName = PromptSelectDialog("Выберите департамент для удаления:", departments.Select(d => d.Name_departament).ToList());
                    if (selectedDeptName != null)
                    {
                        var deptToDelete = departments.FirstOrDefault(d => d.Name_departament == selectedDeptName);
                        if (deptToDelete != null)
                        {
                            // Проверяем, что департамент не используется пользователями
                            bool isUsed = db.User.Any(u => u.Departament_id == deptToDelete.Id_departament);
                            if (isUsed)
                            {
                                MessageBox.Show("Нельзя удалить департамент, который используется пользователями.");
                                return;
                            }
                            if (MessageBox.Show($"Удалить департамент \"{deptToDelete.Name_departament}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                db.Departament.Remove(deptToDelete);
                                db.SaveChanges();
                                MessageBox.Show("Департамент удалён.");
                                UserViewPanel();
                            }
                        }
                    }
                }
            };
            var editRoleButton = new Button { Background = Brushes.Yellow, Content = "Редактировать роль", Margin = new Thickness(5) };
            editRoleButton.Click += (s, e) =>
            {
                using (var db = new DiplomEntities1())
                {
                    var roles = db.Role.ToList();
                    var selectedRoleName = PromptSelectDialog("Выберите роль для редактирования:", roles.Select(r => r.role1).ToList());
                    if (selectedRoleName != null)
                    {
                        var roleToEdit = roles.FirstOrDefault(r => r.role1 == selectedRoleName);
                        if (roleToEdit != null)
                        {
                            string newRoleName = PromptInputDialog($"Введите новое название для роли \"{selectedRoleName}\":");
                            if (!string.IsNullOrWhiteSpace(newRoleName))
                            {
                                // Проверка на дубликаты
                                if (db.Role.Any(r => r.role1 == newRoleName))
                                {
                                    MessageBox.Show("Роль с таким названием уже существует.");
                                    return;
                                }

                                roleToEdit.role1 = newRoleName;
                                db.SaveChanges();
                                MessageBox.Show("Название роли обновлено.");
                                UserViewPanel(); // Обновление интерфейса
                            }
                        }
                    }
                }
            };

            var editDepartmentButton = new Button { Background = Brushes.Yellow,  Content = "Редактировать департамент", Margin = new Thickness(5) };
            editDepartmentButton.Click += (s, e) =>
            {
                using (var db = new DiplomEntities1())
                {
                    var departments = db.Departament.ToList();
                    var selectedDeptName = PromptSelectDialog("Выберите департамент для редактирования:", departments.Select(d => d.Name_departament).ToList());
                    if (selectedDeptName != null)
                    {
                        var deptToEdit = departments.FirstOrDefault(d => d.Name_departament == selectedDeptName);
                        if (deptToEdit != null)
                        {
                            string newDeptName = PromptInputDialog($"Введите новое название для департамента \"{selectedDeptName}\":");
                            if (!string.IsNullOrWhiteSpace(newDeptName))
                            {
                                // Проверка на дубликаты
                                if (db.Departament.Any(d => d.Name_departament == newDeptName))
                                {
                                    MessageBox.Show("Департамент с таким названием уже существует.");
                                    return;
                                }

                                deptToEdit.Name_departament = newDeptName;
                                db.SaveChanges();
                                MessageBox.Show("Название департамента обновлено.");
                                UserViewPanel(); // Обновление интерфейса
                            }
                        }
                    }
                }
            };


            var addButtonsContainer = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };

            // Первая строка кнопок
            var topButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            topButtonsPanel.Children.Add(addUserButton);
            topButtonsPanel.Children.Add(addRoleButton);
            topButtonsPanel.Children.Add(addDepartmentButton);
            topButtonsPanel.Children.Add(deleteRoleButton);
            topButtonsPanel.Children.Add(deleteDepartmentButton);

            // Вторая строка кнопок (редактирование)
            var bottomButtonsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            bottomButtonsPanel.Children.Add(editRoleButton);
            bottomButtonsPanel.Children.Add(editDepartmentButton);

            // Добавляем в контейнер
            addButtonsContainer.Children.Add(topButtonsPanel);
            addButtonsContainer.Children.Add(bottomButtonsPanel);

            // Добавляем в UI
            MessagesPanelA.Children.Add(addButtonsContainer);

            using (var db = new DiplomEntities1())
            {
                var roles = db.Role.ToList();
                var departments = db.Departament.ToList();

                var users = db.User
                    .Include(u => u.Role)
                    .Include(u => u.Departament)
                    .ToList();

                TotalAmountTextBlockA.Text = $"Всего пользователей: {users.Count}";
                TotalAmountTextBlockA.Foreground = Brushes.Black;

                foreach (var user in users)
                {
                    var userPanel = new StackPanel
                    {
                        Margin = new Thickness(5)
                    };

                    // Оборачиваем в Border
                    var userBorder = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.DarkGray,
                        CornerRadius = new CornerRadius(5),
                        Margin = new Thickness(5),
                        Child = userPanel,
                        
                    };

                    // Поля в TextBox
                    var nameBox = new TextBox { Text = user.Name, Margin = new Thickness(0, 2, 0, 2) };
                    var surnameBox = new TextBox { Text = user.Surname, Margin = new Thickness(0, 2, 0, 2) };
                    var nicknameBox = new TextBox { Text = user.Nickname, Margin = new Thickness(0, 2, 0, 2) };
                    var ageBox = new TextBox { Text = user.Age.ToString(), Margin = new Thickness(0, 2, 0, 2) };

                    // Роль в ComboBox
                    var roleCombo = new ComboBox
                    {
                        ItemsSource = roles,
                        DisplayMemberPath = "role1",
                        SelectedItem = user.Role,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    // Департамент в ComboBox
                    var departmentCombo = new ComboBox
                    {
                        ItemsSource = departments,
                        DisplayMemberPath = "Name_departament",
                        SelectedItem = user.Departament,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    // Добавляем в StackPanel
                    userPanel.Children.Add(new TextBlock { Text = "Имя:" });
                    userPanel.Children.Add(nameBox);

                    userPanel.Children.Add(new TextBlock { Text = "Фамилия:" });
                    userPanel.Children.Add(surnameBox);

                    userPanel.Children.Add(new TextBlock { Text = "Никнейм:" });
                    userPanel.Children.Add(nicknameBox);

                    userPanel.Children.Add(new TextBlock { Text = "Возраст:" });
                    userPanel.Children.Add(ageBox);

                    userPanel.Children.Add(new TextBlock { Text = "Роль:" });
                    userPanel.Children.Add(roleCombo);

                    userPanel.Children.Add(new TextBlock { Text = "Департамент:" });
                    userPanel.Children.Add(departmentCombo);

                    // Кнопки
                    var buttonsPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    var saveButton = new Button
                    {
                        Content = "Сохранить",
                        Margin = new Thickness(5),
                        Tag = user.Id_user
                    };

                    var deleteButton = new Button
                    {
                        Content = "Удалить",
                        Margin = new Thickness(5),
                        Tag = user.Id_user
                    };

                    // Обработчик кнопки "Сохранить"
                    saveButton.Click += (s, e) =>
                    {
                        int userId = (int)((Button)s).Tag;
                        using (var dbEdit = new DiplomEntities1())
                        {
                            var u = dbEdit.User.FirstOrDefault(x => x.Id_user == userId);
                            if (u != null)
                            {
                                u.Name = nameBox.Text.Trim();
                                u.Surname = surnameBox.Text.Trim();
                                u.Nickname = nicknameBox.Text.Trim();
                                u.Age = int.TryParse(ageBox.Text.Trim(), out int age) ? age : u.Age;

                                u.Role_id = ((Role)roleCombo.SelectedItem).Id_role;
                                u.Departament_id = ((Departament)departmentCombo.SelectedItem).Id_departament;

                                dbEdit.SaveChanges();
                                MessageBox.Show("Пользователь обновлён.");
                                UserViewPanel(); // Обновить список
                            }
                        }
                    };

                    // Обработчик кнопки "Удалить"
                    deleteButton.Click += (s, e) =>
                    {
                        int userId = (int)((Button)s).Tag;
                        if (MessageBox.Show("Удалить пользователя и открепить все его устройства?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            using (var dbDel = new DiplomEntities1())
                            {
                                var user1 = dbDel.User.FirstOrDefault(x => x.Id_user == userId);
                                if (user1 != null)
                                {
                                    // Найти все устройства, привязанные к пользователю
                                    var devices = dbDel.Device.Where(d => d.User_id == userId).ToList();

                                    // Отвязать устройства
                                    foreach (var device in devices)
                                    {
                                        device.User_id = null;
                                    }

                                    dbDel.User.Remove(user1); // Удалить пользователя
                                    dbDel.SaveChanges();

                                    MessageBox.Show("Пользователь удалён, устройства отвязаны.");
                                    UserViewPanel(); // Обновить интерфейс
                                }
                            }
                        }
                    };

                    buttonsPanel.Children.Add(saveButton);
                    buttonsPanel.Children.Add(deleteButton);
                    userPanel.Children.Add(buttonsPanel);

                    // Добавить в основную панель
                    MessagesPanelA.Children.Add(userBorder);
                }
            }
        }
        private bool? ShowAddUserDialog()
        {
            var addUserWindow = new Window
            {
                Title = "Добавить пользователя",
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = this // чтобы окно было модальным к текущему
            };

            var mainPanel = new StackPanel { Margin = new Thickness(10) };

            var nameBox = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var surnameBox = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var nicknameBox = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var ageBox = new TextBox { Margin = new Thickness(0, 2, 0, 2) };
            var passwordBox = new PasswordBox { Margin = new Thickness(0, 2, 0, 2) };

            var roleCombo = new ComboBox { Margin = new Thickness(0, 2, 0, 2), DisplayMemberPath = "role1" };
            var departmentCombo = new ComboBox { Margin = new Thickness(0, 2, 0, 2), DisplayMemberPath = "Name_departament" };

            using (var db = new DiplomEntities1())
            {
                roleCombo.ItemsSource = db.Role.ToList();
                departmentCombo.ItemsSource = db.Departament.ToList();
            }

            mainPanel.Children.Add(new TextBlock { Text = "Имя:" });
            mainPanel.Children.Add(nameBox);

            mainPanel.Children.Add(new TextBlock { Text = "Фамилия:" });
            mainPanel.Children.Add(surnameBox);

            mainPanel.Children.Add(new TextBlock { Text = "Никнейм:" });
            mainPanel.Children.Add(nicknameBox);

            mainPanel.Children.Add(new TextBlock { Text = "Возраст:" });
            mainPanel.Children.Add(ageBox);

            mainPanel.Children.Add(new TextBlock { Text = "Пароль:" });
            mainPanel.Children.Add(passwordBox);

            mainPanel.Children.Add(new TextBlock { Text = "Роль:" });
            mainPanel.Children.Add(roleCombo);

            mainPanel.Children.Add(new TextBlock { Text = "Департамент:" });
            mainPanel.Children.Add(departmentCombo);

            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };

            var addButton = new Button { Content = "Добавить", Width = 80, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };

            buttonsPanel.Children.Add(addButton);
            buttonsPanel.Children.Add(cancelButton);

            mainPanel.Children.Add(buttonsPanel);

            addUserWindow.Content = mainPanel;

            // Обработка кнопок
            addButton.Click += (s, e) =>
            {
                var password = passwordBox.Password;

                // Проверяем заполненность всех полей
                if (string.IsNullOrWhiteSpace(nameBox.Text) ||
                    string.IsNullOrWhiteSpace(surnameBox.Text) ||
                    string.IsNullOrWhiteSpace(nicknameBox.Text) ||
                    !int.TryParse(ageBox.Text, out int age) ||
                    roleCombo.SelectedItem == null ||
                    departmentCombo.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля корректно, включая пароль.");
                    return;
                }

                // Проверяем возраст
                if (age <= 16 || age >= 100)
                {
                    MessageBox.Show("Возраст должен быть больше 16 и меньше 100.");
                    return;
                }

                using (var db = new DiplomEntities1())
                {
                    string newNickname = nicknameBox.Text.Trim();

                    // Проверяем уникальность никнейма
                    bool nicknameExists = db.User.Any(u => u.Nickname == newNickname);
                    if (nicknameExists)
                    {
                        MessageBox.Show("Пользователь с таким никнеймом уже существует. Пожалуйста, выберите другой никнейм.");
                        return;
                    }

                    var hashedPassword = PasswordHelper.ComputeSha256Hash(password);

                    var newUser = new User
                    {
                        Name = nameBox.Text.Trim(),
                        Surname = surnameBox.Text.Trim(),
                        Nickname = newNickname,
                        Age = age,
                        Role_id = ((Role)roleCombo.SelectedItem).Id_role,
                        Departament_id = ((Departament)departmentCombo.SelectedItem).Id_departament,
                        Password = hashedPassword
                    };

                    try
                    {
                        db.User.Add(newUser);
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        var errors = ex.EntityValidationErrors
                            .SelectMany(eve => eve.ValidationErrors)
                            .Select(ve => $"{ve.PropertyName}: {ve.ErrorMessage}");

                        string errorMsg = "Ошибка валидации:\n" + string.Join("\n", errors);

                        MessageBox.Show(errorMsg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("Пользователь добавлен.");
                addUserWindow.DialogResult = true;
                addUserWindow.Close();               
            };
            return addUserWindow.ShowDialog();
        }
        private string PromptSelectDialog(string title, List<string> options)
        {
            var window = new Window
            {
                Title = title,
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize
            };

            var listBox = new ListBox { ItemsSource = options, Margin = new Thickness(10) };
            var okButton = new Button { Content = "OK", IsEnabled = false, Margin = new Thickness(10), Width = 60 };
            var cancelButton = new Button { Content = "Отмена", Margin = new Thickness(10), Width = 60 };

            okButton.Click += (s, e) => window.DialogResult = true;
            cancelButton.Click += (s, e) => window.DialogResult = false;

            listBox.SelectionChanged += (s, e) => okButton.IsEnabled = listBox.SelectedItem != null;

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            buttonsPanel.Children.Add(okButton);
            buttonsPanel.Children.Add(cancelButton);

            var mainPanel = new StackPanel();
            mainPanel.Children.Add(listBox);
            mainPanel.Children.Add(buttonsPanel);

            window.Content = mainPanel;

            bool? result = window.ShowDialog();
            if (result == true)
                return listBox.SelectedItem as string;
            return null;
        }
        private void LoadAssignments()
        {
            TotalAmountTextBlockA.Visibility = Visibility.Collapsed;
            FilterStack.Visibility = Visibility.Visible;
                    

            MessagesPanelA.Children.Clear();

            using (var db = new DiplomEntities1())
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var completedCount = db.Order
                    .Include(o => o.Status_order)
                    .Where(o => o.Status_order.Name_status.ToLower() == "выполнено"
                             && o.Order_date.Month == currentMonth
                             && o.Order_date.Year == currentYear)
                    .Count();

                TotalAmountTextBlockA.Text = $"Выполнено поручений за месяц: {completedCount}";
                TotalAmountTextBlockA.Foreground = Brushes.Black;

                var query = db.Order
                    .Include(o => o.Locate_order.Departament)
                    .Include(o => o.Status_order)
                    .Include(o => o.User)
                    .AsQueryable();

                // 🔍 Фильтр по статусу
                if (StatusComboBox.SelectedItem != null && StatusComboBox.SelectedItem.ToString() != "Все")
                {
                    var selectedStatus = StatusComboBox.SelectedItem.ToString().ToLower();
                    query = query.Where(o => o.Status_order.Name_status.ToLower() == selectedStatus);
                }

                // 📍 Фильтр по адресу
                if (AddressComboBox.SelectedItem != null && AddressComboBox.SelectedItem.ToString() != "Все")
                {
                    var selectedAddress = AddressComboBox.SelectedItem.ToString();
                    query = query.Where(o => o.Locate_order.AddressO == selectedAddress);
                }

                // 🏢 Фильтр по департаменту
                if (DepartmentComboBox.SelectedItem != null && DepartmentComboBox.SelectedItem.ToString() != "Все")
                {
                    var selectedDepartment = DepartmentComboBox.SelectedItem.ToString();
                    query = query.Where(o => o.Locate_order.Departament.Name_departament == selectedDepartment);
                }

                // 👤 Поиск по имени, фамилии, нику
                var searchText = SearchTextBox.Text?.ToLower();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(o => o.User != null &&
                        (o.User.Name.ToLower().Contains(searchText) ||
                         o.User.Surname.ToLower().Contains(searchText) ||
                         o.User.Nickname.ToLower().Contains(searchText)));
                }

                var orders = query.ToList();

                foreach (var order in orders)
                {
                    var grid = new Grid
                    {
                        Margin = new Thickness(5),
                        Background = Brushes.Azure,
                    };

                    var TaskBorder = new Border
                    {
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.DarkGray,
                        CornerRadius = new CornerRadius(5),
                        Margin = new Thickness(5),
                        Child = grid,
                    };

                    var stack = new StackPanel();

                    stack.Children.Add(new TextBlock { Text = $"Заголовок: {order.Title}", FontWeight = FontWeights.Bold, FontSize = 14 });
                    stack.Children.Add(new TextBlock { Text = $"Описание: {order.Message}" });
                    stack.Children.Add(new TextBlock { Text = $"Дата: {order.Order_date:dd.MM.yyyy}" });
                    stack.Children.Add(new TextBlock { Text = $"Номер: {order.Number}" });
                    stack.Children.Add(new TextBlock { Text = $"Кабинет: {order.Cabinet}" });

                    if (order.Locate_order?.Departament != null)
                    {
                        stack.Children.Add(new TextBlock { Text = $"Департамент: {order.Locate_order.Departament.Name_departament}" });
                    }

                    if (order.User != null)
                    {
                        stack.Children.Add(new TextBlock { Text = $"Пользователь: {order.User.Nickname}" });
                    }

                    if (order.Status_order != null)
                    {
                        var statusText = order.Status_order.Name_status;
                        var statusColor = Brushes.Black;

                        switch (statusText.ToLower())
                        {
                            case "выполнено": statusColor = Brushes.Green; break;
                            case "невозможно выполнить": statusColor = Brushes.Red; break;
                            case "в процессе": statusColor = Brushes.Goldenrod; break;
                        }

                        stack.Children.Add(new TextBlock
                        {
                            Text = $"Статус: {statusText}",
                            Foreground = statusColor,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    var detailsButton = new Button
                    {
                        Content = "Подробнее",
                        Margin = new Thickness(5),
                        Tag = order // сохраняем ссылку на заказ
                    };
                    detailsButton.Click += ShowOrderDetails_Click;

                    stack.Children.Add(detailsButton);

                    grid.Children.Add(stack);
                    MessagesPanelA.Children.Add(TaskBorder);
                }
            }
        }

        private void ShowOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is Order order)
            {
                string reasonText = order.Reason ?? "Нет причины"; // Предполагается, что у Order есть поле Reason

                string message = $"Заголовок: {order.Title}\n" +
                                 $"Описание: {order.Message}\n" +
                                 $"Дата: {order.Order_date:dd.MM.yyyy}\n" +
                                 $"Номер: {order.Number}\n" +
                                 $"Кабинет: {order.Cabinet}\n" +
                                 $"Департамент: {order.Locate_order?.Departament?.Name_departament}\n" +
                                 $"Пользователь: {order.User?.Nickname}\n" +
                                 $"Статус: {order.Status_order?.Name_status}\n";

                // Используем WinForms MessageBox
                System.Windows.MessageBox.Show(message, "Детали поручения", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadFilters()
        {
            using (var db = new DiplomEntities1())
            {
                var statuses = db.Status_order.Select(s => s.Name_status).Distinct().ToList();
                statuses.Insert(0, "Все"); // Добавим вариант "Все"
                StatusComboBox.ItemsSource = statuses;

                var addresses = db.Locate_order.Select(l => l.AddressO).Distinct().ToList();
                addresses.Insert(0, "Все");
                AddressComboBox.ItemsSource = addresses;

                var departments = db.Departament.Select(d => d.Name_departament).Distinct().ToList();
                departments.Insert(0, "Все");
                DepartmentComboBox.ItemsSource = departments;
            }

            // Устанавливаем выбранный элемент по умолчанию
            StatusComboBox.SelectedIndex = 0;
            AddressComboBox.SelectedIndex = 0;
            DepartmentComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadAssignments();
        }
        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void ProducerButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void LocateButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void RoleButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void DepartamentButton_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }

}
