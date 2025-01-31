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
        public ModalitySiteWindow(List<ModalityClass> modalities)
        {
            InitializeComponent();
            load_TaskSets();
            load_Modalities();
            PopulateModalityListedStackPanel();
        }
        private void PopulateModalityListedStackPanel()
        {
            ModalityListedStackPanel.Children.Clear();
            DeleteModalityButton.IsEnabled = false;
            foreach (ModalityClass modalityItem in Modalities)
            {
                // Create a TextBox for the Modality name
                var txt = new TextBox
                {
                    Width = 100,
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
                txt.GotFocus += (s, e) =>
                {
                    _selectedModality = modalityItem;
                    txt.Background = Brushes.LightGray;
                    PopulateTreatmentListedStackPanel();
                    DeleteModalityButton.IsEnabled = true;
                };
                txt.LostFocus += (s, e) =>
                {
                    txt.Background = Brushes.White;
                };

                ModalityListedStackPanel.Children.Add(txt);
            }
        }
        private void PopulateTreatmentListedStackPanel()
        {
            TreatmentListedStackPanel.Children.Clear();
            if (_selectedModality == null) return;
            if (_selectedModality?.Treatments == null)
                return;

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
                txt.GotFocus += (s, e) =>
                {
                    _selectedTreatment = treatment;
                    txt.Background = Brushes.LightGray;
                    PopulateTaskComboBox();
                    PopulateIndividualTaskPanel();
                };
                txt.LostFocus += (s, e) =>
                {
                    txt.Background = Brushes.White;
                };

                TreatmentListedStackPanel.Children.Add(txt);
            }
        }
        private void PopulateTaskComboBox()
        {
            TaskSetComboBox.ItemsSource = null;

            if (_selectedTreatment?.SchedulingTasks == null)
                return;

            TaskSetComboBox.ItemsSource = _selectedTreatment.SchedulingTasks;
            // Because we set DisplayMemberPath="TaskName" in XAML,
            // it will show each task's TaskName
        }
        private void PopulateIndividualTaskPanel()
        {
            IndividualTaskPanel.Children.Clear();
            if (_selectedTreatment?.SchedulingTasks == null) return;

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
                    PopulateTaskComboBox();
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
            var selectedTask = TaskSetComboBox.SelectedItem as IndividualTask;
            // Possibly do something here, or you can skip if you want.
        }
        private void OpenTaskSetWindow_Click(object sender, RoutedEventArgs e)
        {
            TaskSetWindow testSetWindow = new TaskSetWindow();
            testSetWindow.ShowDialog();
            load_TaskSets();
        }

        private void AddModalityButton_Click(object sender, RoutedEventArgs e)
        {
            var newModality = new ModalityClass
            {
                Modality = "New Modality",
                Treatments = new List<TreatmentClass>()
            };
            Modalities.Add(newModality);
            PopulateModalityListedStackPanel();
        }

        private void DeleteTaskSetButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddTreatmentButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteTreatmentButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddTaskSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTreatment == null) return;

            // Example: Add multiple tasks as a new "set"
            _selectedTreatment.SchedulingTasks.Add(new IndividualTask { TaskName = "New Task 1", DaysNeeded = 2 });
            _selectedTreatment.SchedulingTasks.Add(new IndividualTask { TaskName = "New Task 2", DaysNeeded = 5 });

            PopulateTaskComboBox();
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
    }
}
