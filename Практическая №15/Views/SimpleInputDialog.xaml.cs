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
    /// Логика взаимодействия для SimpleInputDialog.xaml
    /// </summary>
    public partial class SimpleInputDialog : Window
    {
        public string InputValue { get; private set; }

        public SimpleInputDialog(string prompt, string title, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            txtPrompt.Text = prompt;
            txtInput.Text = defaultValue;
            txtInput.Focus();
            txtInput.SelectAll();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            InputValue = txtInput.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
