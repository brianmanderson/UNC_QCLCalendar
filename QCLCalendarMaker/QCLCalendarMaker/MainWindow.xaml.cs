using System;
using System.Collections.Generic;
using System.ComponentModel; // Needed for INotifyPropertyChanged
using System.IO;
using System.Text.Json;
using System.Linq;
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
        public List<ModalityClass> Modalities { get; set; } = new List<ModalityClass>();

        public static List<ModalityClass> ReturnUNCModalities()
        {
            List<ModalityClass> UNCModalities = new List<ModalityClass>()
            {
        // ----------------- 3D -----------------
        new ModalityClass
        {
            Modality = "3D",
            Treatments = new List<TreatmentClass>
            {
                // Palliative => 2 days
                new TreatmentClass
                {
                    Site          = "Palliative",
                    PlanningDays  = 2,
                    PlanningToStart = 2,
                    ContouringDays = 2   // If you want to store a default or omit
                },

                // The "four_days" group => 4 days
                new TreatmentClass { Site = "Lung",     PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Abdomen",  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Rectum",   PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Bladder",  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "CSI",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Breast",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "CW+Nodes",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2}
            }
        },

        // ----------------- IMRT -----------------
        new ModalityClass
        {
            Modality = "IMRT",
            Treatments = new List<TreatmentClass>
            {
                // The five_days group => 5 days
                new TreatmentClass { Site = "Abdomen", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Lung", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "GYN (Intact)", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Anal+Nodes",                    PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Brain",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Esophagus",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Rectum",               PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Bladder",    PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "CSI Tomo",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Hippocampal",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Breast",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Chestwall+Nodes",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Pancreas",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "GYN (Post-Operative)",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Prostate+Nodes",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "Prostate Alone", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                new TreatmentClass { Site = "CSI Tomo", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},

            }
        },

        // ----------------- SBRT -----------------
        new ModalityClass
        {
            Modality = "SBRT",
            Treatments = new List<TreatmentClass>
            {
                // So far we only have "Lung", but your code sets everything to 5 days
                new TreatmentClass { Site = "Lung", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
            }
        }
            };

            return UNCModalities;
        }
        public MainWindow()
        {
            InitializeComponent();
            string filePath = Path.Combine(".", "Modalities.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    Modalities = JsonSerializer.Deserialize<List<ModalityClass>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {filePath}:\n{ex.Message}");
                    // Fallback or set a default
                    Modalities = ReturnUNCModalities();
                }
            }
            else
            {
                // If file doesn't exist, start with default data
                Modalities = ReturnUNCModalities();
            }
            string jsonString = JsonSerializer.Serialize(Modalities, new JsonSerializerOptions { WriteIndented = true });

            // 4) Write to the file
            File.WriteAllText(filePath, jsonString);
            // Set the DataContext to this window so bindings work
            DataContext = this;

            // Initialize default values or ranges
            Today = DateTime.Now;
            Today = new DateTime(Today.Year, Today.Month, Today.Day);
            MainCalendar.DisplayDateStart = Today.AddDays(-60);
            MainCalendar.DisplayDateEnd = Today.AddDays(60);
            QCLButton.Visibility = Visibility.Collapsed;
            QCLContainerStackPanel.Visibility = Visibility.Collapsed;

            PlanningTypeCombo.SelectedIndex = -1;
            // Initialize the first combo box
            PlanningTypeCombo.ItemsSource = Modalities;
            PlanningTypeCombo.DisplayMemberPath = "Modality";

            // Optionally, pre-select the first item or do something else...
            if (Modalities.Any())
            {
                PlanningTypeCombo.SelectedIndex = -1;
            }
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
            TreatmentClass specificPlan = SpecificPlanCombo.SelectedItem as TreatmentClass;
            PlanningDays = specificPlan.PlanningDays;
            DaysToPlanStart = 2;
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
                Background = Brushes.Yellow
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
            if (SpecificPlanCombo.SelectedIndex > -1)
            {
                //QCLButton.IsEnabled = true;
                MDContouringDays = ((TreatmentClass)SpecificPlanCombo.SelectedItem).ContouringDays;
                SimReviewOffsetTextBox.Text = MDContouringDays.ToString();
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
            // Clear out the second combo
            SpecificPlanCombo.ItemsSource = null;

            // Grab the selected ModalityClass object
            var selectedModality = PlanningTypeCombo.SelectedItem as ModalityClass;
            SpecificPlanCombo.IsEnabled = false;
            if (selectedModality != null)
            {
                // Populate the SpecificPlanCombo with the 'Site' from each TreatmentClass
                SpecificPlanCombo.ItemsSource = selectedModality.Treatments;
                SpecificPlanCombo.DisplayMemberPath = "Site";

                // Optionally set a default selection
                if (selectedModality.Treatments.Any())
                {
                    SpecificPlanCombo.SelectedIndex = -1;
                    SpecificPlanCombo.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Click event to generate QCL labels from the selected date.
        /// </summary>
        private void QCLButton_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// TextChanged event for the SIM Review offset TextBox.
        /// If invalid integer, show a warning. If valid, update the labels.
        /// </summary>
        private void SimReviewOffsetTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SpecificPlanCombo.SelectedIndex > -1)
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
