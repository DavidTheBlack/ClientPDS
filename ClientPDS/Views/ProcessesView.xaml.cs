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
using ClientPDS.Views;

namespace ClientPDS
{
    /// <summary>
    /// Logica di interazione per ProcessesView.xaml
    /// </summary>
    public partial class ProcessesView : UserControl
    {
        private ProcessesViewModel _viewModel;
       

        public ProcessesView()
        {
            InitializeComponent();
            _viewModel = new ProcessesViewModel();
            this.DataContext = _viewModel;
            EnableHotkeySwitch.IsCheckedChanged += EnableHotkeySwitch_IsCheckedChanged;
            
        }

        private void EnableHotkeySwitch_IsCheckedChanged(object sender, EventArgs e)
        {
            if (EnableHotkeySwitch.IsChecked==true)
            {
                //registro agli eventi
                this.PreviewKeyDown += HandleOnPreviewKeyDown;
                this.PreviewKeyUp += HandleOnPreviewKeyUp;
            }
            else
            {
                //deregistro dagli eventi
                this.PreviewKeyDown -= HandleOnPreviewKeyDown;
                this.PreviewKeyUp -= HandleOnPreviewKeyUp;
            }
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

        private void ShortcutBtn_Click(object sender, RoutedEventArgs e)
        {
            ShortcutWindow sw = new ShortcutWindow(_viewModel);
            sw.DataContext = _viewModel;
            sw.Show();
        }

        public void HandleOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            string commandStr = string.Empty;

            //Controllo se il tasto è un modificatore
            //se lo è esco e non faccio nulla

            //se no nè un modificatore 
            //prelevo il tasto
            //controllo i modificatori premuti
            //invio il dato al server
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);


            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    {
                        shiftSwitch.IsChecked = true;
                        e.Handled = true;
                        break;
                    }
                case Key.LeftAlt:
                case Key.RightAlt:
                    //case Key.System:
                    {
                        altSwitch.IsChecked = true;
                        e.Handled = true;
                        break;
                    }
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    {
                        ctrlSwitch.IsChecked = true;
                        e.Handled = true;
                        break;
                    }
                default:
                    break;
            }

            if (!e.Handled)
            {
                commandStr = "dw/" + Keyboard.Modifiers.GetHashCode() + "/" + KeyInterop.VirtualKeyFromKey(key);
                _viewModel.SendKeyboardCom(commandStr);
            }
            e.Handled = true;
        }

        public void HandleOnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            string commandStr = string.Empty;
            string modifiers = Keyboard.Modifiers.GetHashCode().ToString();
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            switch (key)
            {

                case Key.LeftShift:
                case Key.RightShift:
                    {
                        shiftSwitch.IsChecked = false;
                        e.Handled = true;
                        break;
                    }
                case Key.LeftAlt:
                case Key.RightAlt:
                    //case Key.System:
                    {
                        altSwitch.IsChecked = false;
                        e.Handled = true;
                        break;
                    }
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    {
                        ctrlSwitch.IsChecked = false;
                        e.Handled = true;
                        break;
                    }
                default:
                    break;
            }

            if (!e.Handled)
            {
                commandStr = "up/" + Keyboard.Modifiers.GetHashCode() + "/" + KeyInterop.VirtualKeyFromKey(key);
                _viewModel.SendKeyboardCom(commandStr);
            }

            e.Handled = true;
        }


    }
    
    


}
