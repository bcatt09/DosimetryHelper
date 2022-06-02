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
        private IEnumerable<Course> _existingCourses;
        private IEnumerable<PlanSetup> _existingPlans;
        private IEnumerable<ReferencePoint> _existingReferencePoints;
        private IEnumerable<Image> _existingImages;
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
        // Plan
        private string _planId;
        public string PlanId
        {
            get { return _planId; }
            set
            {
                Set(ref _planId, value);
                PlanIdFlag = true;
                PlanName = _planModalityRegex.Replace(PlanId, "");
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
        // Dataset
        private string _datasetName;
        public string DatasetName
        {
            get { return _datasetName; }
            set
            {
                Set(ref _datasetName, value);
                DatasetNameFlag = true;
                ImageSetList = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x == _context.Image ? DatasetName : x.Id));
                SelectedImageSet = _context.Image;
            }
        }
        private IEnumerable<DatasetComboBoxItem> _imageSetList;
        public IEnumerable<DatasetComboBoxItem> ImageSetList
        {
            get { return _imageSetList; }
            set { Set(ref _imageSetList, value); }
        }
        private Image _selectedImageSet;
        public Image SelectedImageSet
        {
            get { return _selectedImageSet; }
            set { Set(ref _selectedImageSet, value); }
        }
        // Course
        private string _courseId;
        public string CourseId
        {
            get { return _courseId; }
            set
            {
                Set(ref _courseId, value);
                CourseIdFlag = true;
                CourseList = new List<string>() { CourseId }.Concat( _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id)).ToList();
                SelectedCourse = CourseId;
                if (String.IsNullOrEmpty(CourseId))
                    CourseIdFlag = false;
            }
        }
        private List<string> _courseList;
        public List<string> CourseList
        {
            get { return _courseList; }
            set { Set(ref _courseList, value); }
        }
        private string _selectedCourse;
        public string SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                Set(ref _selectedCourse, value);
                UpdateVisiblities();
            }
        }
        // POI
        private IEnumerable<Structure> _poiList;
        public IEnumerable<Structure> POIList
        {
            get { return _poiList; }
            set { Set(ref _poiList, value); }
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
        // Machine
        private List<string> _machineList;
        public List<string> MachineList
        {
            get { return _machineList; }
            set { Set(ref _machineList, value); }
        }
        private string _selectedMachine;
        public string SelectedMachine
        {
            get { return _selectedMachine; }
            set { Set(ref _selectedMachine, value); }
        }
        // Checkbox flags
        private bool _datasetNameFlag;
        public bool DatasetNameFlag
        {
            get { return _datasetNameFlag; }
            set 
            { 
                Set(ref _datasetNameFlag, value);
                if (!DatasetNameFlag)
                {
                    ImageSetList = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x.Id));
                    SelectedImageSet = _context.Image;
                }
                else
                {
                    ImageSetList = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x == _context.Image ? DatasetName : x.Id));
                    SelectedImageSet = _context.Image;
                }
            }
        }
        private bool _courseIdFlag;
        public bool CourseIdFlag
        {
            get { return _courseIdFlag; }
            set
            {
                Set(ref _courseIdFlag, value);
                if(CourseIdFlag)
                    CourseList = new List<string>() { CourseId }.Concat(_context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id)).ToList();
                else
                    CourseList = _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();
                SelectedCourse = CourseList.FirstOrDefault();
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
                AddDummyFieldFlag = PlanIdFlag;
                PlanNameFlag = PlanIdFlag;
                ReferencePointNameFlag = PlanIdFlag;
                RaisePropertyChanged(() => PlanId);
                UpdateVisiblities();
            }
        }
        private bool _planNameFlag;
        public bool PlanNameFlag
        {
            get { return _planNameFlag; }
            set{ Set(ref _planNameFlag, value); }
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
        private bool _addDummyFieldFlag;
        public bool AddDummyFieldFlag
        {
            get { return _addDummyFieldFlag; }
            set { Set(ref _addDummyFieldFlag, value); }
        }
        private bool _isoFlag;
        public bool IsoFlag
        {
            get { return _isoFlag; }
            set { Set(ref _isoFlag, value); }
        }
        // Visibilities
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
                // Dataset Name validation
                if (columnName == nameof(DatasetName))
                {
                    // Dataset Name not entered
                    if (DatasetNameFlag && String.IsNullOrEmpty(DatasetName))
                        return "Please enter a Dataset Name";

                    // Dataset Name starts with a space
                    if (!String.IsNullOrEmpty(DatasetName) && DatasetName.StartsWith(" "))
                        return "Remove any leading spaces";

                    // Dataset Name ends with a space
                    if (!String.IsNullOrEmpty(DatasetName) && DatasetName.EndsWith(" "))
                        return "Remove any trailing spaces";

                    // Dataset Name already exists in Series
                    if (_existingImages.Count(x => x.Id.ToLower() == DatasetName.ToLower() && x.Series.Id == _context.Image.Series.Id) > 0)
                        return "Dataset Name already exists in this Series";

                    // Dataset Name already exists in Patient
                    if (_existingImages.Count(x => x.Id.ToLower() == DatasetName.ToLower()) > 0)
                        return "Dataset Name already exists in this Patient";

                    // Dataset Name doesn't follow date format
                    if (!DatasetName.StartsWith(DateTime.Now.ToString("yyyyMMdd")))
                        return $"Doesn't follow date naming format (start with {DateTime.Now:yyyyMMdd})";
                }
                // Course ID validation
                else if (columnName == nameof(CourseId))
                {
                    // Course ID already exists
                    if (_existingCourses.Select(x => x.Id.ToLower()).Contains(CourseId.ToLower()))
                        return "Course ID already exists, uncheck \"Create Course\"";

                    // Course starts with a space
                    if (!String.IsNullOrEmpty(CourseId) && CourseId.StartsWith(" "))
                        return "Remove any leading spaces";

                    // Course ends with a space
                    if (!String.IsNullOrEmpty(CourseId) && CourseId.EndsWith(" "))
                        return "Remove any trailing spaces";

                    // Course ID doesn't match naming conventions
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
                // Plan ID validation
                else if (columnName == nameof(PlanId))
                {
                    // Plan ID not entered
                    if (PlanIdFlag && String.IsNullOrEmpty(PlanId))
                        return "Please enter a Plan ID";

                    // Plan ID starts with a space
                    if (!String.IsNullOrEmpty(PlanId) && PlanId.StartsWith(" "))
                        return "Remove any leading spaces";

                    // Plan ID ends with a space
                    if (!String.IsNullOrEmpty(PlanId) && PlanId.EndsWith(" "))
                        return "Remove any trailing spaces";

                    // Plan ID already exists in selected Course
                    if (_existingPlans.Count(x => x.Course.Id == SelectedCourse && x.Id.ToLower() == PlanId.ToLower()) > 0)
                        return "Plan ID already exists in the selected Course";

                    // Plan ID doesn't match naming conventions
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
                // Plan Name validation
                else if (columnName == nameof(PlanName))
                {
                    // Plan Name starts with a space
                    if (!String.IsNullOrEmpty(PlanName) && PlanName.StartsWith(" "))
                        return "Remove any leading spaces";

                    // Plan Name ends with a space
                    if (!String.IsNullOrEmpty(PlanName) && PlanName.EndsWith(" "))
                        return "Remove any trailing spaces";

                    // Plan Name doesn't match naming conventions
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
                // Reference Point Name validation
                else if (columnName == nameof(ReferencePointName))
                {
                    // Reference Point Name starts with a space
                    if (!String.IsNullOrEmpty(ReferencePointName) && ReferencePointName.StartsWith(" "))
                        return "Remove any leading spaces";

                    // Reference Point Name ends with a space
                    if (!String.IsNullOrEmpty(ReferencePointName) && ReferencePointName.EndsWith(" "))
                        return "Remove any trailing spaces";

                    // Reference Point Name already exists
                    if (_existingReferencePoints.Select(x => x.Id.ToLower()).Contains(ReferencePointName.ToLower()))
                        return "Reference Point name already exists";

                    // Reference Point Name doesn't match naming conventions
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
                // Selected Course validation
                else if (columnName == nameof(SelectedCourse))
                {
                    if (String.IsNullOrEmpty(SelectedCourse))
                        return "Please select a course";
                    if (_existingCourses.Where(x => x.Id == SelectedCourse).First().IsCompleted())
                        return "Course Status is 'Completed'";
                }

                return null;
            }
        }

        // Commands
        public RelayCommand FinalizeImportWorkflowCommand { get; private set; }

        // Constructor
        public ImportWorkflowViewModel(ScriptContext context)
        {
            _context = context;
            _existingCourses = context.Patient.Courses;
            _existingPlans = context.Patient.Courses.SelectMany(x => x.PlanSetups);
            _existingReferencePoints = context.Patient.ReferencePoints;
            _existingImages = context.Patient.Studies.SelectMany(x => x.Series.SelectMany(y => y.Images.Where(i => i.ZSize > 1)));
            FinalizeImportWorkflowCommand = new RelayCommand(ImportWorkflowPerformUpdates, CanPerformUpdates);

            // Regular expressions for naming convention validation
            _courseIdRegexes = Properties.Resources.CourseSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^\d{{1,2}} (?:[RL] )?{x}$").Select(x => new Regex(x));
            _planIdRegexes = Properties.Resources.PlanSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^(?:[RL] )?{x}(?: (?:LN|Bst))?_\d[a-z]?\.?$").Select(x => new Regex(x));
            _planNameRegexes = Properties.Resources.PlanSiteNames.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(x => $@"^(?:[RL] )?{x}(?: (?:LN|Bst))?$").Select(x => new Regex(x));
            _referencePointRegexes = _planNameRegexes;
            _courseNumberRegex = new Regex(@"(?<index>\d{1,2})");
            _planModalityRegex = new Regex(@"(?<modality>_\d)(?:\.|[a-z])?$");

            PatientName = _context.Patient.Name;
            CourseList = _context.Patient.Courses.OrderByDescending(c => c.HistoryDateTime).Select(c => c.Id).ToList();
            SelectedCourse = CourseList.FirstOrDefault();

            MachineList = GetMachineHelper.GetMachineList(_context.Patient.Hospital.Id);
            SelectedMachine = MachineList.First();

            // Find any image sets with the same Frame Of Reference (these are probably 4D images)
            ImageSetList = _context.Image.Series.Study.Series.SelectMany(series => series.Images).Where(image => image.FOR == _context.Image.FOR && image.ZSize > 1).Select(x => new DatasetComboBoxItem(x, x.Id));
            if (ImageSetList.Where(x => x.Id == _context.Image.Id).Count() > 0)
                SelectedImageSet = _context.Image;
            
            // Get POIs from opened dataset and select the first one
            POIList = _context.StructureSet?.Structures?.Where(s => s.DicomType == "MARKER");
            SelectedPOI = POIList?.FirstOrDefault();
            if (SelectedPOI != null)
                IsoFlag = true;

            // Can I somehow make it show POI - CT_50_1 to know what dataset it's coming from?
            // I can't seem to just get the structure set that it's from though to get the image

            PlanCreationOptionsVisibility = Visibility.Hidden;
            ReferencePointLocationVisibility = Visibility.Hidden;
        }

        // Methods
        public bool CanPerformUpdates()
        {
            // Dataset Name has leading/trailing spaces
            if (!String.IsNullOrEmpty(DatasetName) && (DatasetName.EndsWith(" ") || DatasetName.StartsWith(" ")))
                return false;
            // Dataset Name exists in Series
            else if (!String.IsNullOrEmpty(DatasetName) && _existingImages.Count(x => x.Id.ToLower() == DatasetName.ToLower() && x.Series.Id == _context.Image.Series.Id) > 0)
                return false;
            // Course is not selected
            if (String.IsNullOrEmpty(SelectedCourse))
                return false;
            // Course ID already exists
            else if (CourseIdFlag && _existingCourses.Select(x => x.Id.ToLower()).Contains(CourseId.ToLower()))
                return false;
            // Course is completed
            else if (!CourseIdFlag && !String.IsNullOrEmpty(SelectedCourse) && _existingCourses.FirstOrDefault(x => x.Id == SelectedCourse).IsCompleted())
                return false;
            // Plan ID not entered and a Plan is being created
            else if (PlanIdFlag && String.IsNullOrEmpty(PlanId))
                return false;
            // Plan ID already exists in selected Course
            else if (SelectedCourse != null && PlanId != null && _existingPlans.Count(x => x.Course.Id == SelectedCourse && x.Id.ToLower() == PlanId.ToLower()) > 0)
                return false;
            // Reference Point Name already exists
            else if (!String.IsNullOrEmpty(ReferencePointName) && _existingReferencePoints.Select(x => x.Id.ToLower()).Contains(ReferencePointName.ToLower()))
                return false;
            // Course ID has leading/trailing spaces
            else if (!String.IsNullOrEmpty(CourseId) && (CourseId.EndsWith(" ") || CourseId.StartsWith(" ")))
                return false;
            // Plan ID has leading/trailing spaces
            else if (!String.IsNullOrEmpty(PlanId) && (PlanId.EndsWith(" ") || PlanId.StartsWith(" ")))
                return false;
            // Plan Name has leading/trailing spaces
            else if (!String.IsNullOrEmpty(PlanName) && (PlanName.EndsWith(" ") || PlanName.StartsWith(" ")))
                return false;
            // Reference Point Name has leading/trailing spaces
            else if (!String.IsNullOrEmpty(ReferencePointName) && (ReferencePointName.EndsWith(" ") || ReferencePointName.StartsWith(" ")))
                return false;
            else
                return true;
        }

        public void ImportWorkflowPerformUpdates()
        {
            Log.Initialize(_context);
            _context.Patient.BeginModifications();

            StructureSet selectedStructureSet = null;
            Course course = null;
            ExternalPlanSetup plan = null;

            #region Select StructureSet for Plan
            if (PlanIdFlag)
            {
                try
                {
                    //first see if there's a structure set already attached to this CT
                    IEnumerable<StructureSet> selectedsstemp = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet);

                    //if it's empty, add a new one
                    if (selectedsstemp.Count() == 0)
                    {
                        selectedStructureSet = SelectedImageSet.CreateNewStructureSet();
                        selectedStructureSet.Id = SelectedImageSet.Id;
                    }
                    //if not just use the first one that we find
                    else
                        selectedStructureSet = _context.Patient.StructureSets.Where(s => s.Image == SelectedImageSet).First();
                }
                catch (Exception e)
                {
                    log.Error(e, "Error selecting Structure Set for plan creation\n");
                    MessageBox.Show($"Could not select a structure set to use for plan creation\nChanges must be made manually\n\n{e.Message}", "Error Selecting Structure Set For Plan", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            #region Rename Dataset
            if (DatasetNameFlag)
            {
                try
                {
                    //rename image and structure set (series doesn't seem to be possible)

                    //if we've selected a different structure set to put the plan on rename that one instead
                    if (selectedStructureSet != null)
                    {
                        selectedStructureSet.Image.Id = DatasetName;
                        selectedStructureSet.Id = DatasetName;

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
                    MessageBox.Show($"Could not rename image and structure set to {DatasetName}\nYou may need to rename them manually\n\n{e.Message}", "Error Renaming CT Dataset", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            #region Add Course
            if (CourseIdFlag)
            {
                try
                {
                    // Add new course if possible
                    if (_context.Patient.CanAddCourse())
                    {
                        course = _context.Patient.AddCourse();
                        course.Id = CourseId;
                    }
                    else
                    {
                        log.Error($"Not allowed to add a new course to patient {_context.Patient.Name}\n");
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, $"Could not add new course ({CourseId})\n");
                    MessageBox.Show($"Could not add new course ({CourseId})\nYou may need to add course and plan manually\n\n{e.GetType()}\n\n{e.Message}", "Error Adding Course", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            #region Add Plan
            if (PlanIdFlag)
            {
                try
                {
                    //if selected course does not match the new name, find the existing course that matches the name
                    if (SelectedCourse != CourseId)
                        course = _context.Patient.Courses.Where(x => x.Id == SelectedCourse).First();
                    //otherwise use the course that we've already created that's stored in c

                    //find the structure set associated with the selected image set for the plan to be attached to
                    if (course.CanAddPlanSetup(selectedStructureSet))
                    {
                        plan = course.AddExternalPlanSetup(selectedStructureSet);
                        plan.Id = PlanId;
                        if (PlanNameFlag)
                            plan.Name = PlanName;

                        if (ReferencePointNameFlag)
                        {
                            try
                            {
                                // Eclipse creates a reference point when creating a plan so let's steal that one and rename it
                                plan.ReferencePoints.FirstOrDefault().Id = ReferencePointName;
                            }
                            catch (Exception e)
                            {
                                log.Error(e, $"Couldn't add reference point to plan {plan.Id}");
                                MessageBox.Show($"Could not add a new reference point to plan {plan.Id}\nYou may need to add the reference point manually\n\n{e.Message}", "Error Adding Reference Point", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        log.Error($"Not allowed to add a new plan to course {course.Id} using structure set {selectedStructureSet} and image set {SelectedImageSet}\n");
                        MessageBox.Show($"Could not add a new plan to course {course.Id} using structure set {selectedStructureSet} and image set {SelectedImageSet}\nYou will need to add the plan manually", "Plan Creation Not Allowed", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, $"Could not add new plan ({PlanName})\n");
                    MessageBox.Show($"Could not add new plan ({PlanId})\nYou may need to add the plan manually\n\n{e.Message}", "Error During Plan Creation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            #region Move User Origin
            if (IsoFlag)
            {
                try
                {
                    //move user origin to selected point
                    SelectedImageSet.UserOrigin = SelectedPOI.CenterPoint;
                }
                catch (Exception e)
                {
                    log.Error(e, $"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})\n");
                    MessageBox.Show($"Could not move user origin to {SelectedPOI.Id} ({SelectedPOI.CenterPoint})\nYou may need to set the user origin manually\n\n{e.Message}", "Error Moving User Origin", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            #region Add Dummy Field
            if (AddDummyFieldFlag)
            {
                try
                {
                    VRect<double> jaws = new VRect<double>(-50, -50, 50, 50);
                    ExternalBeamMachineParameters machineParams = new ExternalBeamMachineParameters(SelectedMachine, "6X", 600, "STATIC", "");
                    plan.AddStaticBeam(machineParams, jaws, 0, 0, 0, SelectedImageSet.UserOrigin);
                }
                catch (Exception e)
                {
                    log.Error(e, $"Could not add dummy field to plan {plan.Id}\n");
                    MessageBox.Show($"Could not add dummy field to plan {plan.Id}\nYou may need to add the field manually\n\n{e.Message}", "Error Adding Dummy Field", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            #endregion

            log.Info("Import Workflow");
            LogManager.Shutdown();
        }

        public bool MoveUserOrigin(out string message)
        {
            try
            {
                // Move User Origin to selected point
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
                            $"Plan ID - Flag:{PlanIdFlag} - Name:{PlanId} -> In Course:{SelectedCourse} -> On Dataset:{SelectedImageSet.Id}\n" +
                            $"Plan Name - Flag:{PlanNameFlag} - Name:{PlanName}\n" +
                            $"Reference Point - Flag:{ReferencePointNameFlag} - Name:{ReferencePointName}");
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
