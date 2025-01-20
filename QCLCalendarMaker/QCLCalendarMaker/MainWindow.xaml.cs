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
            DateTime today = DateTime.Now;
            MainCalendar.DisplayDateStart = today.AddDays(-30);
            MainCalendar.DisplayDateEnd = today.AddDays(30);

            // For demonstration, let's set SelectedDay to "today"
            SelectedDay = today;

            // Initialize your combo box
            PlanningTypeCombo.SelectedIndex = 0;
        }
        private DateTime AddBusinessDays(DateTime startDate, int daysToAdd)
        {
            // 'direction' is +1 for future, -1 for past
            int direction = Math.Sign(daysToAdd);
            int absDays = Math.Abs(daysToAdd);

            DateTime newDate = startDate;
            while (absDays > 0)
            {
                newDate = newDate.AddDays(direction);
                // If it's NOT Saturday or Sunday, count it
                if (newDate.DayOfWeek != DayOfWeek.Saturday && newDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    absDays--;
                }
            }
            return newDate;
        }

        // Implement the INotifyPropertyChanged event
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SpecificPlanCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpecificPlanCombo.SelectedIndex != 0)
            {
                QCLButton.IsEnabled = true;
            }
            else
            {
                QCLButton.IsEnabled = false;
            }
        }

        private void PlanningTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Clear out old items from the second ComboBox
            SpecificPlanCombo.Items.Clear();
            List<string> specificComboOptions = new List<string> { };
            bool selected = false;

            SpecificPlanCombo.IsEnabled = false;
            QCLButton.IsEnabled = false;

            if (selectedItem == "3D")
            {
                selected = true;
                specificComboOptions = new List<string>
                {
                    "Select one", "Palliative", "Lung", "Abdomen", "Rectum",
                    "Bladder", "CSI", "Breast", "CW+Nodes"
                };
            }
            else if (selectedItem == "IMRT")
            {
                selected = true;
                specificComboOptions = new List<string>
                {
                    "Select one", "Abdomen", "Lung Non-SBRT", "GYN (intact)",
                    "Head and Neck", "Anal+Nodes", "Brain",
                    "Esophagus", "Rectum", "Bladder",
                    "Hippocampal Sparing Brain", "Breast", "CW+Nodes",
                    "Pancreas", "GYN Post-op", "Prostate (w w/o Nodes)",
                    "CSI Tomo"
                };
            }
            else if (selectedItem == "SBRT")
            {
                selected = true;
                specificComboOptions = new List<string>
                {
                    "Select one", "Lung SBRT"
                };

            }
            if (selected)
            {
                SpecificPlanCombo.IsEnabled = true;
                foreach (var option in specificComboOptions)
                {
                    SpecificPlanCombo.Items.Add(option);
                }
                SpecificPlanCombo.SelectedIndex = 0;
            }
        }

        private void QCLButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear out anything that was there before
            QCLStackPanel.Children.Clear();

            // Example: We’ll show how each label might have a different offset
            // from the SelectedDay. You can adjust these offsets as you like.

            // 1) "MD Contours": 2 business days after SelectedDay
            var mdContoursDate = AddBusinessDays(SelectedDay, 2);
            var mdContoursLabel = new Label
            {
                Content = $"SIM Review: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(mdContoursLabel);

            var mdContoursQCLLabel = new Label
            {
                Content = $"MD Contour QCL: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(mdContoursQCLLabel);


            // 2) "Treatment": 8 business days after SelectedDay
            var treatmentDate = AddBusinessDays(SelectedDay, 8);
            var treatmentLabel = new Label
            {
                Content = $"DOS Tx Planning: {treatmentDate:M/dd}"
            };
            QCLStackPanel.Children.Add(treatmentLabel);
        }
    }
}
