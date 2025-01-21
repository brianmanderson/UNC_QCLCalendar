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
        public int days_to_contour = 2;
        public double days_to_plan = 1.5;
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
                QCLButton.IsEnabled = false;
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
                Content = $"MD Contours: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(mdContoursLabel);

            // 2) "Physics Pre-MD": 4 business days after SelectedDay
            var physicsPreMdDate = AddBusinessDays(SelectedDay, 4);
            var physicsPreMdLabel = new Label
            {
                Content = $"Physics Pre-MD: {physicsPreMdDate:M/dd}"
            };
            QCLStackPanel.Children.Add(physicsPreMdLabel);

            // 3) "Physics Pre-Tx": 6 business days after SelectedDay
            var physicsPreTxDate = AddBusinessDays(SelectedDay, 6);
            var physicsPreTxLabel = new Label
            {
                Content = $"Physics Pre-Tx: {physicsPreTxDate:M/dd}"
            };
            QCLStackPanel.Children.Add(physicsPreTxLabel);

            // 4) "Treatment": 8 business days after SelectedDay
            var treatmentDate = AddBusinessDays(SelectedDay, 8);
            var treatmentLabel = new Label
            {
                Content = $"Treatment: {treatmentDate:M/dd}"
            };
            QCLStackPanel.Children.Add(treatmentLabel);
        }
    }
}
