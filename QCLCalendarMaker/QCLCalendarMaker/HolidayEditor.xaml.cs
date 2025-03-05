using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QCLCalendarMaker.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace QCLCalendarMaker
{
    // Base class for all holiday rules.

    public abstract class HolidayRule : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public abstract DateTime GetHolidayDate(int year);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected string GetMonthName(int month)
        {
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        }
        public string GetOccurrenceSuffix(int number)
        {
            string output = "th";
            if (number == 1)
            {
                output = "st";
            }
            else if (number == 2)
            {
                output = "nd";
            }
            else if (number == 3)
            {
                output = "rd";
            }
            return output;
        }
    }

    // For fixed dates (e.g. Christmas on December 25th).
    public class FixedHolidayRule : HolidayRule
    {
        public int Month { get; set; }
        public int Day { get; set; }

        public override DateTime GetHolidayDate(int year)
        {
            return new DateTime(year, Month, Day);
        }
        public string DisplayString => $"{GetMonthName(Month)} {Day}{GetOccurrenceSuffix(Day)}";
    }

    // For holidays defined as the nth occurrence of a weekday (e.g. 3rd Monday in January).
    public class NthWeekdayHolidayRule : HolidayRule
    {
        public int Month { get; set; }
        public DayOfWeek Weekday { get; set; }
        public int Occurrence { get; set; }

        public override DateTime GetHolidayDate(int year)
        {
            DateTime firstOfMonth = new DateTime(year, Month, 1);
            // Calculate offset to first desired weekday.
            int offset = ((int)Weekday - (int)firstOfMonth.DayOfWeek + 7) % 7;
            DateTime firstOccurrence = firstOfMonth.AddDays(offset);
            return firstOccurrence.AddDays(7 * (Occurrence - 1));
        }
        public string DisplayString
        {
            get
            {
                string suffix = GetOccurrenceSuffix(Occurrence);
                return $"{Occurrence}{suffix} {Weekday} of {GetMonthName(Month)}";
            }
        }
    }

    // For holidays defined as the last occurrence of a weekday in a month (e.g. last Monday in May).
    public class LastWeekdayHolidayRule : HolidayRule
    {
        public int Month { get; set; }
        public DayOfWeek Weekday { get; set; }

        public override DateTime GetHolidayDate(int year)
        {
            DateTime lastOfMonth = new DateTime(year, Month, DateTime.DaysInMonth(year, Month));
            while (lastOfMonth.DayOfWeek != Weekday)
            {
                lastOfMonth = lastOfMonth.AddDays(-1);
            }
            return lastOfMonth;
        }
        public string DisplayString => $"Last {Weekday} of {GetMonthName(Month)}";
    }
    /// <summary>
    /// Interaction logic for HolidayEditor.xaml
    /// </summary>
    public partial class HolidayEditor : Window
    {
        string holidays_filePath = System.IO.Path.Combine(".", "Holidays.json");
        public ObservableCollection<HolidayRule> Holidays { get; set; } = new ObservableCollection<HolidayRule>();

        public HolidayEditor()
        {
            InitializeComponent();
            load_Holidays();
            cmbExistingHolidays.ItemsSource = Holidays;
        }

        private void cmbHolidayType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbHolidayType.SelectedItem is ComboBoxItem selectedItem)
            {
                string type = selectedItem.Content.ToString();
                panelFixedDate.Visibility = type == "Fixed Date" ? Visibility.Visible : Visibility.Collapsed;
                panelNthWeekday.Visibility = type == "Nth Weekday" ? Visibility.Visible : Visibility.Collapsed;
                panelLastWeekday.Visibility = type == "Last Weekday" ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void btnAddHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (cmbHolidayType.SelectedItem is ComboBoxItem selectedItem)
            {
                string type = selectedItem.Content.ToString();
                string name = txtHolidayName.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Please enter a holiday name.");
                    return;
                }

                try
                {
                    HolidayRule newHoliday = null;
                    switch (type)
                    {
                        case "Fixed Date":
                            int fixedMonth = int.Parse(txtFixedMonth.Text);
                            int fixedDay = int.Parse(txtFixedDay.Text);
                            newHoliday = new FixedHolidayRule
                            {
                                Name = name,
                                Month = fixedMonth,
                                Day = fixedDay
                            };
                            break;

                        case "Nth Weekday":
                            int nthMonth = int.Parse(txtNthMonth.Text);
                            int occurrence = int.Parse(txtOccurrence.Text);
                            ComboBoxItem weekdayItem = cmbWeekday.SelectedItem as ComboBoxItem;
                            DayOfWeek weekday = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekdayItem.Content.ToString());
                            newHoliday = new NthWeekdayHolidayRule
                            {
                                Name = name,
                                Month = nthMonth,
                                Occurrence = occurrence,
                                Weekday = weekday
                            };
                            break;

                        case "Last Weekday":
                            int lastMonth = int.Parse(txtLastMonth.Text);
                            ComboBoxItem lastWeekdayItem = cmbLastWeekday.SelectedItem as ComboBoxItem;
                            DayOfWeek lastWeekday = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), lastWeekdayItem.Content.ToString());
                            newHoliday = new LastWeekdayHolidayRule
                            {
                                Name = name,
                                Month = lastMonth,
                                Weekday = lastWeekday
                            };
                            break;

                        default:
                            MessageBox.Show("Unsupported holiday type.");
                            break;
                    }

                    if (newHoliday != null)
                    {
                        Holidays.Add(newHoliday);
                        cmbExistingHolidays.Items.Refresh();
                        cmbExistingHolidays.SelectedItem = newHoliday;

                        // Clear out the input fields
                        txtHolidayName.Clear();
                        txtFixedMonth.Clear();
                        txtFixedDay.Clear();
                        txtNthMonth.Clear();
                        txtOccurrence.Clear();
                        txtLastMonth.Clear();
                        // Reset ComboBoxes (optional: set to default index if desired)
                        cmbHolidayType.SelectedIndex = -1;
                        cmbWeekday.SelectedIndex = -1;
                        cmbLastWeekday.SelectedIndex = -1;

                        // Optionally hide panels if no type is selected anymore
                        panelFixedDate.Visibility = Visibility.Collapsed;
                        panelNthWeekday.Visibility = Visibility.Collapsed;
                        panelLastWeekday.Visibility = Visibility.Collapsed;

                        MessageBox.Show("Holiday added successfully!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding holiday: " + ex.Message);
                }
            }
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
            Close();
        }

        private void Save()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    Formatting = Formatting.Indented
                };
                string jsonString = JsonConvert.SerializeObject(Holidays, settings);
                File.WriteAllText(holidays_filePath, jsonString);

                MessageBox.Show("Holidays saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving holidays: " + ex.Message);
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

        private void btnDeleteHoliday_Click(object sender, RoutedEventArgs e)
        {
            if (cmbExistingHolidays.SelectedItem is HolidayRule selectedHoliday)
            {
                if (MessageBox.Show($"Are you sure you want to delete the holiday '{selectedHoliday.Name}'?",
                                    "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Holidays.Remove(selectedHoliday);
                    cmbExistingHolidays.Items.Refresh();
                    MessageBox.Show("Holiday deleted successfully.");
                }
            }
            else
            {
                MessageBox.Show("Please select a holiday to delete.");
            }
        }

        private void cmbExistingHolidays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbExistingHolidays.SelectedItem is HolidayRule selectedHoliday)
            {
                // Populate the Holiday Name field
                txtHolidayName.Text = selectedHoliday.Name;

                // Determine the type of holiday rule and populate accordingly
                if (selectedHoliday is FixedHolidayRule fixedHoliday)
                {
                    cmbHolidayType.SelectedIndex = 0; // "Fixed Date"
                    panelFixedDate.Visibility = Visibility.Visible;
                    panelNthWeekday.Visibility = Visibility.Collapsed;
                    panelLastWeekday.Visibility = Visibility.Collapsed;
                    txtFixedMonth.Text = fixedHoliday.Month.ToString();
                    txtFixedDay.Text = fixedHoliday.Day.ToString();
                }
                else if (selectedHoliday is NthWeekdayHolidayRule nthHoliday)
                {
                    cmbHolidayType.SelectedIndex = 1; // "Nth Weekday"
                    panelFixedDate.Visibility = Visibility.Collapsed;
                    panelNthWeekday.Visibility = Visibility.Visible;
                    panelLastWeekday.Visibility = Visibility.Collapsed;
                    txtNthMonth.Text = nthHoliday.Month.ToString();
                    txtOccurrence.Text = nthHoliday.Occurrence.ToString();

                    // Select the correct weekday in the combo box
                    foreach (ComboBoxItem item in cmbWeekday.Items)
                    {
                        if (item.Content.ToString() == nthHoliday.Weekday.ToString())
                        {
                            cmbWeekday.SelectedItem = item;
                            break;
                        }
                    }
                }
                else if (selectedHoliday is LastWeekdayHolidayRule lastHoliday)
                {
                    cmbHolidayType.SelectedIndex = 2; // "Last Weekday"
                    panelFixedDate.Visibility = Visibility.Collapsed;
                    panelNthWeekday.Visibility = Visibility.Collapsed;
                    panelLastWeekday.Visibility = Visibility.Visible;
                    txtLastMonth.Text = lastHoliday.Month.ToString();

                    // Select the correct weekday in the last weekday combo box
                    foreach (ComboBoxItem item in cmbLastWeekday.Items)
                    {
                        if (item.Content.ToString() == lastHoliday.Weekday.ToString())
                        {
                            cmbLastWeekday.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

    }
}
