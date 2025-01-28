using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QCLCalendarMaker
{
    /// <summary>
    /// Interaction logic for TaskSetWindow.xaml
    /// </summary>
    public partial class TaskSetWindow : Window
    {
        public ObservableCollection<TaskSet> TaskSets = new ObservableCollection<TaskSet>();
        public TaskSetWindow()
        {
            InitializeComponent();
            TaskSets = new ObservableCollection<TaskSet>
            {
                new TaskSet
                {
                    TaskSetName = "Default Task Set",
                    Tasks = new List<IndividualTask>
                    {
                        new IndividualTask
                        {
                            TaskName = "Sample Task",
                            DaysNeeded = 3,
                            Highlight = false,
                            Editable = true
                        }
                    }
                },
                // Add more TaskSets here if needed
            };
            TaskSetComboBox.SelectedIndex = -1;
            // Initialize the first combo box
            TaskSetComboBox.ItemsSource = TaskSets;
            TaskSetComboBox.DisplayMemberPath = "TaskSetName";
        }
        /// <summary>
        /// Fires whenever a new TaskSet is selected in the ComboBox.
        /// Clears the TaskStackPanel and lists the tasks from the newly selected TaskSet.
        /// </summary>
        private void TaskSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear previous items
            TaskStackPanel.Children.Clear();

            if (TaskSetComboBox.SelectedItem is TaskSet selectedTaskSet)
            {
                // Display each task's name (and other info as needed)
                PopulateTaskStackPanel();
            }
        }

        /// <summary>
        /// Enable/disable the AddTask button based on Name + Days constraints
        /// </summary>
        private void NewTaskNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAddTaskButton();
        }

        private void DaysOffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAddTaskButton();
        }

        /// <summary>
        /// Checks if NewTaskNameTextBox is not empty and DaysOffsetTextBox is a valid integer.
        /// If so, enable the AddTask button; otherwise disable it.
        /// </summary>
        private void ValidateAddTaskButton()
        {
            bool hasName = !string.IsNullOrWhiteSpace(NewTaskNameTextBox.Text);

            int parsed;
            bool isInteger = int.TryParse(DaysOffsetTextBox.Text, out parsed);

            AddTaskButton.IsEnabled = hasName && isInteger;
        }

        /// <summary>
        /// Click handler for adding a new IndividualTask to the currently selected TaskSet
        /// </summary>
        /// 
        private void PopulateTaskStackPanel()
        {
            TaskStackPanel.Children.Clear();
            if (TaskSetComboBox.SelectedItem is TaskSet selectedTaskSet)
            {
                StackPanel top_row = new StackPanel();
                top_row.Orientation = Orientation.Horizontal;
                top_row.Children.Add(new Label { Content = "Task Name" });
                top_row.Children.Add(new Label { Content = "Days from Previous task" });
                top_row.Children.Add(new Label { Content = "Allow edits?" });
                top_row.Children.Add(new Label { Content = "Highlight?" });
                TaskStackPanel.Children.Add(top_row);
                int total_days = 0;
                foreach (IndividualTask task in selectedTaskSet.Tasks)
                {
                    StackPanel new_row = new StackPanel();
                    new_row.Orientation = Orientation.Horizontal;
                    TaskStackPanel.Children.Add(
                        new TextBlock { Text = $"{task.TaskName} requires {task.DaysNeeded} days from previous task. {total_days} from beginning" }
                    );
                    total_days += task.DaysNeeded;
                }
            }
        }
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskSetComboBox.SelectedItem is TaskSet selectedTaskSet)
            {
                // Parse the DaysNeeded from the DaysOffsetTextBox
                int daysNeeded = int.Parse(DaysOffsetTextBox.Text);

                // Create a new IndividualTask based on input fields
                var newTask = new IndividualTask
                {
                    TaskName = NewTaskNameTextBox.Text,
                    DaysNeeded = daysNeeded,
                    Highlight = HighlightCheckBox.IsChecked ?? false,
                    Editable = AllowEditCheckBox.IsChecked ?? false
                };

                // Add the new task to the selected TaskSet
                selectedTaskSet.Tasks.Add(newTask);

                // Also add a visual line to the TaskStackPanel (or refresh it fully).
                // For simplicity, just add a new TextBlock here:

                // Optionally clear input fields, etc.
                // NewTaskNameTextBox.Text = "";
                // DaysOffsetTextBox.Text = "";
                PopulateTaskStackPanel();
            }
        }
    }
}
