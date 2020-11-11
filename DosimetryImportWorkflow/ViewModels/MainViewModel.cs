using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using VMS.TPS.Common.Model.API;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls.Dialogs;

namespace DosimetryHelper
{
	public class MainViewModel : ViewModelBase
    {
        // Properties
        private ScriptContext _context;
        private Window _window;

        // Commands
        public RelayCommand GoToImportWorkflowCommand
        {
            get;
            private set;
        }

        public RelayCommand GoToStructureDeletionCommand
        {
            get;
            private set;
        }

        // Constructor
        public MainViewModel(ScriptContext context, Window window)
        {
            _window = window;
            _context = context;

            GoToImportWorkflowCommand = new RelayCommand(ShowImportWorkflow, CanGoToImportWorkflow);
            GoToStructureDeletionCommand = new RelayCommand(ShowStructureDeletion, CanGoToStructureDeletion);
        }

        // Methods
        public bool CanGoToImportWorkflow()
        {
            return _context.Image != null;
        }

        public bool CanGoToStructureDeletion()
        {
            return _context.StructureSet != null;
        }

        public void ShowImportWorkflow()
        {
            ImportWorkflowViewModel importVM = new ImportWorkflowViewModel(_context);
            ImportWindow importWindow = new ImportWindow(importVM, _window);
        }

        public void ShowStructureDeletion()
        {
            StructureDeletionViewModel structDelVM = new StructureDeletionViewModel(_context);
            StructureWindow structDelWindow = new StructureWindow(structDelVM, _window);
        }

        public void CloseWindow()
        {
            _window.Close();
        }
	}
}
