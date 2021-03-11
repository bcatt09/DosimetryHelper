using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DosimetryHelper
{
    public class ImageListItem
    {
        public bool ToRename { get; set; }
        public String CurrentImageID { get; set; }
        public String NewImageID { get; set; }
        public String Comment { get; set; }
        public String Modality { get; set; }
        public String SeriesOrImage { get; set; }
        public String HistoryDateTime { get; set; }
    }
}
