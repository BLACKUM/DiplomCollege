using KudrDiplom.Menus;
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
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        DataBase database = new DataBase();
        public Authorization()
        {
            InitializeComponent();
            Application.Current.MainWindow.ForceCursor = true;
            Cursor = Cursors.AppStarting;
            Loaded += (s, e) =>
            {
                Application.Current.MainWindow.ForceCursor = false;
                Cursor = Cursors.Arrow;
            };
        }
        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            var LoginUser = loginBox.Text;
            var PasswordUser = passBox.Password;
            if (LoginUser.Length < 5 || PasswordUser.Length < 5)
            {
                string text = "Логин и пароль должны содержать не менее 5 символов.";
                Text.Content = text;
                return;
            }
            DataBase db = new DataBase();
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                db.openConnection();
                var dbContext = Entities.GetContext();
                string query = "SELECT COUNT(*) FROM Регистрация WHERE Логин = @login";
                SqlCommand command = new SqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@login", LoginUser);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count == 0)
                {
                    string text = "Пользователь с таким логином не найден.";
                    Text.Content = text;
                    db.closeConnection();
                    return;
                }
                query = "SELECT ID_Пользователя FROM Регистрация WHERE Логин = @login AND Пароль = @password";
                command = new SqlCommand(query, db.getConnection());
                command.Parameters.AddWithValue("@login", LoginUser);
                command.Parameters.AddWithValue("@password", PasswordUser);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        int userId = reader.GetInt32(reader.GetOrdinal("ID_Пользователя"));
                        var доступныеКурсыIds = dbContext.Пользователи_Курсы
                                    .Where(pc => pc.ID_Пользователя == userId)
                                    .Select(pc => pc.ID_Курса)
                                    .ToList();
                        if (доступныеКурсыIds.Count < 1)
                        {
                            loginBox.Text = "";
                            passBox.Password = "";
                            MessageBox.Show("У вас нет привилегий для доступа к какому-либо курсу, обратитесь к администратору");
                            db.closeConnection();
                            return;
                        }
                        if (доступныеКурсыIds.Count >= 6)
                        {
                            AdminMenu AdminMenu = new AdminMenu(LoginUser, userId);
                            this.Hide();
                            AdminMenu.ShowDialog();
                            return;
                        }
                        PrepodMenu PrepodPanel = new PrepodMenu(LoginUser, userId);
                        this.Hide();
                        PrepodPanel.ShowDialog();
                    }
                    else
                    {
                        string text = "Не верный логин или пароль.";
                        Text.Content = text;
                    }
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
        private void Reg_Click(object sender, RoutedEventArgs e)
        {
            var registration = new Registration();
            registration.Show();
            this.Hide();
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
