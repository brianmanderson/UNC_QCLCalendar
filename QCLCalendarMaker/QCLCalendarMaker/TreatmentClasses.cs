using System;
using System.Linq;
using System.Collections.Generic;


namespace QCLCalendarMaker
{
    public class IndividualTask
    {
        public string TaskName { get; set; }
        public int DaysNeeded { get; set; }
        public bool Highlight { get; set; }
        public bool Editable { get; set; } = false;

        public IndividualTask() { }

        // Copy constructor for IndividualTask
        public IndividualTask(IndividualTask other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            TaskName = other.TaskName;
            DaysNeeded = other.DaysNeeded;
            Highlight = other.Highlight;
            Editable = other.Editable;
        }
    }

    public class TaskSet
    {
        public string TaskSetName { get; set; }
        public List<IndividualTask> Tasks { get; set; }

        public TaskSet() { }

        // Copy constructor for TaskSet
        public TaskSet(TaskSet other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            TaskSetName = other.TaskSetName;
            Tasks = other.Tasks != null
                ? other.Tasks.Select(task => new IndividualTask(task)).ToList()
                : new List<IndividualTask>();
        }
    }

    public class TreatmentClass
    {
        public string Site { get; set; }           // e.g., "Lung", "Prostate", "Breast"
        public List<IndividualTask> SchedulingTasks { get; set; }

        public TreatmentClass() { }

        // Copy constructor for TreatmentClass
        public TreatmentClass(TreatmentClass other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // Copy immutable types (like string) directly.
            Site = other.Site;

            // Deep copy each IndividualTask in the list.
            SchedulingTasks = other.SchedulingTasks != null
                ? other.SchedulingTasks.Select(task => new IndividualTask(task)).ToList()
                : new List<IndividualTask>();
        }
    }

    public class ModalityClass
    {
        public string Modality { get; set; }                // e.g., "3D", "IMRT", "VMAT"
        public List<TreatmentClass> Treatments { get; set; } // A list of sites + day offsets
    }
}
