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

namespace DosimetryHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        private readonly MainViewModel _vm;

        public MainWindow(MainViewModel viewModel)
        {
            _vm = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _vm.CloseWindow();
        }
    }
}
