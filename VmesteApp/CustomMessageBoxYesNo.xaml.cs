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

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для CustomMessageBoxYesNo.xaml
    /// </summary>
    public partial class CustomMessageBoxYesNo : Window
    {
        public bool Result { get; set; } = false;

        public CustomMessageBoxYesNo(string message, string title)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtTitle.Text = title;
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        public static bool Show(string title, string message)
        {
            var msg = new CustomMessageBoxYesNo(title, message);

            // Находим активное окно приложения и назначаем его владельцем
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                msg.Owner = Application.Current.MainWindow;
            }

            msg.ShowDialog(); // ShowDialog делает окно модальным (поверх всех)
            return msg.Result;
        }
    }
}
