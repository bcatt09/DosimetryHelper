using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace VMS.TPS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        private readonly ViewModel _vm;

        public MainWindow(ViewModel viewModel)
        {
            _vm = viewModel;
            InitializeComponent();
        }

        private void ImportHelper_Click(object sender, RoutedEventArgs e)
        {
            _vm.ValidateImportHelper();
        }

        private void Structures_Click(object sender, RoutedEventArgs e)
        {
            _vm.ValidateStructureDeletion();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _vm.CloseWindow();
        }
    }

    public class InvalidConverterTextBlock : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvalidConverterButton : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
