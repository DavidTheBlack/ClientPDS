﻿using System;
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


        bool altKeyisPressed = false;
        bool ctrlKeyisPressed = false;
        bool shiftKeyisPressed = false;






        public ShortcutWindow(ProcessesViewModel procVM)
        {
            processesViewModelObj = procVM;
            InitializeComponent();            
        }


       

        //private void TextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    //Controllo se il tasto è un modificatore
        //    //se lo è esco e non faccio nulla

        //    //se no nè un modificatore 
        //    //prelevo il tasto
        //    //controllo i modificatori premuti
        //    //invio il dato al server

        //    switch (e.Key)
        //    {
        //        case Key.LeftAlt:
        //        case Key.RightAlt:
        //        case Key.System:
        //        case Key.LeftCtrl:
        //        case Key.RightCtrl:
        //        case Key.LeftShift:
        //        case Key.RightShift:
        //            {
        //                e.Handled = true;
        //                break;
        //            }
        //    }

        //    if (!e.Handled)
        //    {
        //        Keyboard.Modifiers.GetHashCode();

        //    }


        //    _pressedKeys.Add(e.Key);
        //    KeyboardDevice kd = e.KeyboardDevice;
        //    _pressedModifiers.Add(kd.Modifiers);

        //}

        //private void TextBox_KeyUp(object sender, KeyEventArgs e)
        //{

        //    shortcutSeq = string.Empty;
        //    modSeq = string.Empty;
        //    foreach (Key k in _pressedKeys)
        //    {
        //        shortcutSeq += k.GetHashCode() + " + ";
        //    }

        //    foreach (ModifierKeys kdT in _pressedModifiers)
        //    {
        //        modSeq += kdT.GetHashCode() + " + ";
        //    }




        //    shortcutSeq += " modificatori: " + modSeq;

        //    //showShortcutLbl.Content = shortcutSeq;



        //    //lblShowShortcut.Content = _pressedKeys.ToString();

        //}

        protected override void OnPreviewKeyDown(KeyEventArgs e)
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
            //    case Key.LeftAlt:
            //    case Key.RightAlt:
            //    case Key.System:
            //        {
            //            if (!altKeyisPressed)
            //            {
            //                altKeyisPressed = true;
            //                altSwitch.IsChecked = true;
            //            }else
            //            {
            //                e.Handled=true;
            //            }
            //            break;
            //        }
            //    case Key.LeftCtrl:
            //    case Key.RightCtrl:
            //        {
            //            if (!ctrlKeyisPressed)
            //            {
            //                ctrlKeyisPressed = true;
            //                ctrlSwitch.IsChecked = true;
            //            }
            //            else
            //                e.Handled = true;
            //            break;
            //        }

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
                commandStr = "dw/"+Keyboard.Modifiers.GetHashCode() +"/" + KeyInterop.VirtualKeyFromKey(key);
                processesViewModelObj.SendKeyboardCom(commandStr);
            }
            e.Handled = true;
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {

            string commandStr = string.Empty;
            string modifiers = Keyboard.Modifiers.GetHashCode().ToString();
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            
            switch (key)
            {

                //    case Key.RightAlt:     //Fix the ALT-gr behaviour of set also the right ctrl
                //        {                   
                //            altKeyisPressed = false;
                //            altSwitch.IsChecked = false;
                //            ctrlSwitch.IsChecked = false;
                //            ctrlKeyisPressed = false;
                //            commandStr = "up/" + KeyInterop.VirtualKeyFromKey(Key.RightCtrl);
                //            processesViewModelObj.SendKeyboardCom(commandStr);

                //            break;
                //        }
                //    case Key.LeftAlt:
                //    case Key.System:
                //        {
                //            if (altKeyisPressed)
                //            {
                //                altKeyisPressed = false;
                //                altSwitch.IsChecked = false;

                //            }
                //            break;
                //        }
                //    case Key.LeftCtrl:
                //    case Key.RightCtrl:
                //        {
                //            if (ctrlKeyisPressed)
                //            {
                //                ctrlKeyisPressed = false;
                //                ctrlSwitch.IsChecked = false;
                //            }
                //                break;
                //        }

                //    case Key.LeftShift:
                //    case Key.RightShift:
                //        {
                //            if (shiftKeyisPressed)
                //            {
                //                shiftKeyisPressed = false;
                //                shiftSwitch.IsChecked = false;
                //            }
                //                break;
                //        }
                //}

                //commandStr = "up/" + KeyInterop.VirtualKeyFromKey(key);

                //processesViewModelObj.SendKeyboardCom(commandStr);

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
                processesViewModelObj.SendKeyboardCom(commandStr);
            }
           
            e.Handled = true;
        }
        




        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            string commandStr;
            commandStr = "dw/" + KeyInterop.VirtualKeyFromKey(Key.System);
            processesViewModelObj.SendKeyboardCom(commandStr);
            commandStr = "dw/126";
            processesViewModelObj.SendKeyboardCom(commandStr);
            commandStr = "up/126";
            processesViewModelObj.SendKeyboardCom(commandStr);
            commandStr = "up/" + KeyInterop.VirtualKeyFromKey(Key.System);
            processesViewModelObj.SendKeyboardCom(commandStr);
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            shortcutSeq = string.Empty;
           // showShortcutLbl.Content = String.Empty;
            _pressedKeys.Clear();
            _pressedModifiers.Clear();
        }

    }
}
