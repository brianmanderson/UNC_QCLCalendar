using System;
using System.Collections.Generic;
using System.ComponentModel; // <-- Needed for INotifyPropertyChanged
using System.Windows;
using System.Windows.Controls;

namespace QCLCalendarMaker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // Private backing field
        private DateTime _selectedDay;

        // Public property for data binding
        public DateTime SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (_selectedDay != value)
                {
                    _selectedDay = value;
                    OnPropertyChanged(nameof(SelectedDay));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext to this window so bindings work
            this.DataContext = this;

            // Initialize default values or ranges
            DateTime today = DateTime.Today.AddDays(15);
            MainCalendar.DisplayDateStart = today.AddDays(-30);
            MainCalendar.DisplayDateEnd = today.AddDays(30);

            // For demonstration, let's set SelectedDay to "today"
            SelectedDay = today;

            // Initialize your combo box
            PlanningTypeCombo.SelectedIndex = 0;
        }

        // Implement the INotifyPropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PlanningTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Clear out old items from the second ComboBox
            SpecificPlanCombo.Items.Clear();

            if (selectedItem == "3D")
            {
                SpecificPlanCombo.IsEnabled = true;

                var threeDoptions = new List<string>
                {
                    "Select one", "Palliative", "Lung", "Abdomen", "Rectum",
                    "Bladder", "CSI", "Breast", "CW+Nodes"
                };
                foreach (var option in threeDoptions)
                {
                    SpecificPlanCombo.Items.Add(option);
                }
                SpecificPlanCombo.SelectedIndex = 0;
            }
            else if (selectedItem == "IMRT")
            {
                SpecificPlanCombo.IsEnabled = true;

                var imrtOptions = new List<string>
                {
                    "Select one", "Abdomen", "Lung Non-SBRT", "GYN (intact)",
                    "Head and Neck", "Anal+Nodes", "Brain",
                    "Esophagus", "Rectum", "Bladder",
                    "Hippocampal Sparing Brain", "Breast", "CW+Nodes",
                    "Pancreas", "GYN Post-op", "Prostate (w w/o Nodes)",
                    "Lung SBRT", "CSI Tomo"
                };

                foreach (var option in imrtOptions)
                {
                    SpecificPlanCombo.Items.Add(option);
                }
                SpecificPlanCombo.SelectedIndex = 0;
            }
            else
            {
                SpecificPlanCombo.IsEnabled = false;
            }
        }
    }
}
