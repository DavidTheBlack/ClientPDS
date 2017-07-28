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
using MahApps.Metro.Controls;


namespace ClientPDS.Views
{





    /// <summary>
    /// Logica di interazione per ShortcutWindow.xaml
    /// </summary>
    public partial class ShortcutWindow : MetroWindow
    {


        List<Key> _pressedKeys = new List<Key>();
        List<ModifierKeys> _pressedModifiers = new List<ModifierKeys>();
        string shortcutSeq = string.Empty;
        string modSeq = string.Empty;
        ProcessesViewModel processesViewModelObj;


        public ShortcutWindow(ProcessesViewModel procVM)
        {

            InitializeComponent();
            
        }


       

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            _pressedKeys.Add(e.Key);
            KeyboardDevice kd = e.KeyboardDevice;
            _pressedModifiers.Add(kd.Modifiers);

        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {

            shortcutSeq = string.Empty;
            modSeq = string.Empty;
            foreach (Key k in _pressedKeys)
            {
                shortcutSeq += k.GetHashCode() + " + ";
            }

            foreach (ModifierKeys kdT in _pressedModifiers)
            {
                modSeq += kdT.GetHashCode() + " + ";
            }




            shortcutSeq += " modificatori: " + modSeq;

            showShortcutLbl.Content = shortcutSeq;



            //lblShowShortcut.Content = _pressedKeys.ToString();

        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            string commandStr = string.Empty;
            commandStr ="dw/"+ e.Key.GetHashCode().ToString();
            processesViewModelObj.SendKeyboardCom(commandStr);
            e.Handled = true;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            string commandStr = string.Empty;
            commandStr = "up/" + e.Key.GetHashCode().ToString();
            processesViewModelObj.SendKeyboardCom(commandStr);
            e.Handled = true;
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            shortcutSeq = string.Empty;
            showShortcutLbl.Content = String.Empty;
            _pressedKeys.Clear();
            _pressedModifiers.Clear();
        }

    }
}
