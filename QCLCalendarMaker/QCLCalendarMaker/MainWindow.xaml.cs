using System;
using System.Collections.Generic;
using System.ComponentModel; // Needed for INotifyPropertyChanged
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Newtonsoft.Json;

namespace QCLCalendarMaker
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DateTime Today;
        private DateTime _selectedDay;
        public ObservableCollection<HolidayRule> Holidays { get; set; } = new ObservableCollection<HolidayRule>();
        public List<DateTime> holiday_datetimes = new List<DateTime>();
        public List<DateTime> holidays_hit = new List<DateTime>();
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

        public ObservableCollection<ModalityClass> Modalities { get; set; } = new ObservableCollection<ModalityClass>();

        public static ObservableCollection<ModalityClass> ReturnUNCModalities()
        {
            ObservableCollection<ModalityClass> UNCModalities = new ObservableCollection<ModalityClass>()
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
                    SchedulingTasks = new List<IndividualTask>
                    {
                        new IndividualTask
                        {
                            TaskName = "SIM Review/Contouring",
                            DaysNeeded = 2,
                            Highlight = false,
                            Editable = true,
                        },
                        new IndividualTask
                        {
                            TaskName = "Planning",
                            DaysNeeded = 4,
                            Highlight = false
                        },
                        new IndividualTask
                        {
                            TaskName = "Recommended EARLIEST start",
                            DaysNeeded = 2,
                            Highlight = true
                        },
                    }
                },

                // The "four_days" group => 4 days
                new TreatmentClass { Site = "Lung",
                    SchedulingTasks = new List<IndividualTask>
                    {
                        new IndividualTask
                        {
                            TaskName = "SIM Review/Contouring",
                            DaysNeeded = 2,
                            Highlight = false
                        },
                        new IndividualTask
                        {
                            TaskName = "Planning",
                            DaysNeeded = 4,
                            Highlight = false
                        },
                        new IndividualTask
                        {
                            TaskName = "Earliest start",
                            DaysNeeded = 2,
                            Highlight = true
                        },
                    }
                },
                //new TreatmentClass { Site = "Abdomen",  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Rectum",   PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Bladder",  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "CSI",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Breast",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "CW+Nodes",  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2}
            }
        },

        // ----------------- IMRT -----------------
        new ModalityClass
        {
            Modality = "IMRT",
            Treatments = new List<TreatmentClass>
            {
                // The five_days group => 5 days
                //new TreatmentClass { Site = "Abdomen", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Lung", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "GYN (Intact)", PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Anal+Nodes",                    PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Brain",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Esophagus",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Rectum",               PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Bladder",    PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "CSI Tomo",                  PlanningDays = 4, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Hippocampal",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Breast",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Chestwall+Nodes",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Pancreas",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "GYN (Post-Operative)",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Prostate+Nodes",                  PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "Prostate Alone", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
                //new TreatmentClass { Site = "CSI Tomo", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},

            }
        },

        // ----------------- SBRT -----------------
        new ModalityClass
        {
            Modality = "SBRT",
            Treatments = new List<TreatmentClass>
            {
                // So far we only have "Lung", but your code sets everything to 5 days
                //new TreatmentClass { Site = "Lung", PlanningDays = 5, PlanningToStart = 2, ContouringDays = 2},
            }
        }
            };

            return UNCModalities;
        }
        string modalities_filePath = Path.Combine(".", "Modalities.json");
        string holidays_filePath = Path.Combine(".", "Holidays.json");
        private void load_Modalities()
        {
            if (File.Exists(modalities_filePath))
            {
                try
                {
                    string json = File.ReadAllText(modalities_filePath);
                    Modalities = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<ModalityClass>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {modalities_filePath}:\n{ex.Message}");
                    // Fallback or set a default
                }
            }
        }
        private void load_Holidays()
        {
            if (File.Exists(holidays_filePath))
            {
                try
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        Formatting = Formatting.Indented
                    };
                    string json = File.ReadAllText(holidays_filePath);
                    Holidays = JsonConvert.DeserializeObject<ObservableCollection<HolidayRule>>(json, settings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {holidays_filePath}:\n{ex.Message}");
                    // Fallback or set a default
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            load_Modalities();
            load_Holidays();
            // 4) Write to the file
            // Set the DataContext to this window so bindings work
            DataContext = this;

            // Initialize default values or ranges
            Today = DateTime.Now;
            Today = new DateTime(Today.Year, Today.Month, Today.Day);
            MainCalendar.DisplayDateStart = Today.AddDays(-60);
            MainCalendar.DisplayDateEnd = Today.AddDays(60);
            QCLStackPanel.Visibility = Visibility.Collapsed;

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
            int start_year = newDate.Year;
            holiday_datetimes = new List<DateTime>();
            foreach (var holiday in Holidays)
            {
                holiday_datetimes.Add(holiday.GetHolidayDate(newDate.Year));
            }
            while (absDays > 0)
            {
                newDate = newDate.AddDays(direction);
                if (newDate.Year != start_year)
                {
                    holiday_datetimes = new List<DateTime>();
                    foreach (var holiday in Holidays)
                    {
                        holiday_datetimes.Add(holiday.GetHolidayDate(newDate.Year));
                    }
                }
                if (newDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }
                else if (newDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    continue;
                }
                else if (holiday_datetimes.Contains(newDate))
                {
                    if (!holidays_hit.Contains(newDate))
                    {
                        holidays_hit.Add(newDate);
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
            holidays_hit = new List<DateTime>();
            TreatmentClass specificPlan = SpecificPlanCombo.SelectedItem as TreatmentClass;
            QCLStackPanel.Visibility = Visibility.Visible;
            // Always clear old labels first
            QCLStackPanel.Children.Clear();

            // Parse the user's offset from the SIM Review TextBox
            // If invalid, fallback to 2 or skip
            int days = 0;
            foreach (IndividualTask t in specificPlan.SchedulingTasks)
            {
                days += t.DaysNeeded;
                DateTime new_day = AddBusinessDays(Today, days);
                Brush background = Brushes.White;
                if (t.Highlight)
                {
                    background = Brushes.Yellow;
                }
                StackPanel taskPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Background = background,
                    Margin = new Thickness(0, 5, 0, 5) // optional margin
                };
                Label newlabel = new Label
                {
                    Content = $"{t.TaskName}: {new_day:M/dd}",
                    Width = 200
                };
                taskPanel.Children.Add(newlabel);
                TextBox daysTextBox = new TextBox
                {
                    Width = 40,
                    Margin = new Thickness(10, 0, 0, 0),
                    DataContext = t,
                    IsEnabled = t.Editable
                };
                Binding daysBinding = new Binding("DaysNeeded")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    NotifyOnSourceUpdated = true
                };
                daysTextBox.SetBinding(TextBox.TextProperty, daysBinding);
                daysTextBox.SourceUpdated += (src, evt) =>
                {
                    // Re-generate the entire list to show the updated offsets, etc.
                    GenerateQCLLabels();
                };
                taskPanel.Children.Add(daysTextBox);
                QCLStackPanel.Children.Add(taskPanel);
            }
            if (holidays_hit.Count > 0)
            {
                string content = $"Holiday encountered, please verify QCLs!";
                foreach (DateTime holiday in holidays_hit)
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
                GenerateQCLLabels();
            }
            else
            {
                QCLStackPanel.Visibility = Visibility.Collapsed;
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

        /// <summary>
        /// TextChanged event for the SIM Review offset TextBox.
        /// If invalid integer, show a warning. If valid, update the labels.
        /// </summary>

        private void EditModalityButton_Click(object sender, RoutedEventArgs e)
        {
            ModalitySiteWindow site_window = new ModalitySiteWindow();
            site_window.ShowDialog();
            load_Modalities();
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

        private void EditHolidayButton_Click(object sender, RoutedEventArgs e)
        {
            HolidayEditor holiday_window = new HolidayEditor();
            holiday_window.ShowDialog();
            load_Holidays();
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
    }
}
