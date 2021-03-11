using GalaSoft.MvvmLight;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DosimetryHelper
{
    public class ImageRenamingViewModel : ViewModelBase
    {
        private ScriptContext _context;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private string _patientName;
        public string PatientName
        {
            get { return _patientName; }
            set { Set(ref _patientName, value); }
        }
        private List<ImageListItem> _imageList;
        public List<ImageListItem> ImageList
        {
            get { return _imageList; }
            set { Set(ref _imageList, value); }
        }
        private bool _structureDeletionInvalid = false;
        public bool StructureDeletionInvalid
        {
            get { return _structureDeletionInvalid; }
            set { Set(ref _structureDeletionInvalid, value); }
        }

        public ImageRenamingViewModel(ScriptContext context)
        {
            _context = context;

            PatientName = _context.Patient.Name;

            ImageList = new List<ImageListItem>();

            foreach (var study in _context.Patient.Studies)
            {
                foreach (var series in study.Series.Where(x => x.Images.Count() > 1 &&
                    new List<SeriesModality> { SeriesModality.CT, SeriesModality.MR, SeriesModality.PT }.Contains(x.Modality) &&
                    x.HistoryUserDisplayName != "DICOM Service"))
                {
                    try
                    {
                        ImageList.Add(new ImageListItem
                        {
                            SeriesOrImage = "Series",
                            Comment = series.Comment,
                            CurrentImageID = series.Id,
                            NewImageID = series.Comment == "" ? "" : series.Comment.Substring(0, Math.Min(16, series.Comment.Length-1)),
                            HistoryDateTime = series.HistoryDateTime.ToString(),
                            Modality = series.Modality.ToString()
                        });
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"{e.Message}\n\n{e.StackTrace}", $"Something failed for Series {series.Id}");
                    }
                    foreach (var image in series.Images.Where(x => !x.ZDirection.Equals(new VVector(Double.NaN, Double.NaN, Double.NaN))))
                    {
                        try
                        {
                            ImageList.Add(new ImageListItem
                            {
                                SeriesOrImage = "Image",
                                Comment = image.Comment,
                                CurrentImageID = image.Id,
                                NewImageID = image.Comment == "" ? "" : image.Comment.Substring(0, Math.Min(16, image.Comment.Length-1)),
                                HistoryDateTime = image.HistoryDateTime.ToString(),
                                Modality = series.Modality.ToString()
                            });
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"{image.ImageType}\n\n{e.Message}\n\n{e.StackTrace}", $"Something failed for Image {image.Id}");
                        }
                    }
                }
            }
        }

        public bool ImageRenamingPerformUpdates()
        {
            //// Warn user if there are contoured structures to be deleted
            //var contouredStructures = StructureList.Where(x => x.ToDelete && x.HasContours).Select(x => x.Structure);
            //if (contouredStructures.Count() > 0)
            //{
            //    var result = MessageBox.Show($"The following structures have contours, are you sure you want to delete them?\n\n{String.Join("\n", contouredStructures.Select(x => x.Id))}", "Confirm Structure Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //    if (result == MessageBoxResult.No)
            //        return false;
            //}

            //Log.Initialize(_context);
            //_context.Patient.BeginModifications();

            //try
            //{
            //    var ss = _context.StructureSet;
            //    var failures = new List<Structure>();

            //    foreach (var struc in StructureList.Where(x => x.ToDelete).Select(x => x.Structure))
            //    {
            //        if (ss.CanRemoveStructure(struc))
            //            ss.RemoveStructure(struc);
            //        else
            //            failures.Add(struc);
            //    }

            //    if (failures.Count > 0)
            //        MessageBox.Show($"Could not delete structures:\n\n{String.Join("\n", failures.Select(x => x.Id))}\n\nPlease delete manually", "Error Deleting Structures", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //catch
            //{
            //    MessageBox.Show($"Something failed when deleting structures, please delete them manually");
            //}

            //log.Info("Structure Deletion");
            //LogManager.Shutdown();
            return true;
        }
    }
}
