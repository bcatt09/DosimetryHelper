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
	public partial class ImportWindow : Window
	{
        private readonly ImportWorkflowViewModel _vm;

		public ImportWindow(ImportWorkflowViewModel viewModel, Window owner)
        {
            _vm = viewModel;
            DataContext = viewModel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            SizeToContent = SizeToContent.WidthAndHeight;
            Title = $"Import Workflow";
            Owner = owner;
			InitializeComponent();
            ShowDialog();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Finalize_Click(object sender, RoutedEventArgs e)
        {
            //_vm.ImportWorkflowPerformUpdates();
            _vm.ImportWorkflowPerformUpdatesDebug();

            Close();
        }

        private void CoursePlanInput_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox.Foreground == Brushes.Gray)
                textbox.Foreground = Brushes.Black;

            if (textbox.Text == "Enter Dataset Name")
            {
                textbox.Text = DateTime.Now.ToString("yyyyMMdd") + " ";
            }
            //if course ID is being changed, add a new course to the list of available to be selected
            else if (textbox.Text == "Enter Course ID")
            {
                _vm.Courses.ToList().Insert(0, "");
                textbox.Text = _vm.ImportWorkflowGetNewCourseName();
            }
            else if (textbox.Text == "Enter Plan ID")
                textbox.Text = "";
        }
    }
}
