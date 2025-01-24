using System;
using System.Collections.Generic;
using System.ComponentModel; // Needed for INotifyPropertyChanged
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QCLCalendarMaker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime Today;
        private DateTime _selectedDay;
        public List<DateTime> holidays = new List<DateTime>();
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

        private int _MDContouringDays;
        public int MDContouringDays
        {
            get => _MDContouringDays;
            set
            {
                if (_MDContouringDays != value)
                {
                    _MDContouringDays = value;
                    OnPropertyChanged(nameof(_MDContouringDays));
                }
            }
        }
        private int _PlanningDays;
        public int PlanningDays
        {
            get => _PlanningDays;
            set
            {
                if (_PlanningDays != value)
                {
                    _PlanningDays = value;
                    OnPropertyChanged(nameof(_PlanningDays));
                }
            }
        }
        public int DaysToPlanStart;
        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext to this window so bindings work
            this.DataContext = this;

            // Initialize default values or ranges
            Today = DateTime.Now;
            Today = new DateTime(Today.Year, Today.Month, Today.Day);
            MainCalendar.DisplayDateStart = Today.AddDays(-60);
            MainCalendar.DisplayDateEnd = Today.AddDays(60);
            MDContouringDays = 2;
            PlanningDays = 4;
            DaysToPlanStart = 3;
            QCLButton.Visibility = Visibility.Collapsed;


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
            List<DateTime> US_holidays = USHolidays.GetFederalHolidays(startDate.Year);
            while (absDays > 0)
            {
                newDate = newDate.AddDays(direction);
                if (newDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                else if (newDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    continue;
                }
                else if (US_holidays.Contains(newDate))
                {
                    if (!holidays.Contains(newDate))
                    {
                        holidays.Add(newDate);
                    }
                    continue;
                }
                absDays--;
            }
            return newDate;
        }

        /// <summary>
        /// Rebuilds the QCLStackPanel labels based on the current offsets and SelectedDay.
        /// </summary>
        private void GenerateQCLLabels()
        {
            holidays = new List<DateTime>();
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var specificPlan = SpecificPlanCombo.SelectedItem.ToString();
            DaysToPlanStart = 3;
            if (selectedItem == "3D")
            {
                DaysToPlanStart = 2;
                List<string> four_days = new List<string>() { "Lung", "Abdomen", "Rectum", "Bladder" };
                if (specificPlan == "Palliative")
                {
                    PlanningDays = 2;
                }
                else if (four_days.Contains(specificPlan))
                {
                    PlanningDays = 4;
                }
                else
                {
                    PlanningDays = 5;
                }
            }
            else if (selectedItem == "IMRT")
            {
                PlanningDays = 4;
                List<string> five_days = new List<string>() { "Hippocampal Sparing Brain", "Breast", "CW+Nodes",
                    "Pancreas", "GYN Post-op", "Prostate (w w/o Nodes)",
                    "CSI Tomo"};
                if (five_days.Contains(specificPlan))
                {
                    PlanningDays = 5;
                }
            }
            else if (selectedItem == "SBRT")
            {
                // So far just one SBRT, for lung, option
                PlanningDays = 5;
            }
            PlanningDaysLabel.Content = PlanningDays.ToString();
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
            MDContouringDays = simOffset;

            // 1) "SIM Review" and "MD Contour QCL" on simOffset days from SelectedDay
            DateTime mdContoursDate = AddBusinessDays(Today, MDContouringDays);

            Label simReviewLabel = new Label
            {
                Content = $"SIM Review: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(simReviewLabel);

            Label mdContoursQCLLabel = new Label
            {
                Content = $"MD Contour QCL: {mdContoursDate:M/dd}"
            };
            QCLStackPanel.Children.Add(mdContoursQCLLabel);

            // 2) "DOS Tx Planning" is 8 business days from SelectedDay (unchanged).
            DateTime treatmentDate = AddBusinessDays(Today, MDContouringDays + PlanningDays);
            Label treatmentLabel = new Label
            {
                Content = $"DOS Tx Planning: {treatmentDate:M/dd}"
            };
            QCLStackPanel.Children.Add(treatmentLabel);
            DateTime startDate = AddBusinessDays(Today, MDContouringDays + PlanningDays + DaysToPlanStart);
            Label StartReccomendationLabel = new Label
            {
                Content = $"Recommended EARLIEST Start: {startDate:M/dd}",
                Background= Brushes.Yellow
            };
            QCLStackPanel.Children.Add(StartReccomendationLabel);
            if (holidays.Count > 0)
            {
                string content = $"Holiday encountered, please verify QCLs!";
                foreach (DateTime holiday in holidays)
                {
                    content += $"\n{holiday:M/dd}";
                }
                Label HolidayLabel = new Label
                {
                    Content = content,
                    Background = Brushes.Red
                };
                QCLStackPanel.Children.Add(HolidayLabel);
            }
        }

        // --- Event Handlers ---

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void SpecificPlanCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If the user picks any site (index != 0), enable QCL button
            if (SpecificPlanCombo.SelectedIndex > 0)
            {
                //QCLButton.IsEnabled = true;
                MDContouringDays = 2;
                GenerateQCLLabels();
            }
            else
            {
                //QCLButton.IsEnabled = false;
                QCLContainerStackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void PlanningTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
            MDContouringDays = 2;
            SimReviewOffsetTextBox.Text = MDContouringDays.ToString();
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
                    "Palliative", "Lung", "Abdomen", "Rectum", "Bladder", "CSI", "Breast", "CW+Nodes"
                };
                hasOptions = true;
            }
            else if (selectedItem == "IMRT")
            {
                specificComboOptions = new List<string>
                {
                    "Abdomen", "Lung IMRT", "GYN (intact)",
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
                    "Lung SBRT"
                };
                hasOptions = true;
            }

            if (hasOptions)
            {
                specificComboOptions.Sort();
                SpecificPlanCombo.IsEnabled = true;
                specificComboOptions.Insert(0, "Select one");
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
            if (SpecificPlanCombo.SelectedIndex > 0)
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
