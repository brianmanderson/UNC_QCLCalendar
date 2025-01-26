using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QCLCalendarMaker
{
    public class TreatmentClass
    {
        public string Site { get; set; }           // e.g., "Lung", "Prostate", "Breast"
        public int PlanningDays { get; set; }      // e.g., 4
        public int ContouringDays { get; set; }    // e.g., 2
    }
    public class ModalityClass
    {
        public string Modality { get; set; }                // e.g., "3D", "IMRT", "VMAT"
        public List<TreatmentClass> Treatments { get; set; } // A list of sites + day offsets
    }
}
