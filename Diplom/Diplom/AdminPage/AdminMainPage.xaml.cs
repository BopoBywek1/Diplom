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
using static Diplom.UserPage.DataService;


namespace Diplom.AdminPage
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private string _nickname;
        public Window1(string nickname)
        {
            InitializeComponent();
            _nickname = nickname;
            DataContext = new MainViewModel(_nickname);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var FirstWindow = new Diplom.Avtorization();
            FirstWindow.Show();
            this.Close();
        }
    }
}
