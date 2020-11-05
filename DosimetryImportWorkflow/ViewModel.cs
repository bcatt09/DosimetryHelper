using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.Types;
using VMS.TPS.Common.Model.API;
using System.Windows;
using System.Text.RegularExpressions;

namespace VMS.TPS
{
	public class ViewModel : INotifyPropertyChanged
    {
        private ScriptContext _context;
        private string _patientName;
        private string _datasetName;
        private string _courseName;
        private string _planName;
        private IEnumerable<Structure> _pois;
        private Structure _selectedPoi;
        private IEnumerable<Image> _imageSets;
        private Image _selectedImageSet;
        private List<string> _courses;
        private string _selectedCourseFromComboBox;
        private string _selectedCourse;
        private bool _datasetNameFlag;
        private bool _courseNameFlag;
        private bool _planNameFlag;
        private bool _isoFlag;
        private Window _window;
        private Visibility _selectedCourseVisibility;
        private Visibility _selectedImageSetVisibility;
        private List<StructureListItem> _structureList;
        private bool _importWorkflowInvalid = false;
        private bool _structureDeletionInvalid = false;
        private Regex _regEx;
        private bool _initialLoad;

        public string PatientName { get { return _patientName; } set { _patientName = value; OnPropertyChanged("PatientName"); } }
        public string DatasetName { get { return _datasetName; } set { _datasetName = value; OnPropertyChanged("DatasetName"); } }
        public string CourseName { get { return _courseName; } set { _courseName = value; OnPropertyChanged("CourseName"); } }
        public string PlanName { get { return _planName; } set { _planName = value; OnPropertyChanged("PlanName"); } }
        public IEnumerable<Structure> POIs { get { return _pois; } set { _pois = value; OnPropertyChanged("POIs"); } }
        public Structure SelectedPOI { get { return _selectedPoi; } set { _selectedPoi = value; OnPropertyChanged("SelectedPOI"); } }
        public IEnumerable<Image> ImageSets { get { return _imageSets; } set { _imageSets = value; OnPropertyChanged("ImageSets"); } }
        public Image SelectedImageSet { get { return _selectedImageSet; } set { _selectedImageSet = value; OnPropertyChanged("SelectedImageSet"); } }
        public List<string> Courses { get { return _courses; } set { _courses = value; OnPropertyChanged("Courses"); } }
        public string SelectedCourseFromComboBox { get { return _selectedCourseFromComboBox; } set { _selectedCourseFromComboBox = value; OnPropertyChanged("SelectedCourseFromComboBox"); } }
        public string SelectedCourse { get { return _selectedCourse; } set { _selectedCourse = value; OnPropertyChanged("SelectedCourse"); } }
        public bool DatasetNameFlag { get { return _datasetNameFlag; } set { _datasetNameFlag = value; OnPropertyChanged("DatasetNameFlag"); } }
        public bool CourseNameFlag { get { return _courseNameFlag; } set { _courseNameFlag = value; OnPropertyChanged("CourseNameFlag"); } }
        public bool PlanNameFlag { get { return _planNameFlag; } set { _planNameFlag = value; OnPropertyChanged("PlanNameFlag"); } }
        public bool IsoFlag { get { return _isoFlag; } set { _isoFlag = value; OnPropertyChanged("IsoFlag"); } }
        public Visibility SelectedCourseVisibility { get { return _selectedCourseVisibility; } set { _selectedCourseVisibility = value; OnPropertyChanged("SelectedCourseVisibility"); } }
        public Visibility SelectedImageSetVisibility { get { return _selectedImageSetVisibility; } set { _selectedImageSetVisibility = value; OnPropertyChanged("SelectedImageSetVisibility"); } }
        public List<StructureListItem> StructureList { get { return _structureList; } set { _structureList = value; OnPropertyChanged("StructureList"); } }
        public bool ImportWorkflowInvalid { get { return _importWorkflowInvalid; } set { _importWorkflowInvalid = value; OnPropertyChanged("ImportWorkflowInvalid"); } }
        public bool StructureDeletionInvalid { get { return _structureDeletionInvalid; } set { _structureDeletionInvalid = value; OnPropertyChanged("StructureDeletionInvalid"); } }
        public RelayCommand 

        public ViewModel(ScriptContext context, Window window)
        {
            _window = window;
            _context = context;
        }

        

        public bool CanGoToImportWorkflow()
        {
            return _context.Image != null;
        }

        public bool CanGoToStructureDeletion()
        {
            return _context.StructureSet != null;
        }




        public void ValidateImportWorkflow()
        {
            if (_context.Image != null)
                ShowImportWorkflow();
            else
                ImportWorkflowInvalid = true;
        }

        public void ShowImportWorkflow()
        {
            _initialLoad = true;

            ImportWindow importWindow = new ImportWindow(this);
            _window.Content = importWindow;
            _window.Title = $"Import Workflow - {_context.Patient.Name}";

            //look for Courses named "C##" or "## Rt Lung"
            _regEx = new Regex(@"(?:C(?<index>\d+))|(?:(?<index>\d+) .*)");

            PatientName = _context.Patient.Name;
            DatasetName = "Enter Dataset Name";
            Courses = _context.Patient.Courses.Where(x => _regEx.IsMatch(x.Id)).OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();
            CourseName = "Enter Course ID";
            PlanName = "Enter Plan ID";

            //find any image sets with the same Frame Of Reference (these are probably 4D images)
            ImageSets = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1);
            if (ImageSets.Where(x => x.Id == _context.Image.Id).Count() > 0)
                SelectedImageSet = _context.Image;

            //POIs = context.Patient.StructureSets.Where(ss => ImageSets.Contains(ss.Image)).SelectMany(structureset => structureset.Structures.Where(structure => structure.DicomType == "MARKER"));
            POIs = _context.StructureSet.Structures.Where(s => s.DicomType == "MARKER");
            SelectedPOI = POIs.FirstOrDefault();
            if (SelectedPOI != null)
                IsoFlag = true;



            //can I somehow make it show POI - CT_50_1 to know what dataset it's coming from?
            // I can't seem to just get the structure set that it's from though to get the image
            




            DatasetNameFlag = false;
            CourseNameFlag = false;
            PlanNameFlag = false;
            SelectedCourseVisibility = Visibility.Hidden;
            SelectedImageSetVisibility = Visibility.Hidden;
            _window.SizeToContent = SizeToContent.WidthAndHeight;

            _initialLoad = false;
        }

        public void ImportWorkflowPerformUpdates()
        {
            _context.Patient.BeginModifications();

            StructureSet SelectedStructureSet = null;
            if (PlanNameFlag)
            {
                //first see if there's a structure set already attached to this CT
                IEnumerable<StructureSet> selectedsstemp = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet);

                //if it's empty, add a new one
                if (selectedsstemp.Count() == 0)
                {
                    SelectedStructureSet = SelectedImageSet.CreateNewStructureSet();
                    SelectedStructureSet.Id = SelectedImageSet.Id;
                }
                //if not just use the first one that we find
                else
                    SelectedStructureSet = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet).First();
            }
               
            try
            {
                if (DatasetNameFlag)
                {
                    //rename image and structure set (series doesn't seem to be possible)

                    //if we've selected a different structure set to put the plan on rename that one instead
                    if (SelectedStructureSet != null)
                    {
                        SelectedStructureSet.Image.Id = DatasetName;
                        SelectedStructureSet.Id = DatasetName;

                    }
                    //otherwise rename the currently open one
                    else
                    {
                        _context.Image.Id = DatasetName;
                        _context.StructureSet.Id = DatasetName;
                    }
                }
            }
            catch
            {
                //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not rename image and structure set to {DatasetName}");
                MessageBox.Show($"Could not rename image and structure set to {DatasetName}\n\nYou may need to rename them manually");
            }

            Course c = null;
            try
            {
                if (CourseNameFlag)
                {
                    //add new course if possible
                    if (_context.Patient.CanAddCourse())
                    {
                        c = _context.Patient.AddCourse();
                        c.Id = CourseName;
                    }
                    else
                    {
                        //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not add a new course to patient");
                        MessageBox.Show("Could not add a new course to patient\n\nYou may need to add course and plan manually", "Could Not Add Course", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not add new course ({CourseName})");
                MessageBox.Show($"Could not add new course ({CourseName})\n\nYou may need to add course and plan manually");
            }

            try
            {
                if (PlanNameFlag)
                {
                    //if selected course does not match the new name, find the existing course that matches the name
                    if (SelectedCourse != CourseName)
                        c = _context.Patient.Courses.Where(x => x.Id == SelectedCourse).First();
                    //otherwise use the one we just created that's already stored in c

                    //find the structure set associated with the selected image set for the plan to be attached to
                    if (c.CanAddPlanSetup(SelectedStructureSet))
                    {
                        ExternalPlanSetup p = c.AddExternalPlanSetup(SelectedStructureSet);
                        p.Id = PlanName;
                    }
                    else
                    {
                        //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not add a new plan to course {c.Id} using structure set {SelectedStructureSet} and image set {SelectedImageSet}");
                        MessageBox.Show($"Could not add a new plan to course {c.Id} using structure set {SelectedStructureSet} and image set {SelectedImageSet}\n\nYou may need to add the plan manually", "Could Not Add Plan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not add new plan ({PlanName})");
                MessageBox.Show($"Could not add new plan ({PlanName})\n\nYou may need to add the plan manually");
            }

            try
            {
                if (IsoFlag)
                {
                    //move user origin to selected point
                    SelectedImageSet.UserOrigin = SelectedPOI.CenterPoint;
                }
            }
            catch
            {
                //ESAPILog.Entry(_context, "DosimetryHelper", $"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})");
                MessageBox.Show($"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})\n\nYou may need to set the user origin manually");
            }

            //ESAPILog.Entry(_context, "DosimetryHelper", "");
        }

        public void ImportWorkflowPerformUpdatesDebug()
        {
            MessageBox.Show($"Dataset - Flag:{DatasetNameFlag} - Name:{DatasetName}\n" +
                            $"POI - Flag:{IsoFlag} - Name:{SelectedPOI}\n" +
                            $"Course - Flag:{CourseNameFlag} - Name:{CourseName}\n" +
                            $"Plan - Flag:{PlanNameFlag} - Name:{PlanName} - Course:{SelectedCourse} - Dataset:{SelectedImageSet}");
        }

        public string ImportWorkflowGetNewCourseName()
        {
            try
            {
                if (_context.Patient.Courses.Count() == 0)
                    return "1 ";

                //find the highest index course and add one to it
                string lastCourse = _context.Patient.Courses.Where(x => _regEx.IsMatch(x.Id)).OrderByDescending(x => Int32.Parse(_regEx.Match(x.Id).Groups["index"].Value)).First().Id;
                return (Int32.Parse(_regEx.Match(lastCourse).Groups["index"].Value) + 1).ToString() + " ";
            }
            catch
            {
                return "";
            }
        }

        public void ValidateStructureDeletion()
        {
            if (_context.StructureSet != null)
                ShowStructureDeletion();
            else
                StructureDeletionInvalid = true;
        }
        public void ShowStructureDeletion()
        {
            StructureWindow structureWindow = new StructureWindow(this);
            _window.Content = structureWindow;
            _window.Title = $"Structure Deletion - {_context.Patient.Name}";

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

            _window.SizeToContent = SizeToContent.WidthAndHeight;
        }

        public void StructureDeletionPerformUpdates()
        {
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

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
            if (!_initialLoad)
            {
                PropertyChangedEventHandler handler = PropertyChanged;

                if (name == "DatasetName")
                    DatasetNameFlag = true;
                if (name == "CourseName")
                {
                    SelectedCourse = CourseName;
                    CourseNameFlag = true;
                }
                if (name == "PlanName")
                    PlanNameFlag = true;
                if (name == "SelectedPOI")
                {
                    if (SelectedPOI != null)
                        IsoFlag = true;
                }
                if (name == "CourseNameFlag" || name == "PlanNameFlag")
                {
                    if (CourseNameFlag)
                        SelectedCourseVisibility = Visibility.Hidden;
                    else
                        SelectedCourseVisibility = Visibility.Visible;

                    if (PlanNameFlag)
                        SelectedImageSetVisibility = Visibility.Visible;
                    else
                    {
                        SelectedCourseVisibility = Visibility.Hidden;
                        SelectedImageSetVisibility = Visibility.Hidden;
                    }
                }

                if (name == "CourseName" || name == "SelectedCourseFromComboBox" || name == "CourseNameFlag")
                {
                    if (PlanNameFlag)
                    {
                        if (CourseNameFlag)
                            SelectedCourse = CourseName;
                        else
                            SelectedCourse = SelectedCourseFromComboBox;
                    }
                }


                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
		}
	}
}
