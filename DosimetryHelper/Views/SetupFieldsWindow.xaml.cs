using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DosimetryHelper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SetupFieldsWindow : MetroWindow
	{
        private readonly SetupFieldsViewModel _vm;

		public SetupFieldsWindow(SetupFieldsViewModel viewModel, Window owner)
        {
            _vm = viewModel;
            DataContext = viewModel;
            SizeToContent = SizeToContent.WidthAndHeight;
            Title = $"Add Setup Fields";
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            ShowDialog();
        }
        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            var cb = e.Source as CheckBox;
            if (!cb.IsChecked.HasValue)
                cb.IsChecked = false;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
