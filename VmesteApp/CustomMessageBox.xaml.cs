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
    /// Логика взаимодействия для CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string message, string title)
        {
            InitializeComponent();
            txtMessage.Text = message;
            txtTitle.Text = title;
        }

        // Статический метод для вызова
        public static void Show(string message, string title = "VmesteApp")
        {
            // 1. Находим главное окно приложения
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // 2. Включаем затемнение
            if (mainWindow != null)
            {
                mainWindow.Overlay.Visibility = Visibility.Visible;
            }

            // 3. Показываем наше кастомное окно
            var msg = new CustomMessageBox(message, title);
            msg.Owner = mainWindow; // Привязываем к главному окну
            msg.ShowDialog();

            // 4. Выключаем затемнение после закрытия
            if (mainWindow != null)
            {
                mainWindow.Overlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
