using System;
using System.Linq;
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

            MainCalendar.SelectedDatesChanged += (s, e) =>
            {
                EventDatePicker.SelectedDate = MainCalendar.SelectedDate;
            };
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Валидация обязательных полей
                if (string.IsNullOrWhiteSpace(TitleBox.Text))
                {
                    CustomMessageBox.Show("Введите название события.", "Ошибка");
                    return;
                }

                if (EventDatePicker.SelectedDate == null)
                {
                    CustomMessageBox.Show("Выберите дату.", "Ошибка");
                    return;
                }

                // 2. Сбор данных из UI
                var currentUser = App.CurrentUser;

                if (currentUser == null || currentUser.familyId == null)
                {
                    CustomMessageBox.Show("Пользователь не авторизован или не состоит в семье.", "Ошибка");
                    return;
                }

                // Парсинг времени
                if (!DateTime.TryParse(TimeBox.Text, out DateTime parsedTime))
                {
                    CustomMessageBox.Show("Некорректный формат времени. Используйте ЧЧ:ММ.", "Ошибка");
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

                CustomMessageBox.Show("Событие успешно добавлено!", "Успех");

                // Очистка полей после сохранения
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Ошибка при сохранении");
            }
        }

        private void TimeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // Удаляем все, кроме цифр
            string digitsOnly = new string(textBox.Text.Where(char.IsDigit).ToArray());

            string result = "";

            // Ограничиваем длину (макс 4 цифры)
            if (digitsOnly.Length > 4)
                digitsOnly = digitsOnly.Substring(0, 4);

            if (digitsOnly.Length > 0)
            {
                // Проверка первой цифры часа (не больше 2)
                int firstDigit = int.Parse(digitsOnly[0].ToString());
                if (firstDigit > 2) digitsOnly = "2" + digitsOnly.Substring(1);

                result = digitsOnly;

                // Если введено больше 2 цифр, вставляем двоеточие
                if (digitsOnly.Length > 2)
                {
                    // Проверка минут (первая цифра минут не больше 5)
                    if (int.Parse(digitsOnly[2].ToString()) > 5)
                    {
                        digitsOnly = digitsOnly.Substring(0, 2) + "5" + (digitsOnly.Length > 3 ? digitsOnly[3].ToString() : "");
                    }

                    result = digitsOnly.Insert(2, ":");
                }

                // Валидация часа (не больше 23)
                if (digitsOnly.Length >= 2)
                {
                    int hours = int.Parse(digitsOnly.Substring(0, 2));
                    if (hours > 23) result = "23" + (digitsOnly.Length > 2 ? ":" + digitsOnly.Substring(2) : "");
                }
            }

            // Чтобы избежать бесконечного цикла вызова TextChanged
            if (textBox.Text != result)
            {
                textBox.Text = result;
                textBox.SelectionStart = textBox.Text.Length; // Курсор в конец
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