using MahApps.Metro.Controls;
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

namespace DosimetryHelper
{
    /// <summary>
    /// Interaction logic for StructureWindow.xaml
    /// </summary>
    public partial class ImageRenamingWindow : MetroWindow
    {
        private readonly ImageRenamingViewModel _vm;

        public ImageRenamingWindow(ImageRenamingViewModel viewModel, Window owner)
        {
            _vm = viewModel;
            DataContext = viewModel;
            SizeToContent = SizeToContent.WidthAndHeight;
            Title = $"Rename External Imaging";
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            ShowDialog();
        }

        private void Finalize_Click(object sender, RoutedEventArgs e)
        {
            if(_vm.ImageRenamingPerformUpdates())
                Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
