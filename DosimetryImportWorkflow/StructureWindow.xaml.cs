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
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    /// <summary>
    /// Interaction logic for StructureWindow.xaml
    /// </summary>
    public partial class StructureWindow : UserControl
    {
        private readonly ViewModel _vm;

        public StructureWindow(ViewModel viewModel)
        {
            _vm = viewModel;
            InitializeComponent();
        }

        private void Finalize_Click(object sender, RoutedEventArgs e)
        {
            _vm.StructureDeletionPerformUpdates();
            _vm.MainScreen();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _vm.MainScreen();
        }
    }

    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "Yes";
            else
                return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString().ToLower() == "yes")
                return true;
            else
                return false;
        }
    }

    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return "LimeGreen";
            else
                return "OrangeRed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StructureToIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Structure)value).Id;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
