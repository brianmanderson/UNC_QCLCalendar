using System.Collections.Generic;


namespace QCLCalendarMaker
{
    public class IndividualTask
    {
        public string TaskName { get; set; }
        public int DaysNeeded { get; set; }
        public bool Highlight { get; set; }
        public bool Editable { get; set; } = false;
    }
    public class TaskSet
    {
        public string TaskSetName { get; set; }
        public List<IndividualTask> Tasks { get; set; }
    }

    public class TreatmentClass
    {
        public string Site { get; set; }           // e.g., "Lung", "Prostate", "Breast"
        public List<IndividualTask> SchedulingTasks { get; set; }
    }
    public class ModalityClass
    {
        public string Modality { get; set; }                // e.g., "3D", "IMRT", "VMAT"
        public List<TreatmentClass> Treatments { get; set; } // A list of sites + day offsets
    }
}
