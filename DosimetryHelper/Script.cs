using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Reflection;
using System.Windows.Input;

using DosimetryHelper;

[assembly: ESAPIScript(IsWriteable = true)]
[assembly: AssemblyVersion("1.5.0")]

namespace VMS.TPS
{
	public class Script
	{
		public Script()
		{
		}

		public void Execute(ScriptContext context, Window window)
		{
			try
			{
				window.KeyDown += (object sender, KeyEventArgs e) => { if (e.Key == Key.Escape) window.Close(); };
				window.Title = $"Dosimetry Helper - {context.Patient.Name}";
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				window.WindowStyle = WindowStyle.None;
				window.ResizeMode = ResizeMode.NoResize;

				MainViewModel viewModel = new MainViewModel(context, window);
				MainWindow userControl = new MainWindow(viewModel);

				window.Content = userControl;
				window.SizeToContent = SizeToContent.WidthAndHeight;
			}
			catch (Exception e)
            {
				MessageBox.Show($"{e.Message}\n\n{e.StackTrace}", "Well this is awkward...");
            }
		}
	}
}

