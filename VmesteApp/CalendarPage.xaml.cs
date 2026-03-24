using System;
using System.Windows;
using System.Windows.Controls;
using VmesteApp.DB.Models;
using VmesteApp.DB.Repository;

namespace VmesteApp
{
    public partial class CalendarPage : Page
    {
        private readonly EventRepository _eventRepository = new EventRepository();

        public CalendarPage()
        {
            InitializeComponent();
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Валидация обязательных полей
                if (string.IsNullOrWhiteSpace(TitleBox.Text))
                {
                    MessageBox.Show("Введите название события.");
                    return;
                }

                if (EventDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату.");
                    return;
                }

                // 2. Сбор данных из UI
                // Предполагаем, что данные текущего пользователя доступны в App.CurrentUser
                var currentUser = App.CurrentUser;

                if (currentUser == null || currentUser.familyId == null)
                {
                    MessageBox.Show("Ошибка: пользователь не авторизован или не состоит в семье.");
                    return;
                }

                // Парсинг времени
                if (!DateTime.TryParse(TimeBox.Text, out DateTime parsedTime))
                {
                    MessageBox.Show("Некорректный формат времени. Используйте ЧЧ:ММ.");
                    return;
                }

                // 3. Создание объекта события
                var newEvent = new Events
                {
                    name = TitleBox.Text,
                    description = DescriptionBox.Text,
                    category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    eventDate = EventDatePicker.SelectedDate.Value,
                    eventTime = new DateTime(1, 1, 1, parsedTime.Hour, parsedTime.Minute, 0),
                    isPrivate = IsPrivateCheck.IsChecked ?? false,
                    userId = currentUser.userId,
                    familyId = currentUser.familyId.Value
                };

                // 4. Сохранение в БД
                _eventRepository.AddEvent(newEvent);

                MessageBox.Show("Событие успешно добавлено!");

                // Очистка полей после сохранения
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            TitleBox.Clear();
            DescriptionBox.Clear();
            CategoryBox.SelectedIndex = -1;
            EventDatePicker.SelectedDate = null;
            TimeBox.Text = "12:00";
            IsPrivateCheck.IsChecked = false;
        }
    }
}