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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QCLCalendarMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DateTime SelectedDay;
        public MainWindow()
        {
            InitializeComponent();

            // For a quick demo binding, you can set the DataContext to this Window itself
            this.DataContext = this;

            // Alternatively, you could set your DisplayDate range directly in code behind:
            DateTime today = DateTime.Today.AddDays(15);
            MainCalendar.DisplayDateStart = today.AddDays(-30);
            MainCalendar.DisplayDateEnd = today.AddDays(30);
            MainCalendar.DisplayDate = today;
            PlanningTypeCombo.SelectedIndex = 0;
        }
        private void PlanningTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected text from the first ComboBox
            var selectedItem = (PlanningTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

            // Clear out any old items from second ComboBox

            if (selectedItem == "3D")
            {
                // Enable second ComboBox
                SpecificPlanCombo.IsEnabled = true;

                // Add items for 3D
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
                // Enable second ComboBox
                SpecificPlanCombo.IsEnabled = true;

                // Add items for IMRT
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
                // If user selected "Pick an option" or anything else, disable second ComboBox
                SpecificPlanCombo.IsEnabled = false;
            }
        }
        // Bindable properties for the Calendar

    }
}
