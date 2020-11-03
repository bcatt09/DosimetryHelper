using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class StructureListItem
    {
        public Structure Structure { get; set; }
        public bool ToDelete { get; set; }
        public bool HasContours { get; set; }
    }
}
