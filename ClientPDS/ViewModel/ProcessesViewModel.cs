using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientPDS
{
    class ProcessesViewModel: INotifyPropertyChanged
    {
        #region properties

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        //public delegate void FocusUpdated(object s, FocusedEventArgs e);
        //public event FocusUpdated OnFocusUpdate;
        //public void RaiseOnFocusUpdate(int pid)
        //{
        //    if (OnFocusUpdate != null)
        //    {
        //        OnFocusUpdate(this, new FocusedEventArgs(pid));
        //    }
        //}

        private int _focusPid = 125;
        public int FocusPid
        {
            get
            {
                return _focusPid;
            }

            set
            {
                if (value != _focusPid)
                {
                    _focusPid = value;
                    RaisePropertyChanged("FocusPid");
                }
                

            }
        }
        #endregion


        public ObservableCollection<ProcessInfo> Processes
        {
            get;
            set;
        }

        

        /// <summary>
        /// Add process to the list of processes and notify the change of the model
        /// </summary>
        /// <param name="p">process info json string object</param>
        /// <returns>true if the insertion of the new process succeeded</returns>
        public bool addProcess(ProcessInfoJsonStr p)
        {
            ProcessInfo tmp = new ProcessInfo();
            int tmpInt;

            //Converting the pid from string to int
            if (System.Int32.TryParse(p.pid, out tmpInt))
                tmp.pid = tmpInt;
            else
                return false;

            ////Converting the state from string to int
            //if (System.Int32.TryParse(p.state, out tmpInt))
            //    tmp.state = tmpInt;
            //else
            //    return false;

            tmp.title = p.title;
            tmp.path = p.path;
            tmp.icon = p.icon;

            //Process added to the list
            Processes.Add(tmp);

            RaisePropertyChanged("Apps");
            return true;
        }

        /// <summary>
        /// Remove a process from the list of processes following it's closing on the server
        /// </summary>
        /// <param name="p">process info json string object</param>
        /// <returns>true if the removing succeeded</returns>
        public bool removeProcess(ProcessInfoJsonStr p)
        {
            int tmpPid = 0;

            if (System.Int32.TryParse(p.pid, out tmpPid))
            {
                int index=-1;
                foreach (ProcessInfo proc in Processes)
                {
                    if (proc.pid == tmpPid)
                    {
                        index = Processes.IndexOf(proc);
                        break;
                    }
                }

                if (index != -1)
                {
                    Processes.RemoveAt(index);
                    RaisePropertyChanged("Apps");
                    return true;
                }
                else //Se processo non trovato
                    return false;

            }
            else //Se si verifica errore nella traduzione da stringa  pid intero
                return false;
        }

        /// <summary>
        /// Update the focussed process
        /// </summary>
        /// <param name="p">process info json string object</param>
        /// <returns>true if the update succeeded</returns>
        public bool updateFocusedProcess(ProcessInfoJsonStr p)
        {
            int tmpPid = 0;

            if (System.Int32.TryParse(p.pid, out tmpPid))
            {
                //Save the focus pid in private var
                FocusPid = tmpPid;

                //int index = -1;
                //foreach (ProcessInfo proc in Processes)
                //{
                //    if (proc.pid == tmpPid)
                //    {
                //        index = Processes.IndexOf(proc);
                //        break;
                //    }
                //}
                //return (index != -1) ? true : false;

                return true;
            }
            else //Se si verifica errore nella traduzione da stringa  pid intero
                return false;
            }

    }



    public class FocusedEventArgs : EventArgs
    {
        /// <summary>
        /// Pid of the process that is focused
        /// </summary>
        private int _pid;
        /// <summary>
        /// Pid of the process that is focused
        /// </summary>
        public int Pid
        {
            get
            {
                return _pid;
            }
        }

        public FocusedEventArgs(int p)
        {
            _pid = p;
        }

    }






}
