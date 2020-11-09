using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace DosimetryHelper
{
    public class ImportWorkflowViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Properties
        // Properties
        private ScriptContext _context;
        private Regex _courseNameRegEx;

        private string _patientName;
        public string PatientName
        {
            get { return _patientName; }
            set { Set(ref _patientName, value); }
        }
        private string _datasetName;
        public string DatasetName
        {
            get { return _datasetName; }
            set
            {
                Set(ref _datasetName, value);
                DatasetNameFlag = true;
            }
        }
        private string _courseName;
        public string CourseName
        {
            get { return _courseName; }
            set
            {
                Set(ref _courseName, value);
                SelectedCourse = CourseName;
                CourseNameFlag = true;
            }
        }
        private string _planName;
        public string PlanName
        {
            get { return _planName; }
            set
            {
                Set(ref _planName, value);
                PlanNameFlag = true;
            }
        }
        private IEnumerable<Structure> _pois;
        public IEnumerable<Structure> POIs
        {
            get { return _pois; }
            set { Set(ref _pois, value); }
        }
        private Structure _selectedPoi;
        public Structure SelectedPOI
        {
            get { return _selectedPoi; }
            set
            {
                Set(ref _selectedPoi, value);
                if (SelectedPOI != null)
                    IsoFlag = true;
            }
        }
        private IEnumerable<Image> _imageSets;
        public IEnumerable<Image> ImageSets
        {
            get { return _imageSets; }
            set { Set(ref _imageSets, value); }
        }
        private Image _selectedImageSet;
        public Image SelectedImageSet
        {
            get { return _selectedImageSet; }
            set { Set(ref _selectedImageSet, value); }
        }
        private List<string> _courses;
        public List<string> Courses
        {
            get { return _courses; }
            set { Set(ref _courses, value); }
        }
        private string _selectedCourseFromComboBox;
        public string SelectedCourseFromComboBox
        {
            get { return _selectedCourseFromComboBox; }
            set
            {
                Set(ref _selectedCourseFromComboBox, value);
                UpdateVisiblities();
            }
        }
        private string _selectedCourse;
        public string SelectedCourse
        {
            get { return _selectedCourse; }
            set { Set(ref _selectedCourse, value); }
        }
        private bool _datasetNameFlag;
        public bool DatasetNameFlag
        {
            get { return _datasetNameFlag; }
            set { Set(ref _datasetNameFlag, value); }
        }
        private bool _courseNameFlag;
        public bool CourseNameFlag
        {
            get { return _courseNameFlag; }
            set
            {
                Set(ref _courseNameFlag, value);
                UpdateVisiblities();
            }
        }
        private bool _planNameFlag;
        public bool PlanNameFlag
        {
            get { return _planNameFlag; }
            set
            {
                Set(ref _planNameFlag, value);
                UpdateVisiblities();
            }
        }
        private bool _isoFlag;
        public bool IsoFlag
        {
            get { return _isoFlag; }
            set { Set(ref _isoFlag, value); }
        }
        private Visibility _selectedCourseVisibility;
        public Visibility SelectedCourseVisibility
        {
            get { return _selectedCourseVisibility; }
            set { Set(ref _selectedCourseVisibility, value); }
        }
        private Visibility _selectedImageSetVisibility;
        public Visibility SelectedImageSetVisibility
        {
            get { return _selectedImageSetVisibility; }
            set { Set(ref _selectedImageSetVisibility, value); }
        }

        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(CourseName) &&
                    !_courseNameRegEx.IsMatch(CourseName))
                    return "Doesn't match naming conventions";

                return null;
            }
        }

        #endregion

        // Constructor
        public ImportWorkflowViewModel(ScriptContext context)
        {
            _context = context;

            // We will want to mainly deal with courses which follow the naming convention of "## R Lung"
            _courseNameRegEx = new Regex(@"(?<index>\d{1,2})");
            //_regEx = new Regex(@"(?:C(?<index>\d+))|(?:(?<index>\d+) .*)");

            PatientName = _context.Patient.Name;
            Courses = _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();

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
        }

        // Methods
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

        public void UpdateVisiblities()
        {
            if (CourseNameFlag)
                SelectedCourseVisibility = Visibility.Hidden;
            else
                SelectedCourseVisibility = Visibility.Visible;

            if (PlanNameFlag)
            {
                if (CourseNameFlag)
                    SelectedCourse = CourseName;
                else
                    SelectedCourse = SelectedCourseFromComboBox;

                SelectedImageSetVisibility = Visibility.Visible;
            }
            else
            {
                SelectedCourseVisibility = Visibility.Hidden;
                SelectedImageSetVisibility = Visibility.Hidden;
            }
        }

        public void ImportWorkflowPerformUpdatesDebug()
        {
            MessageBox.Show($"Dataset - Flag:{DatasetNameFlag} - Name:{DatasetName}\n" +
                            $"POI - Flag:{IsoFlag} - Name:{SelectedPOI}\n" +
                            $"Course - Flag:{CourseNameFlag} - Name:{CourseName}\n" +
                            $"Plan - Flag:{PlanNameFlag} - Name:{PlanName} -> In Course:{SelectedCourse} -> On Dataset:{SelectedImageSet}");
        }

        public string ImportWorkflowGetNewCourseName()
        {
            try
            {
                var numberedCourses = _context.Patient.Courses.Where(x => _courseNameRegEx.IsMatch(x.Id));
                if (numberedCourses.Count() == 0)
                    return "1 ";

                // Find the highest index course and add one to it
                string lastCourse = numberedCourses.OrderByDescending(x => Int32.Parse(_courseNameRegEx.Match(x.Id).Groups["index"].Value)).First().Id;
                return (Int32.Parse(_courseNameRegEx.Match(lastCourse).Groups["index"].Value) + 1).ToString() + " ";
            }
            catch
            {
                return "";
            }
        }
    }
}
