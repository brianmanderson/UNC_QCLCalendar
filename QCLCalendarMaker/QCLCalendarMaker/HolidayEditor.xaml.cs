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
using System.Windows.Shapes;

namespace QCLCalendarMaker
{
    // Base class for all holiday rules.
    public abstract class HolidayRule
    {
        public string Name { get; set; }
        public abstract DateTime GetHolidayDate(int year);
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
    }
    /// <summary>
    /// Interaction logic for HolidayEditor.xaml
    /// </summary>
    public partial class HolidayEditor : Window
    {
        // A list to hold the user-defined holidays.
        public List<HolidayRule> Holidays { get; private set; } = new List<HolidayRule>();

        public HolidayEditor()
        {
            InitializeComponent();
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
                    switch (type)
                    {
                        case "Fixed Date":
                            int fixedMonth = int.Parse(txtFixedMonth.Text);
                            int fixedDay = int.Parse(txtFixedDay.Text);
                            FixedHolidayRule fixedRule = new FixedHolidayRule
                            {
                                Name = name,
                                Month = fixedMonth,
                                Day = fixedDay
                            };
                            Holidays.Add(fixedRule);
                            break;

                        case "Nth Weekday":
                            int nthMonth = int.Parse(txtNthMonth.Text);
                            int occurrence = int.Parse(txtOccurrence.Text);
                            ComboBoxItem weekdayItem = cmbWeekday.SelectedItem as ComboBoxItem;
                            DayOfWeek weekday = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), weekdayItem.Content.ToString());
                            NthWeekdayHolidayRule nthRule = new NthWeekdayHolidayRule
                            {
                                Name = name,
                                Month = nthMonth,
                                Occurrence = occurrence,
                                Weekday = weekday
                            };
                            Holidays.Add(nthRule);
                            break;

                        case "Last Weekday":
                            int lastMonth = int.Parse(txtLastMonth.Text);
                            ComboBoxItem lastWeekdayItem = cmbLastWeekday.SelectedItem as ComboBoxItem;
                            DayOfWeek lastWeekday = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), lastWeekdayItem.Content.ToString());
                            LastWeekdayHolidayRule lastRule = new LastWeekdayHolidayRule
                            {
                                Name = name,
                                Month = lastMonth,
                                Weekday = lastWeekday
                            };
                            Holidays.Add(lastRule);
                            break;

                        default:
                            MessageBox.Show("Unsupported holiday type.");
                            break;
                    }

                    MessageBox.Show("Holiday added successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding holiday: " + ex.Message);
                }
            }
        }

        private void txtHolidayName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
