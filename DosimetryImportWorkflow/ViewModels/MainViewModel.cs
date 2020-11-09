using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using VMS.TPS.Common.Model.API;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace DosimetryHelper
{
	public class MainViewModel : ViewModelBase
    {
        private ScriptContext _context;
        private Regex _regEx;

        private Window _window;
        public Window Window
        {
            get { return _window; }
            set { Set(ref _window, value); }
        }

        private ImportWorkflowViewModel _importWorkflowViewModel;
        public ImportWorkflowViewModel ImportWorkflowViewModel
        {
            get { return _importWorkflowViewModel; }
            set { Set(ref _importWorkflowViewModel, value); }
        }

        private StructureDeletionViewModel _structureDeletionViewModel;
        public StructureDeletionViewModel StructureDeletionViewModel
        {
            get { return _structureDeletionViewModel; }
            set { Set(ref _structureDeletionViewModel, value); }
        }

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
            ImportWindow importWindow = new ImportWindow(importVM, Window);
        }

        public void ShowStructureDeletion()
        {
            StructureDeletionViewModel structDelVM = new StructureDeletionViewModel(_context);
            StructureWindow structDelWindow = new StructureWindow(structDelVM, Window);
        }

        public void MainScreen()
        {
            _window.Content = new MainWindow(this);
            _window.Title = $"Dosimetry Helper - {_context.Patient.Name}";
            _window.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void CloseWindow()
        {
            _window.Close();
        }
	}
}
