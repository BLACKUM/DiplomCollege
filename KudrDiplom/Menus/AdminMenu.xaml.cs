using KudrDiplom.Auth;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Data.Entity;

namespace KudrDiplom.Menus
{
    /// <summary>
    /// Логика взаимодействия для AdminMenu.xaml
    /// </summary>
    public partial class AdminMenu : Window
    {
        Entities dbEnt;
        DataBase db = new DataBase();
        private int currentUserId;
        public AdminMenu(string loginUser, int userId)
        {
            InitializeComponent();
            LoginInfo.Content = $"User:{loginUser},ID:{userId}";
            dbEnt = new Entities();
            this.currentUserId = userId;
        }
        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;
            Separator.Width = 0;
            Storyboard sb = (Storyboard)FindResource("SeparatorAnimation");
            sb.Begin();
            await ReloadTables();
        }
        private void Drag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void CollapseClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        private void ExitAccClick(object sender, RoutedEventArgs e)
        {
            Authorization auth = new Authorization();
            auth.Show();
            this.Close();
        }
        private async Task ReloadTables()
        {
            await LoadDataRaspisanie();
            await LoadDataKursi();
            await LoadDataUch();
            await LoadDataItog();
            await LoadDataPosesh();
            await LoadDataDost();
            await LoadDataPolzov();
            await LoadDataUchKursi();
        }
        private async void UpdateClick(object sender, RoutedEventArgs e)
        {
            await ReloadTables();
            MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async void DeleteSelectedRows<T>(IEnumerable<T> selectedRows, DbSet<T> dbSet, Func<Task> updateMethod) where T : class
        {
            if (!selectedRows.Any())
            {
                MessageBox.Show("Чтобы удалить - выберите строку!");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранные строки?", "Удаление", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var row in selectedRows)
                {
                    dbSet.Remove(row);
                }
                try
                {
                    dbEnt.SaveChanges();
                    await updateMethod();
                    MessageBox.Show("Данные успешно удалены.", "Успешное удаление данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void ExportDataGridToExcel(DataGrid dataGrid, string fileName)
        {
            var dt = new DataTable("Grid");
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (column.Header != null)
                {
                    dt.Columns.Add(column.Header.ToString());
                }
                else
                {
                    dt.Columns.Add();
                }
            }
            foreach (var row in dataGrid.Items)
            {
                DataRow newRow = dt.NewRow();
                for (int i = 0; i < dataGrid.Columns.Count; i++)
                {
                    newRow[i] = (dataGrid.Columns[i].GetCellContent(row) as TextBlock)?.Text;
                }
                dt.Rows.Add(newRow);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"{fileName}.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Файл занят, закройте файл и попробуйте еще раз");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        //Raspisanie
        private List<Расписание> originalListRaspisanie;
        private async Task LoadDataRaspisanie()
        {
            var КурсыIds = await dbEnt.Пользователи_Курсы.ToListAsync();
            var расписание = await dbEnt.Расписание.ToListAsync();
            var назвКурса = await dbEnt.Курсы.ToListAsync();
            Dispatcher.Invoke(() =>
            {
            NameKursRaspisanie.ItemsSource = назвКурса;
            NameKursRaspisanie.DisplayMemberPath = "Название";
            NameKursRaspisanie.SelectedValuePath = "ID_Курса";
            расписаниеDataGridRaspisanie.ItemsSource = расписание;
            originalListRaspisanie = расписание;
            });
            }
        private async Task ResetWindowRaspisanie()
        {
            InfoEditPanelRaspisanie.Visibility = Visibility.Hidden;
            InfoEditLabelRaspisanie.Visibility = Visibility.Hidden;
            EditRaspisanie.Visibility = Visibility.Hidden;
            AddRaspisanie.Visibility = Visibility.Hidden;
            CancelRaspisanie.Visibility = Visibility.Hidden;
            DelToRaspisanie.Visibility = Visibility.Visible;
            EditToRaspisanie.Visibility = Visibility.Visible;
            AddToRaspisanie.Visibility = Visibility.Visible;
            PowerSearchRaspisanie.Visibility = Visibility.Visible;
            ClearRaspisanie.Visibility = Visibility.Hidden;
            SearchTextBoxRaspisanie.Visibility = Visibility.Visible;
            SearchLabelRaspisanie.Visibility = Visibility.Visible;
            Thickness currentMargin = расписаниеDataGridRaspisanie.Margin;
            расписаниеDataGridRaspisanie.Margin = new Thickness(10, 10, 10, 84);
            PrintRaspisanie.Margin = new Thickness(284, 523, 0, 0);
            NameKursRaspisanie.SelectedIndex = -1;
            DatestartRaspisanie.SelectedDate = null;
            TimestartRaspisanie.SelectedTime = null;
            TimeendRaspisanie.SelectedTime = null;
            await LoadDataRaspisanie();
        }
        private void HidenVisibleMainButtonsRaspisanie()
        {
            Thickness currentMargin = расписаниеDataGridRaspisanie.Margin;
            расписаниеDataGridRaspisanie.Margin = new Thickness(396, 10, 10, 84);
            PrintRaspisanie.Margin = new Thickness(396, 523, 0, 0);
            EditToRaspisanie.Visibility = Visibility.Hidden;
            DelToRaspisanie.Visibility = Visibility.Hidden;
            AddToRaspisanie.Visibility = Visibility.Hidden;
            PowerSearchRaspisanie.Visibility = Visibility.Hidden;
            InfoEditPanelRaspisanie.Visibility = Visibility.Visible;
            InfoEditLabelRaspisanie.Visibility = Visibility.Visible;
            CancelRaspisanie.Visibility = Visibility.Visible;
            NameKursRaspisanie.SelectedIndex = -1;
            DatestartRaspisanie.SelectedDate = null;
            TimestartRaspisanie.SelectedTime = null;
            TimeendRaspisanie.SelectedTime = null;
            SearchTextBoxRaspisanie.Text = "";
        }
        private async void CancelClickRaspisanie(object sender, RoutedEventArgs e)
        {
            await ResetWindowRaspisanie();
        }
        private async void EditClickRaspisanie(object sender, RoutedEventArgs e)
        {
            var selectedRow = расписаниеDataGridRaspisanie.SelectedItem as Расписание;
            try
            {
                DateTime startTimeedit = DateTime.Today.Add(selectedRow.Время_начала ?? TimeSpan.Zero);
                DateTime endTimeedit = DateTime.Today.Add(selectedRow.Время_окончания ?? TimeSpan.Zero);
                selectedRow.ID_Курса = (int)NameKursRaspisanie.SelectedValue;
                selectedRow.Дата_начала = DatestartRaspisanie.SelectedDate;
                selectedRow.Время_начала = TimestartRaspisanie.SelectedTime?.TimeOfDay ?? TimeSpan.Zero;
                selectedRow.Время_окончания = TimeendRaspisanie.SelectedTime?.TimeOfDay;
                dbEnt.SaveChanges();
                dbEnt.Entry(selectedRow).Reload();
                await ResetWindowRaspisanie();
                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TimestartRaspisanieSelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (TimestartRaspisanie.SelectedTime >= TimeendRaspisanie.SelectedTime)
            {
                MessageBox.Show("Время начала не может быть позже или равно времени окончания");
                TimestartRaspisanie.SelectedTime = e.OldValue;
            }
        }
        private void TimeendRaspisanieSelectedTimeChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (TimeendRaspisanie.SelectedTime <= TimestartRaspisanie.SelectedTime)
            {
                MessageBox.Show("Время окончания не может быть раньше или равно времени начала");
                TimeendRaspisanie.SelectedTime = e.OldValue;
            }
        }
        private void EditToClickRaspisanie(object sender, RoutedEventArgs e)
        {
            var selectedRow = расписаниеDataGridRaspisanie.SelectedItem as Расписание;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsRaspisanie();
                EditRaspisanie.Visibility = Visibility.Visible;
                DateTime startTime = DateTime.Today.Add(selectedRow.Время_начала ?? TimeSpan.Zero);
                DateTime endTime = DateTime.Today.Add(selectedRow.Время_окончания ?? TimeSpan.Zero);
                NameKursRaspisanie.SelectedValue = selectedRow.ID_Курса;
                DatestartRaspisanie.SelectedDate = selectedRow.Дата_начала;
                TimestartRaspisanie.SelectedTime = startTime;
                TimeendRaspisanie.SelectedTime = endTime;
                PowerSearchRaspisanie.Visibility = Visibility.Hidden;
            }
        }
        private void AddToClickRaspisanie(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsRaspisanie();
            AddRaspisanie.Visibility = Visibility.Visible;
        }
        private void DeleteClickRaspisanie(object sender, RoutedEventArgs e)
        {
            var selectedRows = расписаниеDataGridRaspisanie.SelectedItems.Cast<Расписание>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Расписание, LoadDataRaspisanie);
        }
        private async void AddClickRaspisanie(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NameKursRaspisanie.SelectedValue == null
                    || DatestartRaspisanie.SelectedDate == null
                    || TimestartRaspisanie.SelectedTime == null
                    || TimeendRaspisanie.SelectedTime == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Расписание] ([ID_Курса], [Дата_начала], [Время_начала], [Время_окончания]) VALUES (@ID_Курса, @Дата_начала, @Время_начала, @Время_окончания)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@ID_Курса", NameKursRaspisanie.SelectedValue);
                    command.Parameters.AddWithValue("@Дата_начала", DatestartRaspisanie.SelectedDate);
                    command.Parameters.AddWithValue("@Время_начала", TimestartRaspisanie.SelectedTime);
                    command.Parameters.AddWithValue("@Время_окончания", TimeendRaspisanie.SelectedTime);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowRaspisanie();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchTextBoxTextChangedRaspisanie(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxRaspisanie.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                расписаниеDataGridRaspisanie.ItemsSource = originalListRaspisanie;
                return;
            }
            var Result = originalListRaspisanie.Where(d =>
                d.Курсы.Название.ToLower().Contains(Search.ToLower())
                || d.День_недели.ToLower().Contains(Search.ToLower())
                || (d.Дата_начала.HasValue && d.Дата_начала.Value.ToString("d").ToLower().Contains(Search.ToLower()))
                || (d.Время_начала.HasValue && d.Время_начала.Value.ToString().ToLower().Contains(Search.ToLower()))
                || (d.Время_окончания.HasValue && d.Время_окончания.Value.ToString().ToLower().Contains(Search.ToLower())));
            расписаниеDataGridRaspisanie.ItemsSource = Result.ToList();
        }
        private void PrintClickRaspisanie(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable("Grid");
            dt.Columns.Add("Название");
            dt.Columns.Add("Дата");
            dt.Columns.Add("День недели");
            dt.Columns.Add("Время начала");
            dt.Columns.Add("Время окончания");
            foreach (Расписание row in расписаниеDataGridRaspisanie.Items)
            {
                DataRow newRow = dt.NewRow();
                newRow[0] = row.Курсы.Название;
                newRow[1] = row.Дата_начала.HasValue ? row.Дата_начала.Value.ToString("d") : string.Empty;
                newRow[2] = row.День_недели;
                newRow[3] = row.Время_начала.HasValue ? row.Время_начала.Value.ToString() : string.Empty;
                newRow[4] = row.Время_окончания.HasValue ? row.Время_окончания.Value.ToString() : string.Empty;
                dt.Rows.Add(newRow);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Расписание.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Файл занят, закройте файл и попробуйте еще раз");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void PowerSearchRaspisanieClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsRaspisanie();
            SearchTextBoxRaspisanie.Visibility = Visibility.Hidden;
            SearchLabelRaspisanie.Visibility = Visibility.Hidden;
            SearchRaspisanie.Visibility = Visibility.Visible;
            ClearRaspisanie.Visibility = Visibility.Visible;
        }
        private async void SearchClickRaspisanie(object sender, RoutedEventArgs e)
        {
            var kurs = NameKursRaspisanie.SelectedValue;
            var dateStart = DatestartRaspisanie.SelectedDate;
            var timeStart = TimestartRaspisanie.SelectedTime;
            var timeEnd = TimeendRaspisanie.SelectedTime;
            if (originalListRaspisanie == null)
            {
                await LoadDataRaspisanie();
            }
            IEnumerable<Расписание> result = originalListRaspisanie;
            if (kurs != null)
            {
                result = result.Where(d => d.Курсы.ID_Курса == (int)kurs);
            }
            if (dateStart.HasValue)
            {
                result = result.Where(d => d.Дата_начала.HasValue && d.Дата_начала.Value.Date == dateStart.Value.Date);
            }
            if (timeStart.HasValue)
            {
                result = result.Where(d => d.Время_начала.HasValue && d.Время_начала.Value == timeStart.Value.TimeOfDay);
            }
            if (timeEnd.HasValue)
            {
                result = result.Where(d => d.Время_окончания.HasValue && d.Время_окончания.Value == timeEnd.Value.TimeOfDay);
            }
            расписаниеDataGridRaspisanie.ItemsSource = result.ToList();
        }
        private async void ClearClickRaspisanie(object sender, EventArgs e)
        {
            NameKursRaspisanie.SelectedIndex = -1;
            DatestartRaspisanie.SelectedDate = null;
            TimestartRaspisanie.SelectedTime = null;
            TimeendRaspisanie.SelectedTime = null;
            await LoadDataRaspisanie();
        }
        //Kursi
        private List<Курсы> originalListKursi;
        private async Task LoadDataKursi()
        {
            var КурсыIds = await dbEnt.Пользователи_Курсы.ToListAsync();
            var курсы = await dbEnt.Курсы.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                курсыDataGridKursi.ItemsSource = курсы;
                originalListKursi = курсы;
            });
        }
        private void HidenVisibleMainButtonsKursi()
        {
            Thickness currentMargin = курсыDataGridKursi.Margin;
            курсыDataGridKursi.Margin = new Thickness(396, 10, 10, 84);
            PrintKursi.Margin = new Thickness(396, 523, 0, 0);
            EditToKursi.Visibility = Visibility.Hidden;
            DelToKursi.Visibility = Visibility.Hidden;
            AddToKursi.Visibility = Visibility.Hidden;
            PowerSearchKursi.Visibility = Visibility.Hidden;
            InfoEditPanelKursi.Visibility = Visibility.Visible;
            InfoEditLabelKursi.Visibility = Visibility.Visible;
            CancelKursi.Visibility = Visibility.Visible;
            NameKursKursi.Text = "";
            OpisanieKursi.Text = "";
            DatestartKursi.SelectedDate = null;
            DateendKursi.SelectedDate = null;
            SearchTextBoxKursi.Text = "";
        }
        private async Task ResetWindowKursi()
        {
            InfoEditPanelKursi.Visibility = Visibility.Hidden;
            InfoEditLabelKursi.Visibility = Visibility.Hidden;
            EditKursi.Visibility = Visibility.Hidden;
            AddKursi.Visibility = Visibility.Hidden;
            ClearKursi.Visibility = Visibility.Hidden;
            CancelKursi.Visibility = Visibility.Hidden;
            SearchKursi.Visibility = Visibility.Hidden;
            ClearKursi.Visibility = Visibility.Hidden;
            DelToKursi.Visibility = Visibility.Visible;
            EditToKursi.Visibility = Visibility.Visible;
            AddToKursi.Visibility = Visibility.Visible;
            PowerSearchKursi.Visibility = Visibility.Visible;
            SearchTextBoxKursi.Visibility = Visibility.Visible;
            SearchLabelKursi.Visibility = Visibility.Visible;
            Thickness currentMargin = курсыDataGridKursi.Margin;
            курсыDataGridKursi.Margin = new Thickness(10, 10, 10, 84);
            PrintKursi.Margin = new Thickness(284, 523, 0, 0);
            NameKursKursi.Text = "";
            OpisanieKursi.Text = "";
            DatestartKursi.SelectedDate = null;
            DateendKursi.SelectedDate = null;
            await LoadDataKursi();
        }
        private void DeleteClickKursi(object sender, RoutedEventArgs e)
        {
            var selectedRows = курсыDataGridKursi.SelectedItems.Cast<Курсы>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Курсы, LoadDataKursi);
        }
        private void EditToClickKursi(object sender, RoutedEventArgs e)
        {
            var selectedRow = курсыDataGridKursi.SelectedItem as Курсы;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsKursi();
                EditKursi.Visibility = Visibility.Visible;
                NameKursKursi.Text = selectedRow.Название;
                OpisanieKursi.Text = selectedRow.Описание;
                DatestartKursi.SelectedDate = selectedRow.Дата_начала;
                DateendKursi.SelectedDate = selectedRow.Дата_окончания;
            }
        }
        private void AddToClickKursi(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsKursi();
            AddKursi.Visibility = Visibility.Visible;
        }
        private void PrintClickKursi(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable("Grid");
            dt.Columns.Add("Название");
            dt.Columns.Add("Описание");
            dt.Columns.Add("Дата начала");
            dt.Columns.Add("Дата окончания");
            foreach (Курсы row in курсыDataGridKursi.Items)
            {
                DataRow newRow = dt.NewRow();
                newRow[0] = row.Название;
                newRow[1] = row.Описание;
                newRow[2] = row.Дата_начала.HasValue ? row.Дата_начала.Value.ToString("d") : string.Empty;
                newRow[3] = row.Дата_окончания.HasValue ? row.Дата_окончания.Value.ToString("d") : string.Empty;
                dt.Rows.Add(newRow);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Курсы.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Файл занят, закройте файл и попробуйте еще раз");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void PowerSearchKursiClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsKursi();
            SearchTextBoxKursi.Visibility = Visibility.Hidden;
            SearchLabelKursi.Visibility = Visibility.Hidden;
            SearchKursi.Visibility = Visibility.Visible;
            ClearKursi.Visibility = Visibility.Visible;
        }
        private void SearchTextBoxTextChangedKursi(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxKursi.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                курсыDataGridKursi.ItemsSource = originalListKursi;
                return;
            }
            var Result = originalListKursi.Where(d =>
                d.Название.ToLower().Contains(Search.ToLower())
                || d.Описание.ToLower().Contains(Search.ToLower())
                || (d.Дата_начала.HasValue && d.Дата_начала.Value.ToString("d").ToLower().Contains(Search.ToLower()))
                || (d.Дата_окончания.HasValue && d.Дата_начала.Value.ToString("d").ToLower().Contains(Search.ToLower())));
            курсыDataGridKursi.ItemsSource = Result.ToList();
        }
        private async void CancelClickKursi(object sender, RoutedEventArgs e)
        {
            await ResetWindowKursi();
        }
        private async void AddClickKursi(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NameKursKursi.Text == null
                    || OpisanieKursi.Text == null
                    || DatestartKursi.SelectedDate == null
                    || DateendKursi.SelectedDate == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Курсы] ([Название], [Описание], [Дата_начала], [Дата_окончания]) VALUES (@Название, @Описание, @Дата_начала, @Дата_окончания)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@Название", NameKursKursi.Text);
                    command.Parameters.AddWithValue("@Описание", OpisanieKursi.Text);
                    command.Parameters.AddWithValue("@Дата_начала", DatestartKursi.SelectedDate);
                    command.Parameters.AddWithValue("@Дата_окончания", DateendKursi.SelectedDate);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowKursi();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchClickKursi(object sender, RoutedEventArgs e)
        {
            var name = NameKursKursi.Text;
            var opisanie = OpisanieKursi.Text;
            var dateStart = DatestartKursi.SelectedDate;
            var dateEnd = DateendKursi.SelectedDate;

            if (originalListKursi == null)
            {
                originalListKursi = dbEnt.Курсы.ToList();
            }
            IEnumerable<Курсы> result = originalListKursi;
            if (name != null)
            {
                result = result.Where(d => d.Название.ToLower().Contains(name.ToLower()));
            }
            if (opisanie != null)
            {
                result = result.Where(d => d.Описание.ToLower().Contains(opisanie.ToLower()));
            }
            if (dateStart.HasValue)
            {
                result = result.Where(d => d.Дата_начала.HasValue && d.Дата_начала.Value.Date == dateStart.Value.Date);
            }
            if (dateEnd.HasValue)
            {
                result = result.Where(d => d.Дата_окончания.HasValue && d.Дата_окончания.Value.Date == dateEnd.Value.Date);
            }
            курсыDataGridKursi.ItemsSource = result.ToList();
        }
        private async void EditClickKursi(object sender, RoutedEventArgs e)
        {
            var selectedRow = курсыDataGridKursi.SelectedItem as Курсы;
            try
            {
                selectedRow.Название = NameKursKursi.Text;
                selectedRow.Описание = OpisanieKursi.Text;
                selectedRow.Дата_начала = DatestartKursi.SelectedDate;
                selectedRow.Дата_окончания = DateendKursi.SelectedDate;
                dbEnt.SaveChanges();
                await ResetWindowKursi();
                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ClearClickKursi(object sender, EventArgs e)
        {
            NameKursKursi.Text = "";
            OpisanieKursi.Text = "";
            DatestartKursi.SelectedDate = null;
            DateendKursi.SelectedDate = null;
            await LoadDataKursi();
        }
        private void DatePickerStartSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateendKursi.SelectedDate <= DatestartKursi.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже или равна дате окончания");
                DatestartKursi.SelectedDate = null;
            }
        }
        private void DatePickerEndSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatestartKursi.SelectedDate >= DateendKursi.SelectedDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше или равна дате начала");
                DateendKursi.SelectedDate = null;
            }
        }
        //Uch
        private List<Учащиеся> originalListUch;
        private async Task LoadDataUch()
        {
            var КурсыIds = await dbEnt.Пользователи_Курсы.ToListAsync();
            var УчащиесяIds = await dbEnt.Курсы_Учащихся.ToListAsync();
            var учащиеся = await dbEnt.Учащиеся.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                учащиесяDataGridUch.ItemsSource = учащиеся;
                originalListUch = учащиеся;
            });
        }
        private void HidenVisibleMainButtonsUch()
        {
            Thickness currentMargin = учащиесяDataGridUch.Margin;
            учащиесяDataGridUch.Margin = new Thickness(396, 10, 10, 84);
            PrintUch.Margin = new Thickness(396, 523, 0, 0);
            EditToUch.Visibility = Visibility.Hidden;
            DelToUch.Visibility = Visibility.Hidden;
            AddToUch.Visibility = Visibility.Hidden;
            PowerSearchUch.Visibility = Visibility.Hidden;
            InfoEditPanelUch.Visibility = Visibility.Visible;
            InfoEditLabelUch.Visibility = Visibility.Visible;
            CancelUch.Visibility = Visibility.Visible;
            NameUch.Text = "";
            SurnameUch.Text = "";
            OtchUch.Text = "";
            DateOfBirthUch.SelectedDate = null;
            GenderUch.SelectedIndex = -1;
            SearchTextBoxUch.Text = "";
        }
        private async Task ResetWindowUch()
        {
            InfoEditPanelUch.Visibility = Visibility.Hidden;
            InfoEditLabelUch.Visibility = Visibility.Hidden;
            EditUch.Visibility = Visibility.Hidden;
            AddUch.Visibility = Visibility.Hidden;
            ClearUch.Visibility = Visibility.Hidden;
            CancelUch.Visibility = Visibility.Hidden;
            SearchUch.Visibility = Visibility.Hidden;
            ClearUch.Visibility = Visibility.Hidden;
            DelToUch.Visibility = Visibility.Visible;
            EditToUch.Visibility = Visibility.Visible;
            AddToUch.Visibility = Visibility.Visible;
            PowerSearchUch.Visibility = Visibility.Visible;
            SearchTextBoxUch.Visibility = Visibility.Visible;
            SearchLabelUch.Visibility = Visibility.Visible;
            Thickness currentMargin = учащиесяDataGridUch.Margin;
            учащиесяDataGridUch.Margin = new Thickness(10, 10, 10, 84);
            PrintUch.Margin = new Thickness(284, 523, 0, 0);
            NameUch.Text = "";
            SurnameUch.Text = "";
            OtchUch.Text = "";
            DateOfBirthUch.SelectedDate = null;
            GenderUch.SelectedIndex = -1;
            await LoadDataUch();
        }
        private void DeleteClickUch(object sender, RoutedEventArgs e)
        {
            var selectedRows = учащиесяDataGridUch.SelectedItems.Cast<Учащиеся>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Учащиеся, LoadDataUch);
        }
        private void EditToClickUch(object sender, RoutedEventArgs e)
        {
            var selectedRow = учащиесяDataGridUch.SelectedItem as Учащиеся;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsUch();
                EditUch.Visibility = Visibility.Visible;
                NameUch.Text = selectedRow.Имя;
                SurnameUch.Text = selectedRow.Фамилия;
                OtchUch.Text = selectedRow.Отчество;
                DateOfBirthUch.SelectedDate = selectedRow.Дата_рождения;
                foreach (ComboBoxItem item in GenderUch.Items)
                {
                    if (item.Content.ToString() == selectedRow.Пол)
                    {
                        GenderUch.SelectedItem = item;
                        break;
                    }
                }
            }
        }
        private void AddToClickUch(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsUch();
            AddUch.Visibility = Visibility.Visible;
        }
        private void PrintClickUch(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable("Grid");
            dt.Columns.Add("Имя");
            dt.Columns.Add("Фамилия");
            dt.Columns.Add("Отчество");
            dt.Columns.Add("Дата рождения");
            dt.Columns.Add("Пол");

            foreach (Учащиеся row in учащиесяDataGridUch.Items)
            {
                DataRow newRow = dt.NewRow();
                newRow[0] = row.Имя;
                newRow[1] = row.Фамилия;
                newRow[2] = row.Отчество;
                newRow[3] = row.Дата_рождения.HasValue ? row.Дата_рождения.Value.ToString("d") : string.Empty;
                newRow[4] = row.Пол;
                dt.Rows.Add(newRow);
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Учащиеся.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Файл занят, закройте файл и попробуйте еще раз");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void PowerSearchUchClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsUch();
            SearchTextBoxUch.Visibility = Visibility.Hidden;
            SearchLabelUch.Visibility = Visibility.Hidden;
            SearchUch.Visibility = Visibility.Visible;
            ClearUch.Visibility = Visibility.Visible;
        }
        private void SearchTextBoxTextChangedUch(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxUch.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                учащиесяDataGridUch.ItemsSource = originalListUch;
                return;
            }
            var Result = originalListUch.Where(d =>
                d.Имя.ToLower().Contains(Search.ToLower())
                || d.Фамилия.ToLower().Contains(Search.ToLower())
                || d.Отчество.ToLower().Contains(Search.ToLower())
                || (d.Дата_рождения.HasValue && d.Дата_рождения.Value.ToString("d").ToLower().Contains(Search.ToLower()))
                || d.Пол.ToLower().Contains(Search.ToLower()));
            учащиесяDataGridUch.ItemsSource = Result.ToList();
        }
        private async void CancelClickUch(object sender, RoutedEventArgs e)
        {
            await ResetWindowUch();
        }
        private async void AddClickUch(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NameUch.Text == null
                || SurnameUch.Text == null
                || OtchUch.Text == null
                || DateOfBirthUch.SelectedDate == null
                || GenderUch.SelectedItem == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Учащиеся] ([Имя], [Фамилия], [Отчество], [Дата_рождения], [Пол]) VALUES (@Имя, @Фамилия, @Отчество, @Дата_рождения, @Пол)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@Имя", NameUch.Text);
                    command.Parameters.AddWithValue("@Фамилия", SurnameUch.Text);
                    command.Parameters.AddWithValue("@Отчество", OtchUch.Text);
                    command.Parameters.AddWithValue("@Дата_рождения", DateOfBirthUch.SelectedDate);
                    command.Parameters.AddWithValue("@Пол", GenderUch.Text);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowUch();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchClickUch(object sender, RoutedEventArgs e)
        {
            var name = NameUch.Text;
            var familia = SurnameUch.Text;
            var otch = OtchUch.Text;
            var datarosjdenia = DateOfBirthUch.SelectedDate;
            var pol = GenderUch.SelectedItem as ComboBoxItem;

            if (originalListUch == null)
            {
                originalListUch = dbEnt.Учащиеся.ToList();
            }
            IEnumerable<Учащиеся> result = originalListUch;
            if (name != null)
            {
                result = result.Where(d => d.Имя.ToLower().Contains(name.ToLower()));
            }
            if (familia != null)
            {
                result = result.Where(d => d.Фамилия.ToLower().Contains(familia.ToLower()));
            }
            if (otch != null)
            {
                result = result.Where(d => d.Отчество.ToLower().Contains(otch.ToLower()));
            }
            if (datarosjdenia.HasValue)
            {
                result = result.Where(d => d.Дата_рождения.HasValue && d.Дата_рождения.Value.Date == datarosjdenia.Value.Date);
            }
            if (pol != null)
            {
                var polStr = pol.Content.ToString();
                result = result.Where(d => d.Пол.ToLower() == polStr.ToLower());
            }
            учащиесяDataGridUch.ItemsSource = result.ToList();
        }
        private async void EditClickUch(object sender, RoutedEventArgs e)
        {
            var selectedRow = учащиесяDataGridUch.SelectedItem as Учащиеся;
            try
            {
                selectedRow.Имя = NameUch.Text;
                selectedRow.Фамилия = SurnameUch.Text;
                selectedRow.Отчество = SurnameUch.Text;
                selectedRow.Дата_рождения = DateOfBirthUch.SelectedDate;
                selectedRow.Пол = GenderUch.Text;
                dbEnt.SaveChanges();
                await ResetWindowUch();

                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ClearClickUch(object sender, EventArgs e)
        {
            NameUch.Text = "";
            SurnameUch.Text = "";
            OtchUch.Text = "";
            DateOfBirthUch.SelectedDate = null;
            GenderUch.SelectedIndex = -1;
            await LoadDataUch();
        }
        //Dost
        private List<Пользователи_Курсы> originalListDost;
        private async Task LoadDataDost()
        {
            var usersCourses = await dbEnt.Пользователи_Курсы.ToListAsync();
            var users = await dbEnt.Регистрация.ToListAsync();
            var courses = await dbEnt.Курсы.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                доступDataGridDost.ItemsSource = usersCourses;
                LoginDost.ItemsSource = users;
                LoginDost.DisplayMemberPath = "Логин";
                LoginDost.SelectedValuePath = "ID_Пользователя";
                CourseDost.ItemsSource = courses;
                CourseDost.DisplayMemberPath = "Название";
                CourseDost.SelectedValuePath = "ID_Курса";
            });
        }
        private void HidenVisibleMainButtonsDost()
        {
            Thickness currentMargin = доступDataGridDost.Margin;
            доступDataGridDost.Margin = new Thickness(396, 10, 10, 84);
            PrintDost.Margin = new Thickness(396, 523, 0, 0);
            EditToDost.Visibility = Visibility.Hidden;
            DelToDost.Visibility = Visibility.Hidden;
            AddToDost.Visibility = Visibility.Hidden;
            PowerSearchDost.Visibility = Visibility.Hidden;
            InfoEditPanelDost.Visibility = Visibility.Visible;
            InfoEditLabelDost.Visibility = Visibility.Visible;
            CancelDost.Visibility = Visibility.Visible;
            LoginDost.Text = "";
            CourseDost.Text = "";
        }
        private async Task ResetWindowDost()
        {
            InfoEditPanelDost.Visibility = Visibility.Hidden;
            InfoEditLabelDost.Visibility = Visibility.Hidden;
            EditDost.Visibility = Visibility.Hidden;
            AddDost.Visibility = Visibility.Hidden;
            ClearDost.Visibility = Visibility.Hidden;
            CancelDost.Visibility = Visibility.Hidden;
            SearchDost.Visibility = Visibility.Hidden;
            ClearDost.Visibility = Visibility.Hidden;
            DelToDost.Visibility = Visibility.Visible;
            EditToDost.Visibility = Visibility.Visible;
            AddToDost.Visibility = Visibility.Visible;
            PowerSearchDost.Visibility = Visibility.Visible;
            SearchTextBoxDost.Visibility = Visibility.Visible;
            SearchLabelDost.Visibility = Visibility.Visible;
            Thickness currentMargin = доступDataGridDost.Margin;
            доступDataGridDost.Margin = new Thickness(10, 10, 10, 84);
            PrintDost.Margin = new Thickness(284, 523, 0, 0);
            LoginDost.Text = "";
            CourseDost.Text = "";
            await LoadDataDost();
        }
        private async void DeleteClickDost(object sender, RoutedEventArgs e)
        {
            var selectedRows = доступDataGridDost.SelectedItems.Cast<object>().ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Чтобы удалить - выберите строку!");
                return;
            }
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранные строки?", "Удаление", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool success = true;
                foreach (var row in selectedRows)
                {
                    var id = (int)row.GetType().GetProperty("ID_Пользователи_Курсы").GetValue(row, null);
                    var userCourseToDelete = dbEnt.Пользователи_Курсы.FirstOrDefault(p => p.ID_Пользователи_Курсы == id);
                    if (userCourseToDelete != null)
                    {
                        dbEnt.Пользователи_Курсы.Remove(userCourseToDelete);
                    }
                    else
                    {
                        success = false;
                        MessageBox.Show($"Не удалось найти запись с ID {id} в базе данных.", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                dbEnt.SaveChanges();
                await LoadDataDost();
                if (success)
                {
                    MessageBox.Show("Данные успешно удалены.", "Успешное удаление данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void EditToClickDost(object sender, RoutedEventArgs e)
        {
            var selectedRow = доступDataGridDost.SelectedItem as Пользователи_Курсы;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsDost();
                EditDost.Visibility = Visibility.Visible;
                LoginDost.SelectedValue = selectedRow.ID_Пользователя;
                CourseDost.SelectedValue = selectedRow.ID_Курса;
            }
        }
        private void AddToClickDost(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsDost();
            AddDost.Visibility = Visibility.Visible;
        }
        private void PrintClickDost(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(доступDataGridDost, "Пользователи доступ");
        }
        private void PowerSearchDostClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsDost();
            SearchTextBoxDost.Visibility = Visibility.Hidden;
            SearchLabelDost.Visibility = Visibility.Hidden;
            SearchDost.Visibility = Visibility.Visible;
            ClearDost.Visibility = Visibility.Visible;
        }
        private void SearchTextBoxTextChangedDost(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxDost.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                доступDataGridDost.ItemsSource = originalListDost;
                return;
            }
            var Result = originalListDost.Where(d =>
                d.Регистрация.Логин.ToLower().Contains(Search.ToLower())
                || d.Регистрация.Пароль.ToLower().Contains(Search.ToLower())
                || d.Курсы.Название.ToLower().Contains(Search.ToLower()));
            доступDataGridDost.ItemsSource = Result.ToList();
        }
        private async void CancelClickDost(object sender, RoutedEventArgs e)
        {
            await ResetWindowDost();
        }
        private async void AddClickDost(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginDost.SelectedValue == null || CourseDost.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string checkQuery = "SELECT COUNT(*) FROM [dbo].[Пользователи_Курсы] WHERE [ID_Пользователя] = @ID_Пользователя AND [ID_Курса] = @ID_Курса";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, db.getConnection()))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Пользователя", LoginDost.SelectedValue);
                    checkCommand.Parameters.AddWithValue("@ID_Курса", CourseDost.SelectedValue);
                    int existingCount = (int)checkCommand.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        MessageBox.Show("Такие данные уже существуют.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                        db.closeConnection();
                        return;
                    }
                }
                string query = "INSERT INTO [dbo].[Пользователи_Курсы] ([ID_Пользователя], [ID_Курса]) VALUES (@ID_Пользователя, @ID_Курса)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@ID_Пользователя", LoginDost.SelectedValue);
                    command.Parameters.AddWithValue("@ID_Курса", CourseDost.SelectedValue);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowDost();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchClickDost(object sender, RoutedEventArgs e)
        {
            var login = LoginDost.Text;
            var course = CourseDost.Text;

            if (originalListDost == null)
            {
                originalListDost = dbEnt.Пользователи_Курсы.ToList();
            }
            IEnumerable<Пользователи_Курсы> result = originalListDost;
            if (login != null)
            {
                result = result.Where(d => d.Регистрация.Логин.ToLower().Contains(login.ToLower()));
            }
            if (course != null)
            {
                result = result.Where(d => d.Курсы.Название.ToLower().Contains(course.ToLower()));
            }
            доступDataGridDost.ItemsSource = result.ToList();
        }
        private async void EditClickDost(object sender, RoutedEventArgs e)
        {
            var selectedRow = доступDataGridDost.SelectedItem as Пользователи_Курсы;
            try
            {
                selectedRow.ID_Пользователя = (int)LoginDost.SelectedValue;
                selectedRow.ID_Курса = (int)CourseDost.SelectedValue;
                dbEnt.SaveChanges();
                await ResetWindowDost();

                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ClearClickDost(object sender, EventArgs e)
        {
            LoginDost.Text = "";
            CourseDost.Text = "";
            await LoadDataDost();
        }
        //Itog
        private List<Итоговые_Работы> originalListItog;
        private async Task LoadDataItog()
        {
            var КурсыIds = await dbEnt.Пользователи_Курсы.ToListAsync();
            var Итоговые_Работы = await dbEnt.Итоговые_Работы.ToListAsync();
            var назвКурсы = dbEnt.Курсы.ToList();
            var фамилТиг = dbEnt.Учащиеся.ToList();
            Dispatcher.Invoke(() =>
            {
                NameCursItog.ItemsSource = назвКурсы;
                FamilItog.ItemsSource = фамилТиг;
                NameCursItog.DisplayMemberPath = "Название";
                NameCursItog.SelectedValuePath = "ID_Курса";
                FamilItog.DisplayMemberPath = "Фамилия";
                FamilItog.SelectedValuePath = "ID_Учащегося";
                ИтоговыеРаботыDataGridItog.ItemsSource = Итоговые_Работы;
                originalListItog = Итоговые_Работы;
            });
        }
        private async Task ResetWindowItog()
        {
            InfoEditPanelItog.Visibility = Visibility.Hidden;
            InfoEditLabelItog.Visibility = Visibility.Hidden;
            EditItog.Visibility = Visibility.Hidden;
            AddItog.Visibility = Visibility.Hidden;
            CancelItog.Visibility = Visibility.Hidden;
            DelToItog.Visibility = Visibility.Visible;
            EditToItog.Visibility = Visibility.Visible;
            AddToItog.Visibility = Visibility.Visible;
            PowerSearchItog.Visibility = Visibility.Visible;
            ClearItog.Visibility = Visibility.Hidden;
            SearchTextBoxItog.Visibility = Visibility.Visible;
            SearchLabelItog.Visibility = Visibility.Visible;
            Thickness currentMargin = ИтоговыеРаботыDataGridItog.Margin;
            ИтоговыеРаботыDataGridItog.Margin = new Thickness(10, 10, 10, 84);
            PrintItog.Margin = new Thickness(284, 523, 0, 0);
            FamilItog.Text = "";
            NameCursItog.Text = "";
            NameItogRabItog.Text = "";
            DescItog.Text = "";
            OcenkaItog.SelectedIndex = -1;
            await LoadDataItog();
        }
        private void HidenVisibleMainButtonsItog()
        {
            Thickness currentMargin = ИтоговыеРаботыDataGridItog.Margin;
            ИтоговыеРаботыDataGridItog.Margin = new Thickness(396, 10, 10, 84);
            PrintItog.Margin = new Thickness(396, 523, 0, 0);
            EditToItog.Visibility = Visibility.Hidden;
            DelToItog.Visibility = Visibility.Hidden;
            AddToItog.Visibility = Visibility.Hidden;
            PowerSearchItog.Visibility = Visibility.Hidden;
            InfoEditPanelItog.Visibility = Visibility.Visible;
            InfoEditLabelItog.Visibility = Visibility.Visible;
            CancelItog.Visibility = Visibility.Visible;
            FamilItog.Text = "";
            NameCursItog.Text = "";
            NameItogRabItog.Text = "";
            DescItog.Text = "";
            OcenkaItog.SelectedIndex = -1;
            SearchTextBoxItog.Text = "";
        }
        private async void CancelClickItog(object sender, RoutedEventArgs e)
        {
            await ResetWindowItog();
        }
        private async void EditClickItog(object sender, RoutedEventArgs e)
        {
            var selectedRow = ИтоговыеРаботыDataGridItog.SelectedItem as Итоговые_Работы;
            try
            {
                selectedRow.ID_Учащегося = (int)FamilItog.SelectedValue;
                selectedRow.ID_Курса = (int)NameCursItog.SelectedValue;
                selectedRow.Название = NameItogRabItog.Text;
                selectedRow.Описание = DescItog.Text;
                int ocenka;
                if (int.TryParse(OcenkaItog.Text, out ocenka))
                {
                    selectedRow.Оценка = ocenka;
                }
                else
                {
                    MessageBox.Show("Оценка должна быть числом.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                dbEnt.SaveChanges();
                await ResetWindowItog();
                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditToClickItog(object sender, RoutedEventArgs e)
        {
            var selectedRow = ИтоговыеРаботыDataGridItog.SelectedItem as Итоговые_Работы;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsItog();
                EditItog.Visibility = Visibility.Visible;
                FamilItog.SelectedValue = selectedRow.ID_Учащегося;
                NameCursItog.SelectedValue = selectedRow.ID_Курса;
                NameItogRabItog.Text = selectedRow.Название;
                DescItog.Text = selectedRow.Описание;
                OcenkaItog.Text = selectedRow.Оценка.ToString();
                PowerSearchItog.Visibility = Visibility.Hidden;
            }
        }
        private void AddToClickItog(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsItog();
            AddItog.Visibility = Visibility.Visible;
        }
        private void DeleteClickItog(object sender, RoutedEventArgs e)
        {
            var selectedRows = ИтоговыеРаботыDataGridItog.SelectedItems.Cast<Итоговые_Работы>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Итоговые_Работы, LoadDataItog);
        }
        private async void AddClickItog(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FamilItog.SelectedValue == null
                    || NameCursItog.SelectedValue == null
                    || NameItogRabItog.Text == null
                    || DescItog.Text == null
                    || OcenkaItog.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Итоговые_Работы] ([ID_Учащегося], [ID_Курса], [Название], [Описание], [Оценка]) VALUES (@ID_Учащегося, @ID_Курса, @Название, @Описание, @Оценка)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@ID_Учащегося", FamilItog.SelectedValue);
                    command.Parameters.AddWithValue("@ID_Курса", NameCursItog.SelectedValue);
                    command.Parameters.AddWithValue("@Название", NameItogRabItog.Text);
                    command.Parameters.AddWithValue("@Описание", DescItog.Text);
                    command.Parameters.AddWithValue("@Оценка", OcenkaItog.Text);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowItog();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchTextBoxTextChangedItog(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxItog.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                ИтоговыеРаботыDataGridItog.ItemsSource = originalListItog;
                return;
            }
            var Result = originalListItog.Where(d =>
                d.Учащиеся.Фамилия.ToLower().Contains(Search.ToLower())
                || d.Курсы.Название.ToLower().Contains(Search.ToLower())
                || d.Название.ToLower().Contains(Search.ToLower())
                || d.Описание.ToLower().Contains(Search.ToLower())
                || d.Оценка.Value.ToString().ToLower().Contains(Search.ToLower()));
            ИтоговыеРаботыDataGridItog.ItemsSource = Result.ToList();
        }
        private void PrintClickItog(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(ИтоговыеРаботыDataGridItog, "Итоговые работы");
        }
        private void PowerSearchItogClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsItog();
            SearchTextBoxItog.Visibility = Visibility.Hidden;
            SearchLabelItog.Visibility = Visibility.Hidden;
            SearchItog.Visibility = Visibility.Visible;
            ClearItog.Visibility = Visibility.Visible;
        }
        private async void SearchClickItog(object sender, RoutedEventArgs e)
        {
            var Famil = FamilItog.Text;
            var NameCurs = NameCursItog.Text;
            var NameItogRab = NameItogRabItog.Text;
            var Desc = DescItog.Text;
            var Ocenka = OcenkaItog.Text;
            if (originalListItog == null)
            {
                await LoadDataItog();
            }
            IEnumerable<Итоговые_Работы> result = originalListItog;
            if (Famil != null)
            {
                result = result.Where(d => d.Учащиеся.Фамилия.ToLower().Contains(Famil.ToLower()));
            }
            if (NameCurs != null)
            {
                result = result.Where(d => d.Курсы.Название.ToLower().Contains(NameCurs.ToLower()));
            }
            if (NameItogRab != null)
            {
                result = result.Where(d => d.Название.ToLower().Contains(NameItogRab.ToLower()));
            }
            if (Desc != null)
            {
                result = result.Where(d => d.Описание.ToLower().Contains(Desc.ToLower()));
            }
            if (Ocenka != null)
            {
                result = result.Where(d => d.Оценка.Value.ToString().ToLower().Contains(Ocenka.ToLower()));
            }
            ИтоговыеРаботыDataGridItog.ItemsSource = result.ToList();
        }
        private async void ClearClickItog(object sender, EventArgs e)
        {
            FamilItog.Text = "";
            NameCursItog.Text = "";
            NameItogRabItog.Text = "";
            DescItog.Text = "";
            OcenkaItog.SelectedIndex = -1;
            await LoadDataItog();
        }
        //Polzov
        private List<Регистрация> originalListPolzov;
        private async Task LoadDataPolzov()
        {
            var usersCourses = await dbEnt.Регистрация.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                регистрацияDataGridPolzov.ItemsSource = usersCourses;
            });
        }
        private void HidenVisibleMainButtonsPolzov()
        {
            Thickness currentMargin = регистрацияDataGridPolzov.Margin;
            регистрацияDataGridPolzov.Margin = new Thickness(396, 10, 10, 84);
            PrintPolzov.Margin = new Thickness(396, 523, 0, 0);
            EditToPolzov.Visibility = Visibility.Hidden;
            DelToPolzov.Visibility = Visibility.Hidden;
            AddToPolzov.Visibility = Visibility.Hidden;
            PowerSearchPolzov.Visibility = Visibility.Hidden;
            InfoEditPanelPolzov.Visibility = Visibility.Visible;
            InfoEditLabelPolzov.Visibility = Visibility.Visible;
            CancelPolzov.Visibility = Visibility.Visible;
            LoginPolzov.Text = "";
            PassPolzov.Text = "";
            SearchTextBoxPolzov.Text = "";
        }
        private async Task ResetWindowPolzov()
        {
            InfoEditPanelPolzov.Visibility = Visibility.Hidden;
            InfoEditLabelPolzov.Visibility = Visibility.Hidden;
            EditPolzov.Visibility = Visibility.Hidden;
            AddPolzov.Visibility = Visibility.Hidden;
            ClearPolzov.Visibility = Visibility.Hidden;
            CancelPolzov.Visibility = Visibility.Hidden;
            SearchPolzov.Visibility = Visibility.Hidden;
            ClearPolzov.Visibility = Visibility.Hidden;
            DelToPolzov.Visibility = Visibility.Visible;
            EditToPolzov.Visibility = Visibility.Visible;
            AddToPolzov.Visibility = Visibility.Visible;
            PowerSearchPolzov.Visibility = Visibility.Visible;
            SearchTextBoxPolzov.Visibility = Visibility.Visible;
            SearchLabelPolzov.Visibility = Visibility.Visible;
            Thickness currentMargin = регистрацияDataGridPolzov.Margin;
            регистрацияDataGridPolzov.Margin = new Thickness(10, 10, 10, 84);
            PrintPolzov.Margin = new Thickness(284, 523, 0, 0);
            LoginPolzov.Text = "";
            PassPolzov.Text = "";
            await LoadDataPolzov();
        }
        private void DeleteClickPolzov(object sender, RoutedEventArgs e)
        {
            var selectedRows = регистрацияDataGridPolzov.SelectedItems.Cast<Регистрация>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Регистрация, LoadDataPolzov);
        }
        private void EditToClickPolzov(object sender, RoutedEventArgs e)
        {
            var selectedRow = регистрацияDataGridPolzov.SelectedItem as Регистрация;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsPolzov();
                EditPolzov.Visibility = Visibility.Visible;
                LoginPolzov.Text = selectedRow.Логин;
                PassPolzov.Text = selectedRow.Пароль;
            }
        }
        private void AddToClickPolzov(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsPolzov();
            AddPolzov.Visibility = Visibility.Visible;
        }
        private void PrintClickPolzov(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(регистрацияDataGridPolzov, "Пользователи");
        }
        private void PowerSearchPolzovClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsPolzov();
            SearchTextBoxPolzov.Visibility = Visibility.Hidden;
            SearchLabelPolzov.Visibility = Visibility.Hidden;
            SearchPolzov.Visibility = Visibility.Visible;
            ClearPolzov.Visibility = Visibility.Visible;
        }
        private void SearchTextBoxTextChangedPolzov(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxPolzov.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                регистрацияDataGridPolzov.ItemsSource = originalListPolzov;
                return;
            }
            var Result = originalListPolzov.Where(d =>
                d.Логин.ToLower().Contains(Search.ToLower())
                || d.Пароль.ToLower().Contains(Search.ToLower()));
            регистрацияDataGridPolzov.ItemsSource = Result.ToList();
        }
        private async void CancelClickPolzov(object sender, RoutedEventArgs e)
        {
            await ResetWindowPolzov();
        }
        private async void AddClickPolzov(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LoginPolzov.Text == null || PassPolzov.Text == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Регистрация] ([Логин], [Пароль]) VALUES (@Логин, @Пароль)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@Логин", LoginPolzov.Text);
                    command.Parameters.AddWithValue("@Пароль", PassPolzov.Text);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowPolzov();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchClickPolzov(object sender, RoutedEventArgs e)
        {
            var login = LoginPolzov.Text;
            var pass = PassPolzov.Text;

            if (originalListPolzov == null)
            {
                originalListPolzov = dbEnt.Регистрация.ToList();
            }
            IEnumerable<Регистрация> result = originalListPolzov;
            if (login != null)
            {
                result = result.Where(d => d.Логин.ToLower().Contains(login.ToLower()));
            }
            if (pass != null)
            {
                result = result.Where(d => d.Пароль.ToLower().Contains(pass.ToLower()));
            }
            регистрацияDataGridPolzov.ItemsSource = result.ToList();
        }
        private async void EditClickPolzov(object sender, RoutedEventArgs e)
        {
            var selectedRow = регистрацияDataGridPolzov.SelectedItem as Регистрация;
            try
            {
                selectedRow.Логин = LoginPolzov.Text;
                selectedRow.Пароль = PassPolzov.Text;
                dbEnt.SaveChanges();
                await ResetWindowPolzov();

                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ClearClickPolzov(object sender, EventArgs e)
        {
            LoginPolzov.Text = "";
            PassPolzov.Text = "";
            await LoadDataPolzov();
        }
        //Posesh
        private List<Посещения> originalListPosesh;
        private async Task LoadDataPosesh()
        {
            var КурсыIds = await dbEnt.Пользователи_Курсы.ToListAsync();
            var Посещения = await dbEnt.Посещения.ToListAsync();
            var назвКурсы = await dbEnt.Курсы.ToListAsync();
            var фамилУч = await dbEnt.Учащиеся.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                NameCursPosesh.ItemsSource = назвКурсы;
                FamilPosesh.ItemsSource = фамилУч;
                NameCursPosesh.DisplayMemberPath = "Название";
                NameCursPosesh.SelectedValuePath = "ID_Курса";
                FamilPosesh.DisplayMemberPath = "Фамилия";
                FamilPosesh.SelectedValuePath = "ID_Учащегося";
                посещенияDataGridPosesh.ItemsSource = Посещения;
                originalListPosesh = Посещения;
            });
        }
        private async Task ResetWindowPosesh()
        {
            InfoEditPanelPosesh.Visibility = Visibility.Hidden;
            InfoEditLabelPosesh.Visibility = Visibility.Hidden;
            EditPosesh.Visibility = Visibility.Hidden;
            AddPosesh.Visibility = Visibility.Hidden;
            CancelPosesh.Visibility = Visibility.Hidden;
            DelToPosesh.Visibility = Visibility.Visible;
            EditToPosesh.Visibility = Visibility.Visible;
            AddToPosesh.Visibility = Visibility.Visible;
            PowerSearchPosesh.Visibility = Visibility.Visible;
            ClearPosesh.Visibility = Visibility.Hidden;
            SearchTextBoxPosesh.Visibility = Visibility.Visible;
            SearchLabelPosesh.Visibility = Visibility.Visible;
            Thickness currentMargin = посещенияDataGridPosesh.Margin;
            посещенияDataGridPosesh.Margin = new Thickness(10, 10, 10, 84);
            PrintPosesh.Margin = new Thickness(284, 523, 0, 0);
            FamilPosesh.SelectedIndex = -1;
            NameCursPosesh.SelectedIndex = -1;
            DatePosesh.SelectedDate = null;
            StatusPosesh.Text = null;
            await LoadDataPosesh();
        }
        private void HidenVisibleMainButtonsPosesh()
        {
            Thickness currentMargin = посещенияDataGridPosesh.Margin;
            посещенияDataGridPosesh.Margin = new Thickness(396, 10, 10, 84);
            PrintPosesh.Margin = new Thickness(396, 523, 0, 0);
            EditToPosesh.Visibility = Visibility.Hidden;
            DelToPosesh.Visibility = Visibility.Hidden;
            AddToPosesh.Visibility = Visibility.Hidden;
            PowerSearchPosesh.Visibility = Visibility.Hidden;
            InfoEditPanelPosesh.Visibility = Visibility.Visible;
            InfoEditLabelPosesh.Visibility = Visibility.Visible;
            CancelPosesh.Visibility = Visibility.Visible;
            FamilPosesh.SelectedIndex = -1;
            NameCursPosesh.SelectedIndex = -1;
            DatePosesh.SelectedDate = null;
            StatusPosesh.Text = null;
            SearchTextBoxPosesh.Text = "";
        }
        private async void CancelClickPosesh(object sender, RoutedEventArgs e)
        {
            await ResetWindowPosesh();
        }
        private async void EditClickPosesh(object sender, RoutedEventArgs e)
        {
            var selectedRow = посещенияDataGridPosesh.SelectedItem as Посещения;
            try
            {
                selectedRow.ID_Учащегося = (int)FamilPosesh.SelectedValue;
                selectedRow.ID_Курса = (int)NameCursPosesh.SelectedValue;
                selectedRow.Дата_посещения = DatePosesh.SelectedDate;
                selectedRow.Статус = StatusPosesh.Text;
                dbEnt.SaveChanges();
                await ResetWindowPosesh();
                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditToClickPosesh(object sender, RoutedEventArgs e)
        {
            var selectedRow = посещенияDataGridPosesh.SelectedItem as Посещения;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsPosesh();
                EditPosesh.Visibility = Visibility.Visible;
                FamilPosesh.SelectedValue = selectedRow.ID_Учащегося;
                NameCursPosesh.SelectedValue = selectedRow.ID_Курса;
                DatePosesh.SelectedDate = selectedRow.Дата_посещения;
                StatusPosesh.Text = selectedRow.Статус;
                PowerSearchPosesh.Visibility = Visibility.Hidden;
            }
        }
        private void AddToClickPosesh(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsPosesh();
            AddPosesh.Visibility = Visibility.Visible;
        }
        private void DeleteClickPosesh(object sender, RoutedEventArgs e)
        {
            var selectedRows = посещенияDataGridPosesh.SelectedItems.Cast<Посещения>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Посещения, LoadDataPosesh);
        }
        private async void AddClickPosesh(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FamilPosesh.SelectedValue == null
                    || NameCursPosesh.SelectedValue == null
                    || DatePosesh.SelectedDate == null
                    || StatusPosesh.Text == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string query = "INSERT INTO [dbo].[Посещения] ([ID_Учащегося], [ID_Курса], [Дата_посещения], [Статус]) VALUES (@ID_Учащегося, @ID_Курса, @Дата_посещения, @Статус)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@ID_Учащегося", FamilPosesh.SelectedValue);
                    command.Parameters.AddWithValue("@ID_Курса", NameCursPosesh.SelectedValue);
                    command.Parameters.AddWithValue("@Дата_посещения", DatePosesh.SelectedDate);
                    command.Parameters.AddWithValue("@Статус", StatusPosesh.Text);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowPosesh();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchTextBoxTextChangedPosesh(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxPosesh.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                посещенияDataGridPosesh.ItemsSource = originalListPosesh;
                return;
            }
            var Result = originalListPosesh.Where(d =>
                d.Учащиеся.Фамилия.ToLower().Contains(Search.ToLower())
                || d.Курсы.Название.ToLower().Contains(Search.ToLower())
                || (d.Дата_посещения.HasValue && d.Дата_посещения.Value.ToString("d").ToLower().Contains(Search.ToLower()))
                || d.Статус.ToLower().Contains(Search.ToLower()));
            посещенияDataGridPosesh.ItemsSource = Result.ToList();
        }
        private void PrintClickPosesh(object sender, RoutedEventArgs e)
        {
            var dt = new DataTable("Grid");
            dt.Columns.Add("Фамилия учащегося");
            dt.Columns.Add("Название курса");
            dt.Columns.Add("Дата занятия");
            dt.Columns.Add("Статус");
            foreach (Посещения row in посещенияDataGridPosesh.Items)
            {
                DataRow newRow = dt.NewRow();
                newRow[0] = row.Учащиеся.Фамилия;
                newRow[1] = row.Курсы.Название;
                newRow[2] = row.Дата_посещения.HasValue ? row.Дата_посещения.Value.ToString("d") : string.Empty;
                newRow[3] = row.Статус;
                dt.Rows.Add(newRow);
            }
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "Посещения.xlsx"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        wb.SaveAs(saveFileDialog.FileName);
                    }
                }
                catch (IOException)
                {
                    MessageBox.Show("Файл занят, закройте файл и попробуйте еще раз");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void PowerSearchPoseshClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsPosesh();
            SearchTextBoxPosesh.Visibility = Visibility.Hidden;
            SearchLabelPosesh.Visibility = Visibility.Hidden;
            SearchPosesh.Visibility = Visibility.Visible;
            ClearPosesh.Visibility = Visibility.Visible;
        }
        private async void SearchClickPosesh(object sender, RoutedEventArgs e)
        {
            var famil = FamilPosesh.SelectedValue;
            var namecurs = NameCursPosesh.SelectedValue;
            var dateposesh = DatePosesh.SelectedDate;
            var statusposesh = StatusPosesh.Text;
            if (originalListPosesh == null)
            {
                await LoadDataPosesh();
            }
            IEnumerable<Посещения> result = originalListPosesh;
            if (famil != null)
            {
                result = result.Where(d => d.Учащиеся.ID_Учащегося == (int)famil);
            }
            if (namecurs != null)
            {
                result = result.Where(d => d.Курсы.ID_Курса == (int)namecurs);
            }
            if (dateposesh.HasValue)
            {
                result = result.Where(d => d.Дата_посещения.HasValue && d.Дата_посещения.Value.Date == dateposesh.Value.Date);
            }
            if (statusposesh != null)
            {
                result = result.Where(d => d.Статус.ToString().ToLower() == statusposesh.ToString().ToLower());
            }
            посещенияDataGridPosesh.ItemsSource = result.ToList();
        }
        private async void ClearClickPosesh(object sender, EventArgs e)
        {
            FamilPosesh.SelectedIndex = -1;
            NameCursPosesh.SelectedIndex = -1;
            DatePosesh.SelectedDate = null;
            StatusPosesh.Text = null;
            await LoadDataPosesh();
        }
        //UchKursi
        private List<Курсы_Учащихся> originalListUchKursi;
        private async Task LoadDataUchKursi()
        {
            var UchCourses = await dbEnt.Курсы_Учащихся.ToListAsync();
            var courses = await dbEnt.Курсы.ToListAsync();
            var uch = await dbEnt.Учащиеся.ToListAsync();
            Dispatcher.Invoke(() =>
            {
                учащиесякурсыDataGridUchKursi.ItemsSource = UchCourses;
                CourseUchKursi.ItemsSource = courses;
                NameUchUchKursi.ItemsSource = uch;
                CourseUchKursi.DisplayMemberPath = "Название";
                CourseUchKursi.SelectedValuePath = "ID_Курса";
                NameUchUchKursi.DisplayMemberPath = "Фамилия";
                NameUchUchKursi.SelectedValuePath = "ID_Учащегося";
            });
        }
        private void HidenVisibleMainButtonsUchKursi()
        {
            Thickness currentMargin = учащиесякурсыDataGridUchKursi.Margin;
            учащиесякурсыDataGridUchKursi.Margin = new Thickness(396, 10, 10, 84);
            PrintUchKursi.Margin = new Thickness(396, 523, 0, 0);
            EditToUchKursi.Visibility = Visibility.Hidden;
            DelToUchKursi.Visibility = Visibility.Hidden;
            AddToUchKursi.Visibility = Visibility.Hidden;
            PowerSearchUchKursi.Visibility = Visibility.Hidden;
            InfoEditPanelUchKursi.Visibility = Visibility.Visible;
            InfoEditLabelUchKursi.Visibility = Visibility.Visible;
            CancelUchKursi.Visibility = Visibility.Visible;
            NameUchUchKursi.Text = "";
            CourseUchKursi.Text = "";
        }
        private async Task ResetWindowUchKursi()
        {
            InfoEditPanelUchKursi.Visibility = Visibility.Hidden;
            InfoEditLabelUchKursi.Visibility = Visibility.Hidden;
            EditUchKursi.Visibility = Visibility.Hidden;
            AddUchKursi.Visibility = Visibility.Hidden;
            ClearUchKursi.Visibility = Visibility.Hidden;
            CancelUchKursi.Visibility = Visibility.Hidden;
            SearchUchKursi.Visibility = Visibility.Hidden;
            ClearUchKursi.Visibility = Visibility.Hidden;
            DelToUchKursi.Visibility = Visibility.Visible;
            EditToUchKursi.Visibility = Visibility.Visible;
            AddToUchKursi.Visibility = Visibility.Visible;
            PowerSearchUchKursi.Visibility = Visibility.Visible;
            SearchTextBoxUchKursi.Visibility = Visibility.Visible;
            SearchLabelUchKursi.Visibility = Visibility.Visible;
            Thickness currentMargin = учащиесякурсыDataGridUchKursi.Margin;
            учащиесякурсыDataGridUchKursi.Margin = new Thickness(10, 10, 10, 84);
            PrintUchKursi.Margin = new Thickness(284, 523, 0, 0);
            NameUchUchKursi.Text = "";
            CourseUchKursi.Text = "";
            SearchTextBoxUchKursi.Text = "";
            await LoadDataUchKursi();
        }
        private async void DeleteClickUchKursi(object sender, RoutedEventArgs e)
        {
            var selectedRows = учащиесякурсыDataGridUchKursi.SelectedItems.Cast<object>().ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("Чтобы удалить - выберите строку!");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранные строки?", "Удаление", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                bool success = true;
                foreach (var row in selectedRows)
                {
                    var id = (int)row.GetType().GetProperty("ID_Курса_Учащегося").GetValue(row, null);
                    var userCourseToDelete = dbEnt.Курсы_Учащихся.FirstOrDefault(p => p.ID_Курса_Учащегося == id);
                    if (userCourseToDelete != null)
                    {
                        dbEnt.Курсы_Учащихся.Remove(userCourseToDelete);
                    }
                    else
                    {
                        success = false;
                        MessageBox.Show($"Не удалось найти запись с ID {id} в базе данных.", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                dbEnt.SaveChanges();
                await LoadDataUchKursi();
                if (success)
                {
                    MessageBox.Show("Данные успешно удалены.", "Успешное удаление данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void EditToClickUchKursi(object sender, RoutedEventArgs e)
        {
            var selectedRow = учащиесякурсыDataGridUchKursi.SelectedItem as Курсы_Учащихся;
            if (selectedRow == null)
            {
                MessageBox.Show("Чтобы изменить - выберите строку!");
                return;
            }
            else
            {
                HidenVisibleMainButtonsUchKursi();
                EditUchKursi.Visibility = Visibility.Visible;
                NameUchUchKursi.SelectedValue = selectedRow.ID_Учащегося;
                CourseUchKursi.SelectedValue = selectedRow.ID_Курса;
            }
        }
        private void AddToClickUchKursi(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsUchKursi();
            AddUchKursi.Visibility = Visibility.Visible;
        }
        private void PrintClickUchKursi(object sender, RoutedEventArgs e)
        {
            ExportDataGridToExcel(учащиесякурсыDataGridUchKursi, "Курсы учащихся");
        }
        private void PowerSearchUchKursiClick(object sender, RoutedEventArgs e)
        {
            HidenVisibleMainButtonsUchKursi();
            SearchTextBoxUchKursi.Visibility = Visibility.Hidden;
            SearchLabelUchKursi.Visibility = Visibility.Hidden;
            SearchUchKursi.Visibility = Visibility.Visible;
            ClearUchKursi.Visibility = Visibility.Visible;
        }
        private void SearchTextBoxTextChangedUchKursi(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxUchKursi.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                учащиесякурсыDataGridUchKursi.ItemsSource = originalListUchKursi;
                return;
            }
            var Result = originalListUchKursi.Where(d =>
                d.Курсы.Название.ToLower().Contains(Search.ToLower())
                || d.Учащиеся.Имя.ToLower().Contains(Search.ToLower())
                || d.Учащиеся.Фамилия.ToLower().Contains(Search.ToLower())
                || d.Учащиеся.Отчество.ToLower().Contains(Search.ToLower()));
            учащиесякурсыDataGridUchKursi.ItemsSource = Result.ToList();
        }
        private async void CancelClickUchKursi(object sender, RoutedEventArgs e)
        {
            await ResetWindowUchKursi();
        }
        private async void AddClickUchKursi(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NameUchUchKursi.SelectedValue == null || CourseUchKursi.SelectedValue == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                db.openConnection();
                string checkQuery = "SELECT COUNT(*) FROM [dbo].[Курсы_Учащихся] WHERE [ID_Учащегося] = @ID_Учащегося AND [ID_Курса] = @ID_Курса";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, db.getConnection()))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Учащегося", NameUchUchKursi.SelectedValue);
                    checkCommand.Parameters.AddWithValue("@ID_Курса", CourseUchKursi.SelectedValue);
                    int existingCount = (int)checkCommand.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        MessageBox.Show("Такие данные уже существуют.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                        db.closeConnection();
                        return;
                    }
                }
                string query = "INSERT INTO [dbo].[Курсы_Учащихся] ([ID_Учащегося], [ID_Курса]) VALUES (@ID_Учащегося, @ID_Курса)";
                using (SqlCommand command = new SqlCommand(query, db.getConnection()))
                {
                    command.Parameters.AddWithValue("@ID_Учащегося", NameUchUchKursi.SelectedValue);
                    command.Parameters.AddWithValue("@ID_Курса", CourseUchKursi.SelectedValue);
                    command.ExecuteNonQuery();
                }
                db.closeConnection();
                await ResetWindowUchKursi();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SearchClickUchKursi(object sender, RoutedEventArgs e)
        {
            var fam = NameUchUchKursi.Text;
            var course = CourseUchKursi.Text;

            if (originalListUchKursi == null)
            {
                originalListUchKursi = dbEnt.Курсы_Учащихся.ToList();
            }
            IEnumerable<Курсы_Учащихся> result = originalListUchKursi;
            if (fam != null)
            {
                result = result.Where(d => d.Учащиеся.Фамилия.ToLower().Contains(fam.ToLower()));
            }
            if (course != null)
            {
                result = result.Where(d => d.Курсы.Название.ToLower().Contains(course.ToLower()));
            }
            учащиесякурсыDataGridUchKursi.ItemsSource = result.ToList();
        }
        private async void EditClickUchKursi(object sender, RoutedEventArgs e)
        {
            var selectedRow = учащиесякурсыDataGridUchKursi.SelectedItem as Курсы_Учащихся;
            try
            {
                selectedRow.ID_Учащегося = (int)NameUchUchKursi.SelectedValue;
                selectedRow.ID_Курса = (int)CourseUchKursi.SelectedValue;
                dbEnt.SaveChanges();
                await ResetWindowUchKursi();

                MessageBox.Show("Данные успешно обновлены.", "Успешное обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка обновления данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void ClearClickUchKursi(object sender, EventArgs e)
        {
            NameUchUchKursi.Text = "";
            CourseUchKursi.Text = "";
            await LoadDataUchKursi();
        }
    }
}