using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace DosimetryHelper
{
    public class DatasetComboBoxItem
    {
        public Image Image { get; set; }
        public string Id { get; set; }

        public DatasetComboBoxItem(Image image, string id)
        {
            Image = image;
            Id = id;
        }
    }
}
