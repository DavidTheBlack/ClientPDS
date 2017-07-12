using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

//@TODO mettere un mutex per rendere il tutto thread safe

namespace ClientPDS
{
    class Model : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        private List<ProcessInfo> processesList;

        /// <summary>
        /// Pid del processo che ha focus
        /// </summary>
        private int _focusedProcessPid = 0;

        /// <summary>
        /// Get the focussed pid process
        /// </summary>
        public int FocusedProcessPid
        {
            get
            {
                return _focusedProcessPid;
            }

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
            processesList.Add(tmp);

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
            int tmpPid = 0 ;
            
            if (System.Int32.TryParse(p.pid,out tmpPid)){
                int index = processesList.FindIndex(x => (x.pid == tmpPid));
                if (index != -1)
                {
                    processesList.RemoveAt(index);
                    RaisePropertyChanged("Apps");
                    return true;
                }
                else //Se processo non trovato
                    return false;

            }else //Se si verifica errore nella traduzione da stringa  pid intero
                return false;
        }

        /// <summary>
        /// Update the focussed process
        /// </summary>
        /// <param name="p">process info json string object</param>
        /// <returns>true if the update succeeded</returns>
        public bool updateFocusedProcess(ProcessInfoJsonStr p)
        {

            //Bisogna prima rimuovere il vecchio processo in focus e poi setare il nuovo processo in focus
            //int oldFocusIndex;
            //int newFocusIndex;

            //oldFocusIndex = processesList.FindIndex(x => (x.pid == _focusedProcessPid));

            int tmpPid = 0;

            if (System.Int32.TryParse(p.pid, out tmpPid))
            {
                _focusedProcessPid = tmpPid;
            }
            else
            {
                return false;
            }
            RaisePropertyChanged("Apps");
            return true;

            //    newFocusIndex = processesList.FindIndex(x => (x.pid == tmpPid));

            //    if ((newFocusIndex != -1) && (oldFocusIndex != -1))
            //    {   //Effettuo aggiornamento dello stato
            //        processesList[newFocusIndex].state = 1;
            //        processesList[oldFocusIndex].state = 0;
            //        _focusedProcessPid = tmpPid;
            //        RaisePropertyChanged("ProcessFocussUpdated");
            //        return true;
            //    }
            //    else if (newFocusIndex != -1)
            //    {
            //        processesList[newFocusIndex].state = 1;
            //        _focusedProcessPid = tmpPid;
            //        RaisePropertyChanged("ProcessFocussUpdated");
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }

            //}
            //return false;
        }  
        
                         
    }


    [DataContract]
    public class ProcessInfoJsonStr
    {
        [DataMember]
        public string pid { get; set; }

        [DataMember]
        public string state { get; set; }

        [DataMember]
        public string title { get; set; }

        [DataMember]
        public string path { get; set; }

        [DataMember]
        public string icon { get; set; }
    }

    public class ProcessInfo
    {

        public int pid { get; set; }

        //Informazione ridondante, nel model abbiamo 
        //la variabile che contiene questa informazione
        //public int state { get; set; }

        public string title { get; set; }


        public string path { get; set; }


        public string icon { get; set; }
    }





}

