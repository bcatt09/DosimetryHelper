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
	public partial class ImportWindow : MetroWindow
	{
        private readonly ImportWorkflowViewModel _vm;

		public ImportWindow(ImportWorkflowViewModel viewModel, Window owner)
        {
            _vm = viewModel;
            DataContext = viewModel;
            SizeToContent = SizeToContent.WidthAndHeight;
            Title = $"Import Workflow";
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            ShowDialog();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DatasetNameField_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            // Dataset name should by default start with the scan date
            if (textbox.Text == "")
                textbox.Text = DateTime.Now.ToString("yyyyMMdd") + " ";
        }

        private void CourseNameField_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
                
            // If course ID is being changed, add a new course to the list of available to be selected
            if (textbox.Text == "")
            {
                _vm.Courses.ToList().Insert(0, "");
                textbox.Text = _vm.ImportWorkflowGetNewCourseName();
            }
        }
    }
}
