using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DosimetryHelper
{
    public class ImportWorkflowViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Properties
        // Properties
        private ScriptContext _context;
        private IEnumerable<Regex> _courseIdRegexes;
        private IEnumerable<Regex> _planIdRegexes;
        private IEnumerable<Regex> _planNameRegexes;
        private IEnumerable<Regex> _referencePointRegexes;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        // Helper Regexes
        private Regex _courseNumberRegex;
        private Regex _planModalityRegex;

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
                ImageSets = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x == _context.Image ? DatasetName : x.Id));
                SelectedImageSet = _context.Image;
            }
        }
        private string _courseId;
        public string CourseId
        {
            get { return _courseId; }
            set
            {
                Set(ref _courseId, value);
                //SelectedCourse = CourseId;
                CourseIdFlag = true;
                Courses = new List<string>() { CourseId }.Concat( _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id)).ToList();
                SelectedCourseFromComboBox = CourseId;
            }
        }
        private string _planId;
        public string PlanId
        {
            get { return _planId; }
            set
            {
                Set(ref _planId, value);
                PlanIdFlag = true;
                PlanName = _planModalityRegex.Replace(PlanId,"");
                ReferencePointName = _planModalityRegex.Replace(PlanId, "");
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
        private string _referencePointName;
        public string ReferencePointName
        {
            get { return _referencePointName; }
            set
            {
                Set(ref _referencePointName, value);
                ReferencePointNameFlag = true;
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
        private IEnumerable<DatasetComboBoxItem> _imageSets;
        public IEnumerable<DatasetComboBoxItem> ImageSets
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
        private bool _datasetNameFlag;
        public bool DatasetNameFlag
        {
            get { return _datasetNameFlag; }
            set { Set(ref _datasetNameFlag, value); }
        }
        private bool _courseIdFlag;
        public bool CourseIdFlag
        {
            get { return _courseIdFlag; }
            set
            {
                Set(ref _courseIdFlag, value);
                if(CourseIdFlag)
                    Courses = new List<string>() { CourseId }.Concat(_context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id)).ToList();
                else
                    Courses = _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();
                SelectedCourseFromComboBox = Courses.FirstOrDefault();
                UpdateVisiblities();
            }
        }
        private bool _planIdFlag;
        public bool PlanIdFlag
        {
            get { return _planIdFlag; }
            set
            {
                Set(ref _planIdFlag, value);
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
            }
        }
        private bool _referencePointNameFlag;
        public bool ReferencePointNameFlag
        {
            get { return _referencePointNameFlag; }
            set
            {
                Set(ref _referencePointNameFlag, value);
                UpdateVisiblities();
            }
        }
        private bool _referencePointLocationFlag;
        public bool ReferencePointLocationFlag
        {
            get { return _referencePointLocationFlag; }
            set
            {
                Set(ref _referencePointLocationFlag, value);
            }
        }
        private bool _isoFlag;
        public bool IsoFlag
        {
            get { return _isoFlag; }
            set { Set(ref _isoFlag, value); }
        }
        private Visibility _planCreationOptionsVisibility;
        public Visibility PlanCreationOptionsVisibility
        {
            get { return _planCreationOptionsVisibility; }
            set { Set(ref _planCreationOptionsVisibility, value); }
        }
        private Visibility _referencePointLocationVisibility;
        public Visibility ReferencePointLocationVisibility
        {
            get { return _referencePointLocationVisibility; }
            set { Set(ref _referencePointLocationVisibility, value); }
        }

        #endregion

        // TextBox Validation
        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(CourseId))
                {
                    bool resultFound = false;
                    foreach (var regex in _courseIdRegexes)
                    {
                        if (regex.IsMatch(CourseId))
                        {
                            resultFound = true;
                            break;
                        }
                    }
                    if (!resultFound)
                        return "Doesn't match naming conventions";
                }

                else if (columnName == nameof(PlanId))
                {
                    bool resultFound = false;
                    foreach (var regex in _planIdRegexes)
                    {
                        if (regex.IsMatch(PlanId))
                        {
                            resultFound = true;
                            break;
                        }
                    }
                    if (!resultFound)
                        return "Doesn't match naming conventions";
                }

                else if (columnName == nameof(PlanName))
                {
                    bool resultFound = false;
                    foreach (var regex in _planNameRegexes)
                    {
                        if (regex.IsMatch(PlanName))
                        {
                            resultFound = true;
                            break;
                        }
                    }
                    if (!resultFound)
                        return "Doesn't match naming conventions";
                }

                else if (columnName == nameof(ReferencePointName))
                {
                    bool resultFound = false;
                    foreach (var regex in _planNameRegexes)
                    {
                        if (regex.IsMatch(ReferencePointName))
                        {
                            resultFound = true;
                            break;
                        }
                    }
                    if (!resultFound)
                        return "Doesn't match naming conventions";
                }

                else if (columnName == nameof(SelectedCourseFromComboBox))
                {
                    if (String.IsNullOrEmpty(SelectedCourseFromComboBox))
                        return "Please select a course";
                }

                return null;
            }
        }

        // Commands
        public RelayCommand FinalizeImportWorkflowCommand
        {
            get;
            private set;
        }

        // Constructor
        public ImportWorkflowViewModel(ScriptContext context)
        {
            _context = context;
            FinalizeImportWorkflowCommand = new RelayCommand(ImportWorkflowPerformUpdates, CanPerformUpdates);

            // Regular expressions for naming convention validation
            _courseIdRegexes = Properties.Resources.CourseSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^\d{{1,2}} (?:[RL] )?{x}$").Select(x => new Regex(x));
            _planIdRegexes = Properties.Resources.PlanSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^(?:[RL] )?{x}(?: (?:LN|Bst))?_\d[a-z]?\.?$").Select(x => new Regex(x));
            _planNameRegexes = Properties.Resources.PlanSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^(?:[RL] )?{x}(?: (?:LN|Bst))?$").Select(x => new Regex(x));
            _referencePointRegexes = _planNameRegexes;
            _courseNumberRegex = new Regex(@"(?<index>\d{1,2})");
            _planModalityRegex = new Regex(@"(?<modality>_\d)$");

            PatientName = _context.Patient.Name;
            Courses = _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();

            //find any image sets with the same Frame Of Reference (these are probably 4D images)
            ImageSets = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x.Id));
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
            CourseIdFlag = false;
            PlanIdFlag = false;
            PlanCreationOptionsVisibility = Visibility.Hidden;
            ReferencePointLocationVisibility = Visibility.Hidden;
        }

        // Methods
        public bool CanPerformUpdates()
        {
            if (PlanNameFlag)
            {
                if (!String.IsNullOrEmpty(SelectedCourseFromComboBox))
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public void ImportWorkflowPerformUpdates()
        {
            Log.Initialize(_context);
            _context.Patient.BeginModifications();

            StructureSet SelectedStructureSet = null;
            if (PlanIdFlag)
            {
                if (!SelectStructureSetForPlan(ref SelectedStructureSet, out string message))
                {
                    MessageBox.Show(message, "Error Selecting Structure Set For Plan", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }

            if (DatasetNameFlag)
            {
                if (!RenameDataset(SelectedStructureSet, out string message))
                {
                    MessageBox.Show(message, "Error Renaming CT Dataset", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            Course course = null;

            if (CourseIdFlag)
            {
                if(!AddCourse(ref course, out string message))
                {
                    MessageBox.Show(message, "Error Adding Course", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (PlanIdFlag)
            {
                if(!AddPlan(ref course, SelectedStructureSet, out string message))
                {
                    MessageBox.Show(message, "Error During Plan Creation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (IsoFlag)
            {
                if (!MoveUserOrigin(out string message))
                {
                    MessageBox.Show(message, "Error Moving User Origin", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            log.Info("Import Workflow");
            LogManager.Shutdown();
        }

        public bool SelectStructureSetForPlan(ref StructureSet ss, out string message)
        {
            try
            {
                //first see if there's a structure set already attached to this CT
                IEnumerable<StructureSet> selectedsstemp = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet);

                //if it's empty, add a new one
                if (selectedsstemp.Count() == 0)
                {
                    ss = SelectedImageSet.CreateNewStructureSet();
                    ss.Id = SelectedImageSet.Id;
                }
                //if not just use the first one that we find
                else
                    ss = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet).First();
            }
            catch (Exception e)
            {
                log.Error(e, "Error selecting Structure Set for plan creation\n");
                message = $"Could not select a structure set to use for plan creation\nChanges must be made manually\n\n{e.Message}";
                return false;
            }

            message = "";
            return true;
        }

        public bool RenameDataset(StructureSet ss, out string message)
        {

            try
            {
                //rename image and structure set (series doesn't seem to be possible)

                //if we've selected a different structure set to put the plan on rename that one instead
                if (ss != null)
                {
                    ss.Image.Id = DatasetName;
                    ss.Id = DatasetName;

                }
                //otherwise rename the currently open one
                else
                {
                    _context.Image.Id = DatasetName;
                    _context.StructureSet.Id = DatasetName;
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not rename image and structure set to {DatasetName}\n");
                message = $"Could not rename image and structure set to {DatasetName}\nYou may need to rename them manually\n\n{e.Message}";
                return false;
            }

            message = "";
            return true;
        }

        public bool AddCourse(ref Course c, out string message)
        {
            try
            {
                //add new course if possible
                if (_context.Patient.CanAddCourse())
                {
                    c = _context.Patient.AddCourse();
                    c.Id = CourseId;
                }
                else
                {
                    log.Error($"Not allowed to add a new course to patient {_context.Patient.Name}\n");
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not add new course ({CourseId})\n");
                message = $"Could not add new course ({CourseId})\nYou may need to add course and plan manually\n\n{e.GetType()}\n\n{e.Message}";
                return false;
            }

            message = "";
            return true;
        }

        public bool AddPlan(ref Course c, StructureSet ss, out string message)
        {
            try
            {
                //if selected course does not match the new name, find the existing course that matches the name
                if (SelectedCourseFromComboBox != CourseId)
                    c = _context.Patient.Courses.Where(x => x.Id == SelectedCourseFromComboBox).First();
                //otherwise use the course that we've already created that's stored in c

                //find the structure set associated with the selected image set for the plan to be attached to
                if (c.CanAddPlanSetup(ss))
                {
                    ExternalPlanSetup p = c.AddExternalPlanSetup(ss);
                    p.Id = PlanId;
                    if (PlanNameFlag)
                        p.Name = PlanName;

                    if (ReferencePointNameFlag)
                    {
                        try
                        {
                            // Eclipse creates a reference point when creating a plan so let's steal that one and rename it
                            if (!ReferencePointLocationFlag)
                            {
                                p.ReferencePoints.FirstOrDefault().Id = ReferencePointName;
                            }
                            else
                            {
                                var userOrigin = p.StructureSet.Image.UserOrigin;
                                p.AddReferencePoint(true, new VVector(userOrigin.x, userOrigin.y, userOrigin.z), ReferencePointName);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e, $"Couldn't add reference point to plan {p.Id}");
                            message = $"Could not add a new reference point to plan {p.Id}\nYou may need to add the reference point manually\n\n{e.Message}";
                            return false;
                        }
                    }
                }
                else
                {
                    log.Error($"Not allowed to add a new plan to course {c.Id} using structure set {ss} and image set {SelectedImageSet}\n");
                    message = $"Could not add a new plan to course {c.Id} using structure set {ss} and image set {SelectedImageSet}\nYou will need to add the plan manually";
                    return false;
                }
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not add new plan ({PlanName})\n");
                message = $"Could not add new plan ({PlanId})\nYou may need to add the plan manually\n\n{e.Message}";
                return false;
            }
            message = "";
            return true;
        }

        public bool MoveUserOrigin(out string message)
        {
            try
            {
                //move user origin to selected point
                SelectedImageSet.UserOrigin = SelectedPOI.CenterPoint;
            }
            catch (Exception e)
            {
                log.Error(e, $"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})\n");
                message = $"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})\nYou may need to set the user origin manually\n\n{e.Message}";
                return false;
            }

            message = "";
            return true;
        }

        public void UpdateVisiblities()
        {
            if (ReferencePointNameFlag)
                ReferencePointLocationVisibility = Visibility.Visible;
            else
                ReferencePointLocationVisibility = Visibility.Hidden;

            if (PlanIdFlag)
                PlanCreationOptionsVisibility = Visibility.Visible;
            else
                PlanCreationOptionsVisibility = Visibility.Hidden;
        }

        public void ImportWorkflowPerformUpdatesDebug()
        {
            MessageBox.Show($"Dataset - Flag:{DatasetNameFlag} - Name:{DatasetName}\n" +
                            $"POI - Flag:{IsoFlag} - Name:{SelectedPOI}\n" +
                            $"Course ID - Flag:{CourseIdFlag} - Name:{CourseId}\n" +
                            $"Plan ID - Flag:{PlanIdFlag} - Name:{PlanId} -> In Course:{SelectedCourseFromComboBox} -> On Dataset:{SelectedImageSet.Id}\n" +
                            $"Plan Name - Flag:{PlanNameFlag} - Name:{PlanName}\n" +
                            $"Reference Point - Flag:{ReferencePointNameFlag} - Name:{ReferencePointName} - Has Location?:{ReferencePointLocationFlag}");
        }

        public string ImportWorkflowGetNewCourseName()
        {
            try
            {
                var numberedCourses = _context.Patient.Courses.Where(x => _courseNumberRegex.IsMatch(x.Id));
                if (numberedCourses.Count() == 0)
                    return "1 ";

                // Find the highest index course and add one to it
                string lastCourse = numberedCourses.OrderByDescending(x => Int32.Parse(_courseNumberRegex.Match(x.Id).Groups["index"].Value)).First().Id;
                return (Int32.Parse(_courseNumberRegex.Match(lastCourse).Groups["index"].Value) + 1).ToString() + " ";
            }
            catch
            {
                return "";
            }
        }
    }
}
