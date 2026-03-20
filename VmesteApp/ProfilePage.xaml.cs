using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VmesteApp
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        private void ChangeAvatarClick(object sender, RoutedEventArgs e)
        {
            // Создаем диалоговое окно выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Настраиваем фильтр (только картинки)
            openFileDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                // Получаем путь к выбранному файлу
                string selectedFilePath = openFileDialog.FileName;

                // Создаем новое изображение
                BitmapImage bitmap = new BitmapImage(new System.Uri(selectedFilePath));

                // Находим ImageBrush внутри фона Border и меняем его источник
                // (Для этого мы дали Border имя x:Name="AvatarBorder")
                if (AvatarBorder.Background is ImageBrush imageBrush)
                {
                    imageBrush.ImageSource = bitmap;
                }
            }
        }
    }
}
