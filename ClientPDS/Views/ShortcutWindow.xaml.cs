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
        string shortcutSeq = string.Empty;



        public ShortcutWindow()
        {
            InitializeComponent();
        }


       

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (_pressedKeys.Contains(e.Key))
                return;
            _pressedKeys.Add(e.Key);


        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            shortcutSeq = string.Empty;
            foreach (Key k in _pressedKeys)
            {
                shortcutSeq += k.GetHashCode() + " + ";
            }


            KeyboardDevice kd = e.KeyboardDevice;

            shortcutSeq += " modificatori: " + kd.Modifiers.ToString();

            showShortcutLbl.Content = shortcutSeq;



            //lblShowShortcut.Content = _pressedKeys.ToString();




        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            shortcutSeq = string.Empty;
            showShortcutLbl.Content = String.Empty;
            _pressedKeys.Clear();
        }

    }
}
