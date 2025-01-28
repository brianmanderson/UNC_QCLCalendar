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
    /// <summary>
    /// Interaction logic for TaskSetWindow.xaml
    /// </summary>
    public partial class TaskSetWindow : Window
    {
        public List<TaskSet> TaskSets = new List<TaskSet>();
        public TaskSetWindow()
        {
            InitializeComponent();
            TaskSetComboBox.SelectedIndex = -1;
            // Initialize the first combo box
            TaskSetComboBox.ItemsSource = TaskSets;
            TaskSetComboBox.DisplayMemberPath = "TaskSetName";
        }
    }
}
