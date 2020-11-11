using GalaSoft.MvvmLight;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace DosimetryHelper
{
    public class StructureDeletionViewModel : ViewModelBase
    {
        private ScriptContext _context;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string _patientName;
        public string PatientName
        {
            get { return _patientName; }
            set { Set(ref _patientName, value); }
        }
        private List<StructureListItem> _structureList;
        public List<StructureListItem> StructureList
        {
            get { return _structureList; }
            set { Set(ref _structureList, value); }
        }
        private bool _importWorkflowInvalid = false;
        public bool ImportWorkflowInvalid
        {
            get { return _importWorkflowInvalid; }
            set { Set(ref _importWorkflowInvalid, value); }
        }
        private bool _structureDeletionInvalid = false;
        public bool StructureDeletionInvalid
        {
            get { return _structureDeletionInvalid; }
            set { Set(ref _structureDeletionInvalid, value); }
        }

        public StructureDeletionViewModel(ScriptContext context)
        {
            _context = context;

            PatientName = _context.Patient.Name;

            StructureList = new List<StructureListItem>();

            foreach (var struc in _context.StructureSet.Structures)
            {
                try
                {
                    StructureList.Add(new StructureListItem
                    {
                        Structure = struc,
                        HasContours = !struc.IsEmpty,
                        ToDelete = struc.IsEmpty
                    });
                }
                catch
                {
                    MessageBox.Show($"Something failed for {struc.Id}");
                }
            }
        }

        public bool StructureDeletionPerformUpdates()
        {
            // Warn user if there are contoured structures to be deleted
            var contouredStructures = StructureList.Where(x => x.ToDelete && x.HasContours).Select(x => x.Structure);
            if (contouredStructures.Count() > 0)
            {
                var result = MessageBox.Show($"The following structures have contours, are you sure you want to delete them?\n\n{String.Join("\n", contouredStructures.Select(x => x.Id))}", "Confirm Structure Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return false;
            }

            Log.Initialize(_context);
            _context.Patient.BeginModifications();

            try
            {
                var ss = _context.StructureSet;
                var failures = new List<Structure>();

                foreach (var struc in StructureList.Where(x => x.ToDelete).Select(x => x.Structure))
                {
                    if (ss.CanRemoveStructure(struc))
                        ss.RemoveStructure(struc);
                    else
                        failures.Add(struc);
                }

                if (failures.Count > 0)
                    MessageBox.Show($"Could not delete structures:\n\n{String.Join("\n", failures.Select(x => x.Id))}\n\nPlease delete manually", "Error Deleting Structures", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show($"Something failed when deleting structures, please delete them manually");
            }

            log.Info("Structure Deletion");
            LogManager.Shutdown();
            return true;
        }
    }
}
