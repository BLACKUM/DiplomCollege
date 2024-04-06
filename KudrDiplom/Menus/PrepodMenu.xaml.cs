﻿using KudrDiplom.Auth;
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
using System.Data.Entity;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace KudrDiplom.Menus
{
    /// <summary>
    /// Логика взаимодействия для PrepodMenu.xaml
    /// </summary>
    public partial class PrepodMenu : Window
    {
        Entities dbEnt;
        DataBase db = new DataBase();
        private int currentUserId;
        public PrepodMenu(string loginUser, int userId)
        {
            InitializeComponent();
            LoginInfo.Content = $"Добро пожаловать,{loginUser}";
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
                dbEnt.SaveChanges();
                await updateMethod();
                MessageBox.Show("Данные успешно удалены.", "Успешное удаление данных", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private async Task ReloadTables()
        {
            await LoadDataRaspisanie();
            await LoadDataKursi();
            await LoadDataUch();
            await LoadDataItog();
            await LoadDataPosesh();
        }
        //Raspisanie
        private List<Расписание> originalListRaspisanie;
        private async Task LoadDataRaspisanie()
        {
            var доступныеКурсыIds = await dbEnt.Пользователи_Курсы
                .Where(pc => pc.ID_Пользователя == currentUserId)
                .Select(pc => pc.ID_Курса)
                .ToListAsync();
            var расписание = await dbEnt.Расписание
                .Where(r => доступныеКурсыIds.Contains(r.ID_Курса))
                .ToListAsync();
            var назвКурса = await dbEnt.Курсы
                    .Where(к => доступныеКурсыIds.Contains(к.ID_Курса))
                    .ToListAsync();
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
            var доступныеКурсыIds = await dbEnt.Пользователи_Курсы
                .Where(pc => pc.ID_Пользователя == currentUserId)
                .Select(pc => pc.ID_Курса)
                .ToListAsync();
            var курсы = await dbEnt.Курсы
                .Where(к => доступныеКурсыIds.Contains(к.ID_Курса))
                .ToListAsync();
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
        private async void ResetWindowKursi()
        {
            InfoEditPanelKursi.Visibility = Visibility.Hidden;
            InfoEditLabelKursi.Visibility = Visibility.Hidden;
            ClearKursi.Visibility = Visibility.Hidden;
            CancelKursi.Visibility = Visibility.Hidden;
            SearchKursi.Visibility = Visibility.Hidden;
            ClearKursi.Visibility = Visibility.Hidden;
            PowerSearchKursi.Visibility = Visibility.Visible;
            SearchTextBoxKursi.Visibility = Visibility.Visible;
            SearchLabelKursi.Visibility = Visibility.Visible;
            Thickness currentMargin = курсыDataGridKursi.Margin;
            курсыDataGridKursi.Margin = new Thickness(10, 10, 10, 84);
            PrintKursi.Margin = new Thickness(1019, 523, 0, 0);
            NameKursKursi.Text = "";
            OpisanieKursi.Text = "";
            DatestartKursi.SelectedDate = null;
            DateendKursi.SelectedDate = null;
            await LoadDataKursi();
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
        private void CancelClickKursi(object sender, RoutedEventArgs e)
        {
            ResetWindowKursi();
        }
        private async void SearchClickKursi(object sender, RoutedEventArgs e)
        {
            var name = NameKursKursi.Text;
            var opisanie = OpisanieKursi.Text;
            var dateStart = DatestartKursi.SelectedDate;
            var dateEnd = DateendKursi.SelectedDate;

            if (originalListKursi == null)
            {
                await LoadDataKursi();
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
            var доступныеКурсыIds = await dbEnt.Пользователи_Курсы
                .Where(pc => pc.ID_Пользователя == currentUserId)
                .Select(pc => pc.ID_Курса)
                .ToListAsync();
            var доступныеУчащиесяIds = await dbEnt.Курсы_Учащихся
                .Where(ку => доступныеКурсыIds.Contains(ку.ID_Курса))
                .Select(ку => ку.ID_Учащегося)
                .ToListAsync();
            var учащиеся = await dbEnt.Учащиеся
                .Where(у => доступныеУчащиесяIds.Contains(у.ID_Учащегося))
                .ToListAsync();
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
        private async void ResetWindowUch()
        {
            InfoEditPanelUch.Visibility = Visibility.Hidden;
            InfoEditLabelUch.Visibility = Visibility.Hidden;
            ClearUch.Visibility = Visibility.Hidden;
            CancelUch.Visibility = Visibility.Hidden;
            SearchUch.Visibility = Visibility.Hidden;
            ClearUch.Visibility = Visibility.Hidden;
            PowerSearchUch.Visibility = Visibility.Visible;
            SearchTextBoxUch.Visibility = Visibility.Visible;
            SearchLabelUch.Visibility = Visibility.Visible;
            Thickness currentMargin = учащиесяDataGridUch.Margin;
            учащиесяDataGridUch.Margin = new Thickness(10, 10, 10, 84);
            PrintUch.Margin = new Thickness(1019, 523, 0, 0);
            NameUch.Text = "";
            SurnameUch.Text = "";
            OtchUch.Text = "";
            DateOfBirthUch.SelectedDate = null;
            GenderUch.SelectedIndex = -1;
            await LoadDataUch();
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
        private async void SearchTextBoxTextChangedUch(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxUch.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                await LoadDataUch();
            }
            var Result = originalListUch.Where(d =>
                d.Имя.ToLower().Contains(Search.ToLower())
                || d.Фамилия.ToLower().Contains(Search.ToLower())
                || d.Отчество.ToLower().Contains(Search.ToLower())
                || (d.Дата_рождения.HasValue && d.Дата_рождения.Value.ToString("d").ToLower().Contains(Search.ToLower()))
                || d.Пол.ToLower().Contains(Search.ToLower()));
            учащиесяDataGridUch.ItemsSource = Result.ToList();
        }
        private void CancelClickUch(object sender, RoutedEventArgs e)
        {
            ResetWindowUch();
        }
        private async void SearchClickUch(object sender, RoutedEventArgs e)
        {
            var name = NameUch.Text;
            var familia = SurnameUch.Text;
            var otch = OtchUch.Text;
            var datarosjdenia = DateOfBirthUch.SelectedDate;
            var pol = GenderUch.SelectedItem as ComboBoxItem;

            if (originalListUch == null)
            {
                await LoadDataUch();
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
        private async void ClearClickUch(object sender, EventArgs e)
        {
            NameUch.Text = "";
            SurnameUch.Text = "";
            OtchUch.Text = "";
            DateOfBirthUch.SelectedDate = null;
            GenderUch.SelectedIndex = -1;
            await LoadDataUch();
        }
        //Itog
        private List<Итоговые_Работы> originalListItog;
        private async Task LoadDataItog()
        {
            var доступныеКурсыIds = await dbEnt.Пользователи_Курсы
                .Where(pc => pc.ID_Пользователя == currentUserId)
                .Select(pc => pc.ID_Курса)
                .ToListAsync();
            var доступныеУчащиесяIds = await dbEnt.Курсы_Учащихся
                .Where(ку => доступныеКурсыIds.Contains(ку.ID_Курса))
                .Select(ку => ку.ID_Учащегося)
                .ToListAsync();
            var Итоговые_Работы = await dbEnt.Итоговые_Работы
                .Where(r => доступныеУчащиесяIds.Contains(r.ID_Учащегося))
                .ToListAsync();
            var курсы = await dbEnt.Курсы
                .Where(к => доступныеКурсыIds.Contains(к.ID_Курса))
                .ToListAsync();
            var учащиеся = await dbEnt.Учащиеся
                .Where(к => доступныеУчащиесяIds.Contains(к.ID_Учащегося))
                .ToListAsync();
            Dispatcher.Invoke(() =>
            {
                NameCursItog.ItemsSource = курсы;
                NameCursItog.DisplayMemberPath = "Название";
                NameCursItog.SelectedValuePath = "ID_Курса";
                FamilItog.ItemsSource = учащиеся;
                FamilItog.DisplayMemberPath = "Фамилия";
                FamilItog.SelectedValuePath = "ID_Учащегося";
                ИтоговыеРаботыDataGridItog.ItemsSource = Итоговые_Работы;
                originalListItog = Итоговые_Работы;
            });
        }
        private async void ResetWindowItog()
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
        private void CancelClickItog(object sender, RoutedEventArgs e)
        {
            ResetWindowItog();
        }
        private void EditClickItog(object sender, RoutedEventArgs e)
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
                ResetWindowItog();
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
        private void AddClickItog(object sender, RoutedEventArgs e)
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
                ResetWindowItog();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void SearchTextBoxTextChangedItog(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxItog.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                await LoadDataItog();
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
        //Posesh
        private List<Посещения> originalListPosesh;
        private async Task LoadDataPosesh()
        {
            var доступныеКурсыIds = await dbEnt.Пользователи_Курсы
                .Where(pc => pc.ID_Пользователя == currentUserId)
                .Select(pc => pc.ID_Курса)
                .ToListAsync();
            var доступныеУчащиесяIds = await dbEnt.Курсы_Учащихся
                .Where(ку => доступныеКурсыIds.Contains(ку.ID_Курса))
                .Select(ку => ку.ID_Учащегося)
                .ToListAsync();
            var доступныеПосещения = await dbEnt.Посещения
                .Where(p => доступныеУчащиесяIds.Contains(p.ID_Учащегося))
                .ToListAsync();
            var курсы = await dbEnt.Курсы
                .Where(к => доступныеКурсыIds.Contains(к.ID_Курса))
                .ToListAsync();
            var учащиеся = await dbEnt.Учащиеся
                .Where(к => доступныеУчащиесяIds.Contains(к.ID_Учащегося))
                .ToListAsync();
            Dispatcher.Invoke(() =>
            {
                посещенияDataGridPosesh.ItemsSource = доступныеПосещения;
                NameCursPosesh.ItemsSource = курсы;
                NameCursPosesh.DisplayMemberPath = "Название";
                NameCursPosesh.SelectedValuePath = "ID_Курса";
                FamilPosesh.ItemsSource = учащиеся;
                FamilPosesh.DisplayMemberPath = "Фамилия";
                FamilPosesh.SelectedValuePath = "ID_Учащегося";
                originalListPosesh = доступныеПосещения;
            });
        }
        private async void ResetWindowPosesh()
        {
            InfoEditPanelPosesh.Visibility = Visibility.Hidden;
            InfoEditLabelPosesh.Visibility = Visibility.Hidden;
            EditPosesh.Visibility = Visibility.Hidden;
            AddPosesh.Visibility = Visibility.Hidden;
            CancelPosesh.Visibility = Visibility.Hidden;
            AddAdvPosesh.Visibility = Visibility.Hidden;
            NextAdvPosesh.Visibility = Visibility.Hidden;
            PrevAdvPosesh.Visibility = Visibility.Hidden;
            AddAdvDniPosesh.Visibility = Visibility.Hidden;
            DonePosesh.Visibility = Visibility.Hidden;
            SearchPosesh.Visibility = Visibility.Hidden;
            ClearPosesh.Visibility = Visibility.Hidden;
            DelToPosesh.Visibility = Visibility.Visible;
            EditToPosesh.Visibility = Visibility.Visible;
            AddToPosesh.Visibility = Visibility.Visible;
            PowerSearchPosesh.Visibility = Visibility.Visible;
            SearchTextBoxPosesh.Visibility = Visibility.Visible;
            SearchLabelPosesh.Visibility = Visibility.Visible;
            Thickness currentMargin = посещенияDataGridPosesh.Margin;
            посещенияDataGridPosesh.Margin = new Thickness(10, 10, 10, 84);
            PrintPosesh.Margin = new Thickness(284, 523, 0, 0);
            FamilPosesh.SelectedIndex = -1;
            NameCursPosesh.SelectedIndex = -1;
            DatePosesh.SelectedDate = null;
            StatusPosesh.Text = null;
            DatePosesh.IsEnabled = true;
            FamilPosesh.IsEnabled = true;
            NameCursPosesh.IsEnabled = true;
            StatusPosesh.IsEnabled = true;
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
        private void CancelClickPosesh(object sender, RoutedEventArgs e)
        {
            ResetWindowPosesh();
        }
        private void EditClickPosesh(object sender, RoutedEventArgs e)
        {
            var selectedRow = посещенияDataGridPosesh.SelectedItem as Посещения;
            try
            {
                selectedRow.ID_Учащегося = (int)FamilPosesh.SelectedValue;
                selectedRow.ID_Курса = (int)NameCursPosesh.SelectedValue;
                selectedRow.Дата_посещения = DatePosesh.SelectedDate;
                selectedRow.Статус = StatusPosesh.Text;
                dbEnt.SaveChanges();
                ResetWindowPosesh();
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
            AddAdvPosesh.Visibility = Visibility.Visible;
        }
        private void DeleteClickPosesh(object sender, RoutedEventArgs e)
        {
            var selectedRows = посещенияDataGridPosesh.SelectedItems.Cast<Посещения>().ToList();
            DeleteSelectedRows(selectedRows, dbEnt.Посещения, LoadDataPosesh);
        }
        private void AddClickPosesh(object sender, RoutedEventArgs e)
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
                ResetWindowPosesh();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void SearchTextBoxTextChangedPosesh(object sender, TextChangedEventArgs e)
        {
            var Search = SearchTextBoxPosesh.Text;
            if (string.IsNullOrWhiteSpace(Search))
            {
                await LoadDataPosesh();
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
        private void AddAdvClickPosesh(object sender, RoutedEventArgs e)
        {
            FamilPosesh.IsEnabled = false;
            NameCursPosesh.IsEnabled = false;
            StatusPosesh.IsEnabled = false;
            FamilPosesh.SelectedIndex = -1;
            NameCursPosesh.SelectedIndex = -1;
            DatePosesh.SelectedDate = null;
            StatusPosesh.Text = null;
            DonePosesh.Visibility = Visibility.Visible;
            AddPosesh.Visibility = Visibility.Hidden;
            AddAdvPosesh.Visibility = Visibility.Hidden;
            MessageBox.Show("Выберите дату");
        }
        private async void AddAdvDniClickPosesh(object sender, RoutedEventArgs e)
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
                string checkQuery = "SELECT * FROM [dbo].[Посещения] WHERE [ID_Учащегося] = @ID_Учащегося AND [ID_Курса] = @ID_Курса AND [Дата_посещения] = @Дата_посещения AND [Статус] = @Статус";
                using (SqlCommand checkCommand = new SqlCommand(checkQuery, db.getConnection()))
                {
                    checkCommand.Parameters.AddWithValue("@ID_Учащегося", FamilPosesh.SelectedValue);
                    checkCommand.Parameters.AddWithValue("@ID_Курса", NameCursPosesh.SelectedValue);
                    checkCommand.Parameters.AddWithValue("@Дата_посещения", DatePosesh.SelectedDate);
                    checkCommand.Parameters.AddWithValue("@Статус", StatusPosesh.Text);
                    using (SqlDataReader reader = checkCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            MessageBox.Show("Данные уже существуют.", "Ошибка добавления данных", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                string insertQuery = "INSERT INTO [dbo].[Посещения] ([ID_Учащегося], [ID_Курса], [Дата_посещения], [Статус]) VALUES (@ID_Учащегося, @ID_Курса, @Дата_посещения, @Статус)";
                using (SqlCommand insertCommand = new SqlCommand(insertQuery, db.getConnection()))
                {
                    insertCommand.Parameters.AddWithValue("@ID_Учащегося", FamilPosesh.SelectedValue);
                    insertCommand.Parameters.AddWithValue("@ID_Курса", NameCursPosesh.SelectedValue);
                    insertCommand.Parameters.AddWithValue("@Дата_посещения", DatePosesh.SelectedDate);
                    insertCommand.Parameters.AddWithValue("@Статус", StatusPosesh.Text);
                    insertCommand.ExecuteNonQuery();
                }
                db.closeConnection();
                await LoadDataPosesh();
                MessageBox.Show("Данные успешно добавлены в базу данных.", "Успешное добавление данных", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    if (FamilPosesh.SelectedIndex < FamilPosesh.Items.Count - 1)
                    {
                        FamilPosesh.SelectedIndex += 1;
                    }
                    else
                    {
                        FamilPosesh.SelectedIndex = -1;
                        NameCursPosesh.SelectedIndex = -1;
                        DatePosesh.SelectedDate = null;
                        StatusPosesh.Text = null;
                        ResetWindowPosesh();
                        MessageBox.Show("Вы достигли конца списка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                    ResetWindowPosesh();
                }
            }
            catch (Exception ex)
            {
                db.closeConnection();
                MessageBox.Show("Ошибка при добавлении данных в базу данных: " + ex.Message, "Ошибка добавлении данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NextAdvClickPosesh(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FamilPosesh.SelectedIndex < FamilPosesh.Items.Count - 1)
                {
                    FamilPosesh.SelectedIndex += 1;
                }
                else
                {
                    MessageBox.Show("Вы достигли конца списка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
        private void PrevAdvClickPosesh(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FamilPosesh.SelectedIndex > 0)
                {
                    FamilPosesh.SelectedIndex -= 1;
                }
                else
                {
                    MessageBox.Show("Вы достигли начала списка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
        private void DoneClickPosesh(object sender, RoutedEventArgs e)
        {
            DatePosesh.IsEnabled = false;
            StatusPosesh.IsEnabled = true;
            FamilPosesh.SelectedIndex = 0;
            NameCursPosesh.SelectedIndex = 0;
            DonePosesh.Visibility = Visibility.Hidden;
            AddAdvDniPosesh.Visibility = Visibility.Visible;
            NextAdvPosesh.Visibility = Visibility.Visible;
            PrevAdvPosesh.Visibility = Visibility.Visible;
        }
    }
}
