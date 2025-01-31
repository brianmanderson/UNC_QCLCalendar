using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QCLCalendarMaker
{
    /// <summary>
    /// Interaction logic for TaskSetWindow.xaml
    /// </summary>
    public partial class TaskSetWindow : Window
    {
        public ObservableCollection<TaskSet> TaskSets = new ObservableCollection<TaskSet>();
        public string taskset_filePath = Path.Combine(".", "TaskkSets.json");
        private void load_TaskSets()
        {
            if (File.Exists(taskset_filePath))
            {
                try
                {
                    string json = File.ReadAllText(taskset_filePath);
                    TaskSets = JsonSerializer.Deserialize<ObservableCollection<TaskSet>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {taskset_filePath}:\n{ex.Message}");
                    // Fallback or set a default
                }
            }
        }
        public TaskSetWindow()
        {
            InitializeComponent();
            load_TaskSets();
            TaskSetComboBox.SelectedIndex = -1;
            if (TaskSets.Count > 0)
            {
                TaskSetComboBox.SelectedIndex = 0;
            }
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
            DeleteTaskSetButton.IsEnabled = false;
            if (TaskSetComboBox.SelectedItem is TaskSet selectedTaskSet)
            {
                // Display each task's name (and other info as needed)
                PopulateTaskStackPanel();
                DeleteTaskSetButton.IsEnabled = true;
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
                // Top row remains the same (labels only)
                StackPanel top_row = new StackPanel();
                top_row.Orientation = Orientation.Horizontal;
                top_row.Children.Add(new Label { Content = "Task Name"});
                top_row.Children.Add(new Label { Content = "Days from Previous task" });
                top_row.Children.Add(new Label { Content = "Days from Sim" });
                top_row.Children.Add(new Label { Content = "Allow edits?" });
                top_row.Children.Add(new Label { Content = "Highlight?" });
                TaskStackPanel.Children.Add(top_row);

                int total_days = 0;
                foreach (IndividualTask task in selectedTaskSet.Tasks)
                {
                    total_days += task.DaysNeeded;

                    // Replaced the StackPanel with a Grid
                    Grid rowGrid = new Grid
                    {
                        Margin = new Thickness(0, 5, 0, 5),
                        DataContext = task // Bind directly to the IndividualTask
                    };

                    // Define columns for each control you add below
                    // Adjust widths if you need fixed sizes
                    for (int i = 0; i < 6; i++)
                    {
                        rowGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    // 1) TextBox for TaskName
                    TextBox nameTextBox = new TextBox
                    {
                        Text = task.TaskName,
                        Margin = new Thickness(0, 0, 20, 0)
                    };
                    Binding nameBinding = new Binding("TaskName")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    nameTextBox.SetBinding(TextBox.TextProperty, nameBinding);
                    rowGrid.Children.Add(nameTextBox);
                    Grid.SetColumn(nameTextBox, 0);

                    // 2) TextBox for DaysNeeded
                    TextBox daysTextBox = new TextBox
                    {
                        Margin = new Thickness(0, 0, 20, 0)
                    };
                    Binding daysBinding = new Binding("DaysNeeded")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    daysTextBox.SetBinding(TextBox.TextProperty, daysBinding);
                    rowGrid.Children.Add(daysTextBox);
                    Grid.SetColumn(daysTextBox, 1);

                    // 3) TextBlock to show total days so far
                    TextBlock daysFromStartTextBlock = new TextBlock
                    {
                        Text = total_days.ToString(),
                        Margin = new Thickness(0, 0, 20, 0)
                    };
                    rowGrid.Children.Add(daysFromStartTextBlock);
                    Grid.SetColumn(daysFromStartTextBlock, 2);

                    // 4) CheckBox bound to 'Editable'
                    CheckBox editableCheckBox = new CheckBox
                    {
                        Content = "Editable",
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    Binding editableBinding = new Binding("Editable")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    editableCheckBox.SetBinding(CheckBox.IsCheckedProperty, editableBinding);
                    rowGrid.Children.Add(editableCheckBox);
                    Grid.SetColumn(editableCheckBox, 3);

                    // 5) CheckBox bound to 'Highlight'
                    CheckBox highlightCheckBox = new CheckBox
                    {
                        Content = "Highlight",
                        Margin = new Thickness(0, 0, 20, 0)
                    };
                    Binding highlightBinding = new Binding("Highlight")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    highlightCheckBox.SetBinding(CheckBox.IsCheckedProperty, highlightBinding);
                    rowGrid.Children.Add(highlightCheckBox);
                    Grid.SetColumn(highlightCheckBox, 4);

                    // 6) A "Delete?" Button
                    Button deleteButton = new Button
                    {
                        Content = "Delete?",
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    deleteButton.Click += (s, e) =>
                    {
                        // Remove from the in-memory list
                        selectedTaskSet.Tasks.Remove(task);
                        // Re-populate the panel
                        PopulateTaskStackPanel();
                    };
                    rowGrid.Children.Add(deleteButton);
                    Grid.SetColumn(deleteButton, 5);

                    // Finally, add the row (grid) to the panel
                    TaskStackPanel.Children.Add(rowGrid);
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

        private void AddNewTaskSetButton_Click(object sender, RoutedEventArgs e)
        {
            TaskSet new_set = new TaskSet();
            new_set.TaskSetName = NewTaskSetBox.Text;
            new_set.Tasks = new List<IndividualTask>();
            TaskSets.Add(new_set);
            NewTaskSetBox.Text = "";
        }

        private void NewTaskSetNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddNewTaskSetButton.IsEnabled = false;
            if (NewTaskSetBox.Text != "")
            {
                AddNewTaskSetButton.IsEnabled = true;
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateTaskStackPanel();
        }

        private void Save()
        {
            string jsonString = JsonSerializer.Serialize(TaskSets, new JsonSerializerOptions { WriteIndented = true });

            // 4) Write to the file
            File.WriteAllText(taskset_filePath, jsonString);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void DeleteTaskSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskSetComboBox.SelectedItem is TaskSet selectedTaskSet)
            {
                TaskSets.Remove(selectedTaskSet);
                TaskSetComboBox.SelectedIndex = -1;
                PopulateTaskStackPanel();
            }
        }

        private void SaveAndExitButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }
    }
}
