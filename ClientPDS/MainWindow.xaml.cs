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
using MahApps.Metro.Controls;

namespace ClientPDS
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ProcessesViewModel processesViewModelObj;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ProcessesViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Per eseguire eventuali metodi di inizializzazione del viewmodel            
            processesViewModelObj= (ProcessesViewModel)ProcessesViewControl.DataContext;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            processesViewModelObj.closeApplication();            
            //Application.Current.Shutdown();                       
        }



    }
}
