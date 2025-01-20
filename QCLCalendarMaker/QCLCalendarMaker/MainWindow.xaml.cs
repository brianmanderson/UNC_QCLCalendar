using System;
using System.Collections.Generic;
using System.ComponentModel; // Needed for INotifyPropertyChanged
using System.Windows;
using System.Windows.Controls;

namespace QCLCalendarMaker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime _selectedDay;
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
            SelectedDay = today;

            // Initialize the first combo box
            PlanningTypeCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// Add N business days (skipping weekends) to a given date.
        /// </summary>
        private DateTime AddBusinessDays(DateTime startDate, int daysToAdd)
        {
            int direction = Math.Sign(daysToAdd);
            int absDays = Math.Abs(daysToAdd);

            DateTime newDate = startDate;
            while (absDays > 0)
            {
                newDate = newDate.AddDays(direction);

                if (newDate.DayOfWeek != DayOfWeek.Saturday &&
                    newDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    absDays--;
                }
            }
            return newDate;
        }

        /// <summary>
        /// Rebuilds the QCLStackPanel labels based on the current offsets and SelectedDay.
        /// </summary>
        private void GenerateQCLLabels()
        {
            QCLContainerStackPanel.Visibility = Visibility.Visible;
            // Always clear old labels first
            QCLStackPanel.Children.Clear();

            // Parse the user's offset from the SIM Review TextBox
            // If invalid, fallback to 2 or skip
            int simOffset;
            if (!int.TryParse(SimReviewOffsetTextBox.Text, out simOffset))
            {
                // If it's invalid, we should already have a warning visible,
                // so just return to avoid generating confusing labels.
                return;
            }

            // 1) "SIM Review" and "MD Contour QCL" on simOffset days from SelectedDay
            var mdContoursDate = AddBusinessDays(SelectedDay, simOffset);

            var simReviewLabel = new Label
            {
                Content = $"SIM Review: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(simReviewLabel);

            var mdContoursQCLLabel = new Label
            {
                Content = $"MD Contour QCL: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(mdContoursQCLLabel);

            // 2) "DOS Tx Planning" is 8 business days from SelectedDay (unchanged).
            var treatmentDate = AddBusinessDays(SelectedDay, 8);
            var treatmentLabel = new Label
            {
                Content = $"DOS Tx Planning: {treatmentDate:M/dd}"
            };
            QCLStackPanel.Children.Add(treatmentLabel);
        }

        // --- Event Handlers ---

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SpecificPlanCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the user picks any site (index != 0), enable QCL button
            if (SpecificPlanCombo.SelectedIndex != 0)
            {
                QCLButton.IsEnabled = true;
            }
            else
            {
                QCLButton.IsEnabled = false;
                QCLContainerStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void PlanningTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Clear and reset
            SpecificPlanCombo.Items.Clear();
            SpecificPlanCombo.IsEnabled = false;
            QCLContainerStackPanel.Visibility = Visibility.Collapsed;
            QCLButton.IsEnabled = false;

            // Decide which list to show
            List<string> specificComboOptions = new List<string>();
            bool hasOptions = false;

            if (selectedItem == "3D")
            {
                specificComboOptions = new List<string>
                {
                    "Select one", "Palliative", "Lung", "Abdomen", "Rectum",
                    "Bladder", "CSI", "Breast", "CW+Nodes"
                };
                hasOptions = true;
            }
            else if (selectedItem == "IMRT")
            {
                specificComboOptions = new List<string>
                {
                    "Select one", "Abdomen", "Lung Non-SBRT", "GYN (intact)",
                    "Head and Neck", "Anal+Nodes", "Brain",
                    "Esophagus", "Rectum", "Bladder",
                    "Hippocampal Sparing Brain", "Breast", "CW+Nodes",
                    "Pancreas", "GYN Post-op", "Prostate (w w/o Nodes)",
                    "CSI Tomo"
                };
                hasOptions = true;
            }
            else if (selectedItem == "SBRT")
            {
                specificComboOptions = new List<string>
                {
                    "Select one", "Lung SBRT"
                };
                hasOptions = true;
            }

            if (hasOptions)
            {
                SpecificPlanCombo.IsEnabled = true;
                foreach (var option in specificComboOptions)
                {
                    SpecificPlanCombo.Items.Add(option);
                }
                SpecificPlanCombo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Click event to generate QCL labels from the selected date.
        /// </summary>
        private void QCLButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateQCLLabels();
        }

        /// <summary>
        /// TextChanged event for the SIM Review offset TextBox.
        /// If invalid integer, show a warning. If valid, update the labels.
        /// </summary>
        private void SimReviewOffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (QCLButton.IsEnabled)
            {
                // Try to parse the new value
                if (int.TryParse(SimReviewOffsetTextBox.Text, out _))
                {
                    // Valid integer, hide warning
                    WarningLabel.Visibility = Visibility.Collapsed;

                    // If QCLButton is enabled (i.e., user has selected a site),
                    // go ahead and regenerate the labels immediately
                    GenerateQCLLabels();
                }
                else
                {
                    // Invalid integer, show warning and clear labels
                    WarningLabel.Content = "Please enter a valid integer offset.";
                    WarningLabel.Visibility = Visibility.Visible;
                    QCLStackPanel.Children.Clear();
                }
            }
        }
    }
}
