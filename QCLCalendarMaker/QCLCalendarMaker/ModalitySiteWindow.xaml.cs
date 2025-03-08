using System;
using System.Collections.Generic;
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
using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace QCLCalendarMaker
{
    /// <summary>
    /// Interaction logic for ModalitySiteWindow.xaml
    /// </summary>
    public partial class ModalitySiteWindow : Window
    {
        public string TaskSetName { get; set; }
        public string taskset_filePath = Path.Combine(".", "TaskkSets.json");
        public string modalities_filePath = Path.Combine(".", "Modalities.json");
        public ObservableCollection<TaskSet> TaskSets = new ObservableCollection<TaskSet>();
        public ObservableCollection<ModalityClass> Modalities = new ObservableCollection<ModalityClass>();
        private ModalityClass _selectedModality;
        private TreatmentClass _selectedTreatment;
        private TaskSet _selectedTaskSet;
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
        private void load_Modalities()
        {
            if (File.Exists(modalities_filePath))
            {
                try
                {
                    string json = File.ReadAllText(modalities_filePath);
                    Modalities = JsonSerializer.Deserialize<ObservableCollection<ModalityClass>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {modalities_filePath}:\n{ex.Message}");
                    // Fallback or set a default
                }
            }
        }
        public ModalitySiteWindow()
        {
            InitializeComponent();
            load_TaskSets();
            load_Modalities();
            TaskSetComboBox.ItemsSource = TaskSets;
            PopulateModalityListedStackPanel();
        }
        private void PopulateModalityListedStackPanel()
        {
            ModalityListedStackPanel.Children.Clear();
            DeleteModalityButton.IsEnabled = false;
            TaskSetComboBox.IsEnabled = false;
            AddTaskSetButton.IsEnabled = false;
            foreach (ModalityClass modalityItem in Modalities)
            {
                // Create a TextBox for the Modality name
                var txt = new TextBox
                {
                    Width = 80,
                    Margin = new Thickness(0, 0, 0, 5),
                    DataContext = modalityItem
                };
                // TwoWay binding to the 'Modality' property
                var binding = new Binding("Modality")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                txt.SetBinding(TextBox.TextProperty, binding);

                // On focus, set the selected modality and populate treatments
                txt.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    // Mark this as the selected Modality
                    _selectedModality = modalityItem;

                    // Reset backgrounds of all siblings to White
                    foreach (var child in ModalityListedStackPanel.Children)
                    {
                        if (child is TextBox otherTxt)
                        {
                            otherTxt.Background = Brushes.White;
                        }
                    }

                    // Highlight the clicked TextBox
                    txt.Background = Brushes.LightGray;

                    // Refresh the Treatment list
                    PopulateTreatmentListedStackPanel();

                    // Enable the "Delete" button
                    DeleteModalityButton.IsEnabled = true;

                    // Optional: handle the event so it doesn't bubble further
                    e.Handled = true;
                };

                ModalityListedStackPanel.Children.Add(txt);
            }
        }
        private void PopulateTreatmentListedStackPanel()
        {
            TreatmentListedStackPanel.Children.Clear();
            IndividualTaskPanel.Children.Clear();
            if (_selectedModality == null) return;
            if (_selectedModality?.Treatments == null)
                return;
            TaskSetComboBox.IsEnabled = false;
            AddTaskSetButton.IsEnabled = false;
            foreach (TreatmentClass treatment in _selectedModality.Treatments)
            {
                var txt = new TextBox
                {
                    Width = 100,
                    Margin = new Thickness(0, 0, 0, 5),
                    DataContext = treatment
                };
                var binding = new Binding("Site")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                txt.SetBinding(TextBox.TextProperty, binding);

                // On focus, set the selected treatment and update the Task list
                txt.PreviewMouseLeftButtonDown += (s, e) =>
                {
                    // Mark this as the selected Modality
                    _selectedTreatment = treatment;

                    // Reset backgrounds of all siblings to White
                    foreach (var child in TreatmentListedStackPanel.Children)
                    {
                        if (child is TextBox otherTxt)
                        {
                            otherTxt.Background = Brushes.White;
                        }
                    }

                    // Highlight the clicked TextBox
                    txt.Background = Brushes.LightGray;

                    // Refresh the Treatment list
                    PopulateIndividualTaskPanel();

                    // Enable the "Delete" button
                    DeleteTreatmentButton.IsEnabled = true;

                    // Optional: handle the event so it doesn't bubble further
                    e.Handled = true;
                };

                TreatmentListedStackPanel.Children.Add(txt);
            }
        }
        private void PopulateIndividualTaskPanel()
        {
            IndividualTaskPanel.Children.Clear();
            TaskSetComboBox.IsEnabled = false;
            AddTaskSetButton.IsEnabled = false;
            if (_selectedTreatment?.SchedulingTasks == null) return;
            if (_selectedTreatment.SchedulingTasks.Count == 0)
            {
                TaskSetComboBox.IsEnabled = true;
                AddTaskSetButton.IsEnabled = true;
            }
            foreach (var task in _selectedTreatment.SchedulingTasks)
            {
                // Create a grid for this "row"
                var rowGrid = new Grid
                {
                    Margin = new Thickness(0, 5, 0, 5),
                    DataContext = task
                };

                // Define columns (adjust widths as needed)
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // TaskName
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });  // DaysNeeded
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Editable
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });  // Highlight
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // Delete Button

                // TaskName TextBox
                var taskNameText = new TextBox();
                taskNameText.SetBinding(TextBox.TextProperty,
                    new Binding("TaskName") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                Grid.SetColumn(taskNameText, 0);
                rowGrid.Children.Add(taskNameText);

                // DaysNeeded TextBox
                var daysText = new TextBox();
                daysText.SetBinding(TextBox.TextProperty,
                    new Binding("DaysNeeded") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                Grid.SetColumn(daysText, 1);
                rowGrid.Children.Add(daysText);

                // Editable Checkbox
                var editableCheck = new CheckBox { Content = "Editable" };
                editableCheck.SetBinding(CheckBox.IsCheckedProperty,
                    new Binding("Editable") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                Grid.SetColumn(editableCheck, 2);
                rowGrid.Children.Add(editableCheck);

                // Highlight Checkbox
                var highlightCheck = new CheckBox { Content = "Highlight" };
                highlightCheck.SetBinding(CheckBox.IsCheckedProperty,
                    new Binding("Highlight") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                Grid.SetColumn(highlightCheck, 3);
                rowGrid.Children.Add(highlightCheck);

                // Delete Button
                var deleteBtn = new Button
                {
                    Content = "Delete?",
                    Margin = new Thickness(5, 0, 0, 0)
                };
                deleteBtn.Click += (s, e) =>
                {
                    _selectedTreatment.SchedulingTasks.Remove(task);
                    PopulateIndividualTaskPanel();
                };
                Grid.SetColumn(deleteBtn, 4);
                rowGrid.Children.Add(deleteBtn);

                // Add this row grid to the panel
                IndividualTaskPanel.Children.Add(rowGrid);
            }
        }
        private void TaskSetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If you need to track a "selected" task in the combo box:
            _selectedTaskSet = TaskSetComboBox.SelectedItem as TaskSet;
            AddTaskSetButton.IsEnabled = true;
            // Possibly do something here, or you can skip if you want.
        }
        private void OpenTaskSetWindow_Click(object sender, RoutedEventArgs e)
        {
            TaskSetWindow testSetWindow = new TaskSetWindow();
            testSetWindow.ShowDialog();
            load_TaskSets();
            TaskSetComboBox.ItemsSource = TaskSets;
        }

        private void AddModalityButton_Click(object sender, RoutedEventArgs e)
        {
            ModalityClass newModality = new ModalityClass
            {
                Modality = NewModalityNameTextBox.Text,
                Treatments = new List<TreatmentClass>()
            };
            Modalities.Add(newModality);
            _selectedTreatment = null;
            PopulateModalityListedStackPanel();
            NewModalityNameTextBox.Text = "";
        }

        private void AddTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            TreatmentClass newTreatment = new TreatmentClass
            {
                Site = NewTreatmentNameTextBox.Text,
                SchedulingTasks = new List<IndividualTask>()
            };
            _selectedModality.Treatments.Add(newTreatment);
            PopulateTreatmentListedStackPanel();
            NewTreatmentNameTextBox.Text = "";
            IndividualTaskPanel.Children.Clear();
        }

        private void DeleteTreatmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModality == null) return;
            if (_selectedTreatment == null) return;
            _selectedModality.Treatments.Remove(_selectedTreatment);
            PopulateTreatmentListedStackPanel();
        }

        private void AddTaskSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTaskSet == null) return;

            // Example: Add multiple tasks as a new "set"
            foreach (IndividualTask task in _selectedTaskSet.Tasks)
            {
                _selectedTreatment.SchedulingTasks.Add(new IndividualTask { TaskName = task.TaskName,
                    DaysNeeded = task.DaysNeeded, Editable = task.Editable, Highlight=task.Highlight});
            }
            PopulateIndividualTaskPanel();
        }


        private void DeleteModalityButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedModality == null) return;
            Modalities.Remove(_selectedModality);
            PopulateModalityListedStackPanel();
            _selectedModality = null;
            PopulateTreatmentListedStackPanel();
        }

        private void NewModalityNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddModalityButton.IsEnabled = false;
            if (NewModalityNameTextBox.Text.Length == 0) return;
            AddModalityButton.IsEnabled = true;
        }
        private void Save()
        {
            string jsonString = JsonSerializer.Serialize(Modalities, new JsonSerializerOptions { WriteIndented = true });

            // 4) Write to the file
            File.WriteAllText(modalities_filePath, jsonString);
        }
        private void SaveAndExitButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void NewTreatmentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AddTreatmentButton.IsEnabled = false;
            if (NewTreatmentNameTextBox.Text.Length == 0) return;
            AddTreatmentButton.IsEnabled = true;
        }

        private void ValidateAddTaskButton()
        {
            bool hasName = !string.IsNullOrWhiteSpace(NewTaskNameTextBox.Text);

            int parsed;
            bool isInteger = int.TryParse(DaysOffsetTextBox.Text, out parsed);

            AddTaskButton.IsEnabled = hasName && isInteger;
        }
        private void NewTreatmentTaskNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Enable the AddTreatmentTaskButton if the text is not empty and a treatment is selected.
            ValidateAddTaskButton();
        }

        private void ResetAddTreatmentRow()
        {
            NewTaskNameTextBox.Text = "";
            DaysOffsetTextBox.Text = "";
            HighlightCheckBox.IsChecked = false;
            AllowEditCheckBox.IsChecked = false;
            ValidateAddTaskButton();
        }
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var newTask = new IndividualTask
            {
                TaskName = NewTaskNameTextBox.Text,
                DaysNeeded = int.Parse(DaysOffsetTextBox.Text),
                Highlight = HighlightCheckBox.IsChecked ?? false,
                Editable = AllowEditCheckBox.IsChecked ?? false
            };
            _selectedTreatment.SchedulingTasks.Add(newTask);
            ResetAddTreatmentRow();
            PopulateIndividualTaskPanel();
        }

        private void NewTaskNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAddTaskButton();
        }

        private void DaysOffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAddTaskButton();
        }
    }
}
