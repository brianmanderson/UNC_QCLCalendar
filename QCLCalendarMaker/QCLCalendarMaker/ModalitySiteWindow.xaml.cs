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
using System.IO;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace QCLCalendarMaker
{
    /// <summary>
    /// Interaction logic for ModalitySiteWindow.xaml
    /// </summary>
    public partial class ModalitySiteWindow : Window
    {
        public string TaskSetName { get; set; }
        public string taskset_filePath = Path.Combine(".", "TaskkSets.json");
        public ObservableCollection<TaskSet> TaskSets = new ObservableCollection<TaskSet>();
        private void load_TaskSets()
        {
            if (File.Exists(taskset_filePath))
            {
                try
                {
                    string json = File.ReadAllText(taskset_filePath);
                    TaskSets = JsonSerializer.Deserialize<ObservableCollection<TaskSet>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading {taskset_filePath}:\n{ex.Message}");
                    // Fallback or set a default
                }
            }
        }
        public ModalitySiteWindow(List<ModalityClass> modalities)
        {
            InitializeComponent();
            load_TaskSets();
        }

        private void OpenTaskSetWindow_Click(object sender, RoutedEventArgs e)
        {
            TaskSetWindow testSetWindow = new TaskSetWindow();
            testSetWindow.ShowDialog();
            load_TaskSets();
        }
    }
}
