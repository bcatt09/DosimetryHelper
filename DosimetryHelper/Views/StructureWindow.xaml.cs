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
    public partial class StructureWindow : MetroWindow
    {
        private readonly StructureDeletionViewModel _vm;

        public StructureWindow(StructureDeletionViewModel viewModel, Window owner)
        {
            _vm = viewModel;
            DataContext = viewModel;
            SizeToContent = SizeToContent.WidthAndHeight;
            Title = $"Structure Deletion";
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            ShowDialog();
        }

        private void Finalize_Click(object sender, RoutedEventArgs e)
        {
            if(_vm.StructureDeletionPerformUpdates())
                Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RemoveFocus(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Keyboard.ClearFocus();  // Remove focus from TextBoxes to prevent crashing Eclipse
            System.Threading.Thread.Sleep(1000);  // And then pause for a bit to let it sink in?
        }
    }
}
