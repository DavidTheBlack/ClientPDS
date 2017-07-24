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

namespace ClientPDS
{
    /// <summary>
    /// Logica di interazione per ProcessesView.xaml
    /// </summary>
    public partial class ProcessesView : UserControl
    {
        private ProcessesViewModel _viewModel;
        //public ProcessesViewModel ViewModel
        //{
        //    set {
        //        _viewModel = value;                
        //    }
        //    private get { return _viewModel; }
        //}

        public ProcessesView()
        {
            InitializeComponent();
            _viewModel = new ProcessesViewModel();
            this.DataContext = _viewModel;
            
        }

        private void ConnectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.connectedToServer)
            {
                StartConnection();             
            }
            else
            {
                StopConnection();
            }
        }

        /// <summary>
        /// Handle the changing connection state event from the processesViewModel adapting the gui
        /// </summary>
        public void ConnectionStateChangeHandler(object source, EventArgs e)
        {
            if (_viewModel.connectedToServer)
            {                
                ConnectionBtn.Content = "Disconnect";
                ServerIpTxt.IsEnabled = false;
            }
            else
            {
                ConnectionBtn.Content = "Connect";
                ServerIpTxt.IsEnabled = true;

            }

        }        

        private void StartConnection()
        {
            if (!_viewModel.StartNetworkTask())            
                MessageBox.Show("Non è stato possibile stabilire la connessione con il server.\n" + _viewModel.Log);      
        }

        private void StopConnection()
        {
            _viewModel.CloseConnection();
        }

}
    
    


}
