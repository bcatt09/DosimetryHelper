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
using Types = VMS.TPS.Common.Model.Types;

namespace DosimetryHelper
{
    public class SetupFieldsViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Properties
        // Properties
        private ScriptContext _context;
        private string _cbctName = "CBCT";
        private string _apName = "AP";
        private string _paName = "PA";
        private string _rtLatName = "Rt Lat";
        private string _ltLatName = "Lt Lat";
        private List<PatientOrientation> _supportedOrientations = new List<PatientOrientation> { Types.PatientOrientation.HeadFirstSupine, Types.PatientOrientation.HeadFirstProne, Types.PatientOrientation.FeetFirstSupine, Types.PatientOrientation.FeetFirstProne };
        private bool _checkboxesUpdating = false;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string _patientName;
        public string PatientName
        {
            get { return _patientName; }
            set { Set(ref _patientName, value); }
        }
        private string _patientOrientation;
        public string PatientOrientation
        {
            get { return _patientOrientation; }
            set { Set(ref _patientOrientation, value); }
        }
        private string _suffix1 = "kV";
        public string Suffix1
        {
            get { return _suffix1; }
            set { Set(ref _suffix1, value); }
        }
        private string _suffix2 = "MV";
        public string Suffix2
        {
            get { return _suffix2; }
            set { Set(ref _suffix2, value); }
        }
        private bool _cbctFlag;
        public bool CbctFlag
        {
            get { return _cbctFlag; }
            set { Set(ref _cbctFlag, value); }
        }
        private bool _apFlag;
        public bool ApFlag
        {
            get { return _apFlag; }
            set
            {
                Set(ref _apFlag, value);
                if (!_checkboxesUpdating)
                {
                    if (ApFlag && RtLatFlag)
                        ApRtLatFlag = true;
                    else if (!ApFlag && !RtLatFlag)
                        ApRtLatFlag = false;
                    else
                        ApRtLatFlag = null;
                }
            }
        }
        private bool _rtLatFlag;
        public bool RtLatFlag
        {
            get { return _rtLatFlag; }
            set
            {
                Set(ref _rtLatFlag, value);
                if (!_checkboxesUpdating)
                {
                    if (ApFlag && RtLatFlag)
                        ApRtLatFlag = true;
                    else if (!ApFlag && !RtLatFlag)
                        ApRtLatFlag = false;
                    else
                        ApRtLatFlag = null;
                }
            }
        }
        private bool _paFlag;
        public bool PaFlag
        {
            get { return _paFlag; }
            set
            {
                Set(ref _paFlag, value);
                if (!_checkboxesUpdating)
                {
                    if (PaFlag && LtLatFlag)
                        PaLtLatFlag = true;
                    else if (!PaFlag && !LtLatFlag)
                        PaLtLatFlag = false;
                    else
                        PaLtLatFlag = null;
                }
            }
        }
        private bool _ltLatFlag;
        public bool LtLatFlag
        {
            get { return _ltLatFlag; }
            set
            {
                Set(ref _ltLatFlag, value);
                if (!_checkboxesUpdating)
                {
                    if (PaFlag && LtLatFlag)
                        PaLtLatFlag = true;
                    else if (!PaFlag && !LtLatFlag)
                        PaLtLatFlag = false;
                    else
                        PaLtLatFlag = null;
                }
            }
        }
        private bool? _apRtLatFlag;
        public bool? ApRtLatFlag
        {
            get { return _apRtLatFlag; }
            set 
            { 
                Set(ref _apRtLatFlag, value);
                if(ApRtLatFlag.HasValue)
                {
                    _checkboxesUpdating = true;
                    ApFlag = ApRtLatFlag.Value;
                    RtLatFlag = ApRtLatFlag.Value;
                    _checkboxesUpdating = false;
                }
            }
        }
        private bool? _paLtLatFlag;
        public bool? PaLtLatFlag
        {
            get { return _paLtLatFlag; }
            set
            {
                Set(ref _paLtLatFlag, value);
                if (PaLtLatFlag.HasValue)
                {
                    _checkboxesUpdating = true;
                    PaFlag = PaLtLatFlag.Value;
                    LtLatFlag = PaLtLatFlag.Value;
                    _checkboxesUpdating = false;
                }
            }
        }
        private bool _mvkvPairFlag;
        public bool MvKvPairFlag
        {
            get { return _mvkvPairFlag; }
            set 
            { 
                Set(ref _mvkvPairFlag, value);
                if (MvKvPairFlag)
                    SuffixesVisibility = Visibility.Visible;
                else
                    SuffixesVisibility = Visibility.Hidden;
            }
        }
        private Visibility _suffixesVisibility;
        public Visibility SuffixesVisibility
        {
            get { return _suffixesVisibility; }
            set { Set(ref _suffixesVisibility, value); }
        }
        #endregion

        // TextBox Validation
        public string Error => "";

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Suffix1))
                {
                    if (String.IsNullOrEmpty(Suffix1))
                        return "Please enter a suffix for field names";
                }

                else if (columnName == nameof(Suffix2))
                {
                    if (String.IsNullOrEmpty(Suffix2))
                        return "Please enter a suffix for field names";
                }

                return null;
            }
        }

        // Commands
        public RelayCommand FinalizeSetupFieldsCommand
        {
            get;
            private set;
        }

        // Constructor
        public SetupFieldsViewModel(ScriptContext context)
        {
            _context = context;
            FinalizeSetupFieldsCommand = new RelayCommand(SetupFieldsPerformUpdates, CanPerformUpdates);

            PatientName = _context.Patient.Name;
            PatientOrientation = _supportedOrientations.Contains(_context.PlanSetup.TreatmentOrientation) ? _context.PlanSetup.TreatmentOrientation.ToString() : "Unsupported";

            ApRtLatFlag = false;
            PaLtLatFlag = false;
            SuffixesVisibility = Visibility.Hidden;
        }

        // Methods
        public bool CanPerformUpdates()
        {
            if (MvKvPairFlag)
            {
                if (!String.IsNullOrEmpty(Suffix1) && !String.IsNullOrEmpty(Suffix2))
                    return true;
                else
                    return false;
            }
            else
                return true;
        }

        public void SetupFieldsPerformUpdates()
        {
            Log.Initialize(_context);
            _context.Patient.BeginModifications();

            ExternalPlanSetup plan = _context.PlanSetup as ExternalPlanSetup;
            Beam beam = plan.Beams.FirstOrDefault();
            VRect<double> jaws = new VRect<double>(-50, -50, 50, 50);
            ExternalBeamMachineParameters machineParams = new ExternalBeamMachineParameters(beam.TreatmentUnit.Id, beam.EnergyModeDisplayName, beam.DoseRate, "STATIC", "");

            int ap, pa, rtLat, ltLat;
            switch (PatientOrientation)
            {
                case "HeadFirstSupine":
                    ap = 0;
                    pa = 180;
                    rtLat = 270;
                    ltLat = 90;
                    break;
                case "FeetFirstSupine":
                    ap = 0;
                    pa = 180;
                    rtLat = 90;
                    ltLat = 270;
                    break;
                case "HeadFirstProne":
                    ap = 180;
                    pa = 0;
                    rtLat = 90;
                    ltLat = 270;
                    break;
                case "FeetFirstProne":
                    ap = 180;
                    pa = 0;
                    rtLat = 270;
                    ltLat = 90;
                    break;
                default:
                    ap = 0;
                    pa = 180;
                    rtLat = 270;
                    ltLat = 90;
                    break;
            }
            

            if (!MvKvPairFlag)
            {
                if (CbctFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, 0, 0, beam.IsocenterPosition).Id = _cbctName;
                if (ApFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, ap, 0, beam.IsocenterPosition).Id = _apName;
                if (PaFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, pa, 0, beam.IsocenterPosition).Id = _paName;
                if (RtLatFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, rtLat, 0, beam.IsocenterPosition).Id = _rtLatName;
                if (LtLatFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, ltLat, 0, beam.IsocenterPosition).Id = _ltLatName;
            }
            else
            {
                if (CbctFlag)
                    plan.AddSetupBeam(machineParams, jaws, 0, 0, 0, beam.IsocenterPosition).Id = _cbctName;
                if (ApFlag)
                {
                    plan.AddSetupBeam(machineParams, jaws, 0, ap, 0, beam.IsocenterPosition).Id = $"{_apName} {Suffix1}";
                    plan.AddSetupBeam(machineParams, jaws, 0, ap, 0, beam.IsocenterPosition).Id = $"{_apName} {Suffix2}";
                }
                if (PaFlag)
                {
                    plan.AddSetupBeam(machineParams, jaws, 0, pa, 0, beam.IsocenterPosition).Id = $"{_paName} {Suffix1}";
                    plan.AddSetupBeam(machineParams, jaws, 0, pa, 0, beam.IsocenterPosition).Id = $"{_paName} {Suffix2}";
                }
                if (RtLatFlag)
                {
                    plan.AddSetupBeam(machineParams, jaws, 0, rtLat, 0, beam.IsocenterPosition).Id = $"{_rtLatName} {Suffix1}";
                    plan.AddSetupBeam(machineParams, jaws, 0, rtLat, 0, beam.IsocenterPosition).Id = $"{_rtLatName} {Suffix2}";
                }
                if (LtLatFlag)
                {
                    plan.AddSetupBeam(machineParams, jaws, 0, ltLat, 0, beam.IsocenterPosition).Id = $"{_ltLatName} {Suffix1}";
                    plan.AddSetupBeam(machineParams, jaws, 0, ltLat, 0, beam.IsocenterPosition).Id = $"{_ltLatName} {Suffix2}";
                }
            }

            log.Info("Setup Fields");
            LogManager.Shutdown();
        }
    }
}
