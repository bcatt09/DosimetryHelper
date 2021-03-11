using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

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
        public RelayCommand GoToImageRenamingCommand
        {
            get;
            private set;
        }

        public RelayCommand GoToStructureDeletionCommand
        {
            get;
            private set;
        }

        public RelayCommand GoToAddSetupFieldsCommand
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
            GoToImageRenamingCommand = new RelayCommand(ShowImageRenaming, CanGoToImageRenaming);
            GoToStructureDeletionCommand = new RelayCommand(ShowStructureDeletion, CanGoToStructureDeletion);
            GoToAddSetupFieldsCommand = new RelayCommand(ShowAddSetupFields, CanGoToAddSetupFields);
        }

        // Methods
        public bool CanGoToImportWorkflow()
        {
            try
            {
                return _context.Image != null;
            }
            catch
            {
                return false;
            }
        }

        public bool CanGoToImageRenaming()
        {
            try
            {
                return _context.Patient.Studies.Count() > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool CanGoToStructureDeletion()
        {
            try
            {
                return _context.StructureSet != null;
            }
            catch
            {
                return false;
            }
        }

        public bool CanGoToAddSetupFields()
        {
            try
            {
                return _context.PlanSetup.Beams.Where(x => !x.IsSetupField).Count() > 0 &&
                       _context.PlanSetup.Beams.Select(x => x.IsocenterPosition).Distinct().Count() == 1;
            }
            catch
            {
                return false;
            }
        }

        public void ShowImportWorkflow()
        {
            ImportWorkflowViewModel importVM = new ImportWorkflowViewModel(_context);
            ImportWindow importWindow = new ImportWindow(importVM, _window);
        }

        public void ShowImageRenaming()
        {
            ImageRenamingViewModel imageRenameVM = new ImageRenamingViewModel(_context);
            ImageRenamingWindow imageRenameWindow = new ImageRenamingWindow(imageRenameVM, _window);
        }

        public void ShowStructureDeletion()
        {
            StructureDeletionViewModel structDelVM = new StructureDeletionViewModel(_context);
            StructureWindow structDelWindow = new StructureWindow(structDelVM, _window);
        }

        public void ShowAddSetupFields()
        {
            SetupFieldsViewModel structDelVM = new SetupFieldsViewModel(_context);
            SetupFieldsWindow setupFieldsWindow = new SetupFieldsWindow(structDelVM, _window);
        }

        public void CloseWindow()
        {
            _window.Close();
        }
	}
}
