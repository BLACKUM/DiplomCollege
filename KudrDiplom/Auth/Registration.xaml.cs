using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace KudrDiplom.Auth
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        DataBase database = new DataBase();
        public Registration()
        {
            InitializeComponent();
        }
        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private async void Reg_Click(object sender, RoutedEventArgs e)
        {
            string login = loginBox.Text;
            string password = passBox.Password;
            if (login.Length < 5 || password.Length < 5 || login.Length > 20 || password.Length > 20)
            {
                Text.Content = login.Length >= 20 || password.Length >= 20
                    ? "Логин и пароль должны содержать не более 20 символов."
                    : "Логин и пароль должны содержать не менее 5 символов.";
                return;
            }
            DataBase db = new DataBase();
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await db.openConnectionAsync();
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM Регистрация WHERE Логин = @Логин", db.getConnection());
                command.Parameters.AddWithValue("@Логин", login);
                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                if (count == 0)
                {
                    command = new SqlCommand("INSERT INTO Регистрация (Логин, Пароль) VALUES (@Логин, @Пароль)", db.getConnection());
                    command.Parameters.AddWithValue("@Логин", login);
                    command.Parameters.AddWithValue("@Пароль", password);
                    await command.ExecuteNonQueryAsync();
                    var authorization = new Authorization();
                    authorization.Show();
                    this.Close();
                    MessageBox.Show("Регистрация прошла успешно.");
                }
                else
                {
                    Text.Content = "Пользователь с таким логином уже существует.";
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                if (db.getConnection().State == ConnectionState.Open)
                {
                    db.closeConnection();
                }
            }
        }
        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var authorization = new Authorization();
            authorization.Show();
            this.Close();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
    }
}
