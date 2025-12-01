using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ShoeStoreApp.Helpers;
using ShoeStoreApp.Models;

namespace ShoeStoreApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            if (!DatabaseHelper.TestConnection())
            {
                MessageBox.Show("Не удалось подключиться к базе данных.\nПроверьте настройки подключения.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginUser();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginUser();
            }
        }

        private void LoginUser()
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = DatabaseHelper.GetContext())
                {
                    var user = context.Users
                        .Include("UserRole")
                        .FirstOrDefault(u => u.UserLogin == login && u.UserPassword == password);

                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль",
                            "Ошибка авторизации",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        txtPassword.Clear();
                        return;
                    }

                    UserSession.CurrentUser = user;

                    MessageBox.Show($"Добро пожаловать, {UserSession.CurrentUser.FullName}!",
                        "Успешный вход",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    OpenMainWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            UserSession.CurrentUser = null;

            MessageBox.Show("Вы вошли как гость.\nДоступ к функциям ограничен.",
                "Гостевой вход",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            OpenMainWindow();
        }

        private void OpenMainWindow()
        {
            Views.MainWindow mainWindow = new Views.MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}