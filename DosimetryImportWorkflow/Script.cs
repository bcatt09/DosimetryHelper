using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Reflection;
using System.Windows.Input;

[assembly: ESAPIScript(IsWriteable = true)]
[assembly: AssemblyVersion("1.0.0")]

namespace VMS.TPS
{
	public class Script
	{
		public Script()
		{
		}

		public void Execute(ScriptContext context, Window window)
		{
			window.Background = System.Windows.Media.Brushes.AliceBlue;
			window.KeyDown += (object sender, KeyEventArgs e) => { if (e.Key == Key.Escape) window.Close(); };
			window.Title = $"Dosimetry Helper - {context.Patient.Name}";
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

			ViewModel viewModel = new ViewModel(context, window);
			MainWindow userControl = new MainWindow(viewModel);

			window.Content = userControl;
			window.DataContext = viewModel;
			window.SizeToContent = SizeToContent.WidthAndHeight;
		}
	}
}

