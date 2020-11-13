using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DosimetryHelper
{
    public static class GetMachineHelper
    {
		private static class MachineNames
		{
			public static readonly string BAY_TB = "BAY_TB3384";
			public static readonly string CEN_EX = "CMCH-21EX";
			public static readonly string CLA_EX = "21EX";
			public static readonly string DET_IX = "IX_GROC";
			public static readonly string DET_TB = "GROC_TB1601";
			public static readonly string FAR_IX = "IX_Farmington";
			public static readonly string FLT_FrontTB = "TrueBeamSN2873";
			public static readonly string FLT_BackTB = "TrueBeam1030";
			public static readonly string LAN_IX = "ING21IX1";
			public static readonly string LAP_IX = "21IX-SN3743";
			public static readonly string MAC_TB = "MAC_TB3568";
			public static readonly string MAC_IX = "TRILOGY3789";
			public static readonly string MPH_TB = "TB2681";
			public static readonly string NOR_EX = "2100ex";
			public static readonly string NOR_IX = "TRILOGY";
			public static readonly string OWO_IX = "21IX-SN3856";
		}

		private static List<string> machineIds = new List<string> { MachineNames.BAY_TB, MachineNames.CEN_EX, MachineNames.CLA_EX, MachineNames.DET_IX, MachineNames.DET_TB, MachineNames.FAR_IX, MachineNames.FLT_BackTB, MachineNames.FLT_FrontTB, MachineNames.LAN_IX, MachineNames.LAP_IX, MachineNames.MAC_IX, MachineNames.MAC_TB, MachineNames.MPH_TB, MachineNames.NOR_EX, MachineNames.NOR_IX, MachineNames.OWO_IX };

        private static List<string> getLocalMachines(string hospital)
		{
			if (hospital.ToLower().Contains("bay"))
				return new List<string> { MachineNames.BAY_TB };
			else if (hospital.ToLower().Contains("central"))
				return new List<string> { MachineNames.CEN_EX };
			else if (hospital.ToLower().Contains("clarkston"))
				return new List<string> { MachineNames.CLA_EX };
			else if (hospital.ToLower().Contains("detroit"))
				return new List<string> { MachineNames.DET_IX, MachineNames.DET_TB };
			else if (hospital.ToLower().Contains("farmington"))
				return new List<string> { MachineNames.FAR_IX };
			else if (hospital.ToLower().Contains("flint"))
				return new List<string> { MachineNames.FLT_BackTB, MachineNames.FLT_FrontTB };
			else if (hospital.ToLower().Contains("lansing"))
				return new List<string> { MachineNames.LAN_IX };
			else if (hospital.ToLower().Contains("macomb"))
				return new List<string> { MachineNames.MAC_IX, MachineNames.MAC_TB };
			else if (hospital.ToLower().Contains("huron"))
				return new List<string> { MachineNames.MPH_TB };
			else if (hospital.ToLower().Contains("northern"))
				return new List<string> { MachineNames.NOR_EX, MachineNames.NOR_IX };
			else if (hospital.ToLower().Contains("owosso"))
				return new List<string> { MachineNames.OWO_IX };
			else
				return new List<string>();
		}

		public static List<string> GetMachineList(string hospital)
        {
			var machinesMinusLocal = machineIds.Except(getLocalMachines(hospital)).OrderBy(x => x);

			return getLocalMachines(hospital).Concat(machinesMinusLocal).ToList();
        }
    }
}
