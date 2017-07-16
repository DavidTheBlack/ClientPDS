using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using Network;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows;
using ClientPDS.HelperClass;
using System.Windows.Threading;

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

        delegate bool processAddDelegate(ProcessInfoJsonStr p);
        processAddDelegate processAdd;




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



        private NetworkObject netObj;   //Oggetto di rete che incapsula socket ed altro
        private SyncBuffer msgBuffer;      //Buffer di sincronizzazione
        private Thread recThread;       //Thread incaricato della ricezione dei messaggi da parte del server
        private ThreadStart threadDelegate;


        private List<ProcessInfoJsonStr> processesList;

        private ObservableCollection<ProcessInfo> _processes;
        public ObservableCollection<ProcessInfo> Processes
        {
            get { return _processes; }
            set {
                _processes = value;
                RaisePropertyChanged("Apps");
            }
        }

        #endregion

        public ProcessesViewModel()
        {
            msgBuffer = new SyncBuffer();
            _processes = new ObservableCollection<ProcessInfo>();
            msgBuffer.messageReceived += handleReceivedMex;
            processAdd += new processAddDelegate(addProcess);
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
            //tmp.icon = p.icon;

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
        //public bool removeProcess(ProcessInfoJsonStr p)
        //{
        //    int tmpPid = 0;

        //    if (System.Int32.TryParse(p.pid, out tmpPid))
        //    {
        //        int index=-1;
        //        foreach (ProcessInfo proc in Processes)
        //        {
        //            if (proc.pid == tmpPid)
        //            {
        //                index = Processes.IndexOf(proc);
        //                break;
        //            }
        //        }

        //        if (index != -1)
        //        {
        //            Processes.RemoveAt(index);
        //            RaisePropertyChanged("Apps");
        //            return true;
        //        }
        //        else //Se processo non trovato
        //            return false;

        //    }
        //    else //Se si verifica errore nella traduzione da stringa  pid intero
        //        return false;
        //}

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

        /// <summary>
        /// Function used to connect to server
        /// </summary>
        /// <param name="serverIP">ip address of the server</param>
        /// <returns>true if the connection succeded</returns>
        public bool connectToServer(string serverIP)
        {
            int retry = 2;
            while (retry != 0)
            {
                try
                {
                    netObj = new NetworkObject(serverIP, 4444);
                    netObj.OpenTcpConnection();                    
                    break;
                }
                catch (Exception e)
                {
                    retry--;
                    if (retry==0) return false;
                }
            }
            threadDelegate = new ThreadStart(this.receiveMessage);
            recThread = new Thread(threadDelegate);
            recThread.Start();
            //recThread.IsBackground = true;  //Setto il thread in background
            //return true;
            return netObj.remoteIsConnected ? true : false;           
        }

        /// <summary>
        /// Thread method to receive network message
        /// </summary>
        private void receiveMessage()
        {
            while (true)
            {
                msgBuffer.push(netObj.ReceiveData());
            }
        }

        /// <summary>
        /// This method handles the received mex from the server popping them from the messages queue
        /// </summary>
        private void handleReceivedMex(object source, EventArgs e)
        {            
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(List<ProcessInfoJsonStr>));

            //byte[] recBuf = System.Text.UnicodeEncoding.Unicode.GetBytes(netObj.receivedMex);
            byte[] recBuf;
            if(msgBuffer.pop(out recBuf)) //Se il prelievo del messaggio è andato a buon fine lo elaboro
            {
                MemoryStream ms = new MemoryStream(recBuf);
                try
                {
                    processesList = (List<ProcessInfoJsonStr>)js.ReadObject(ms);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore di deserializzazione: " + ex.Message);
                }

                foreach (ProcessInfoJsonStr p in processesList)
                {
                    Application.Current.Dispatcher.Invoke(processAdd,p);
                    //MessageBox.Show("Aggiunto processo in coda");
                }
            }else
            {
                throw(new Exception("Errore prelievo messaggi dalla coda"));
            }
            
            //Console.WriteLine("Messaggio ricevuto: "+net.receivedMex);
            //(System.Text.UnicodeEncoding.Unicode.GetBytes(net.receivedMex));
            //File.WriteAllText("c:\\users\\david\\desktop\\json.txt", net.receivedMex);
            
            //Console.WriteLine("Dati Processo- Pid: " + processes[0].pid + " stato: " + processes[0].state);

        } 


        /// <summary>
        /// This method handles the connection state
        /// </summary>
        private void handleConnectionStateChange(object source, EventArgs e)
        {

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
