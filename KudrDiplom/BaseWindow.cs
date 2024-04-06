using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace KudrDiplom
{
    public class BaseWindow : Window
    {
        public BaseWindow()
        {
            Loaded += BaseWindow_Loaded;
        }

        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Установка начального значения Width для элемента Separator
            // Separator.Width = 0;

            // Запуск анимации SeparatorAnimation
            Storyboard sb = (Storyboard)FindResource("SeparatorAnimation");
            sb.Begin();
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void BackTo(object sender, RoutedEventArgs e)
        {

        }
    }
}
