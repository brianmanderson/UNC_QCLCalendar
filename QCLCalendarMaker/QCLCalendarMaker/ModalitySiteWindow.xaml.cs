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
    /// Interaction logic for ModalitySiteWindow.xaml
    /// </summary>
    public partial class ModalitySiteWindow : Window
    {
        public string TaskSetName { get; set; }
        public ModalitySiteWindow(List<ModalityClass> modalities)
        {
            InitializeComponent();
        }

        private void OpenTestSetWindow_Click(object sender, RoutedEventArgs e)
        {
            TaskSetWindow testSetWindow = new TaskSetWindow();
            testSetWindow.ShowDialog();
        }
    }
}
