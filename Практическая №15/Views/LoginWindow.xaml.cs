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

namespace Практическая__15.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void ManagerLogin_Click(object sender, RoutedEventArgs e)
        {
            string pinCode = PinCodeBox.Password;

            if (pinCode == "1234")
            {
                var mainWindow = new MainWindow("Manager");
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный пин-код!", "Ошибка входа",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                PinCodeBox.Clear();
            }
        }

        private void GuestLogin_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow("Guest");
            mainWindow.Show();
            this.Close();
        }
    }
}
