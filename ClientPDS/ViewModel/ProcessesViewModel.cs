using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Network;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows;
using ClientPDS.HelperClass;
using System.Windows.Threading;
using System.Net;
using System.ComponentModel;
using System.Data;

namespace ClientPDS
{
    public class ProcessesViewModel : INotifyPropertyChanged
    /*
     * InotifyPropertyChanged mi obbliga ad implementare gli eventi PropertyChangedEventHandler.
     * Questo evento è usato per notificare i cambiamenti delle variabili ai componenti grafici collegati a tali variabili.
     * Se un elemento grafico ha il binding con una variabile e questa vinee modificata via codice, 
     * se non viene sollevato l'evento PropertyChangedEventHandler l'elemento grafico non mostrerà la variazione. 
     * Viceversa se l'elemento grafico viene modificato (es textbox) dall'utente la modifica si
     * propagherà anche alla variabile. 
     */
    {
        #region event-Delegates

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }

        public delegate void NetworkStateChangedHandler(object s, EventArgs e);
        public event NetworkStateChangedHandler NetworkStateChanged;
        protected void RaiseNetworkStateChanged()
        {
            if (NetworkStateChanged != null)
            {
                NetworkStateChanged(this, EventArgs.Empty);
            }
        }

        //Delegato per avviare avviare il metodo via dispatcher
        delegate bool editProcessesDelegate(ProcessInfoJsonStr p);
        editProcessesDelegate editProcesses;

        //Delegate used to update GUI connection element 
        delegate void updateConnectionInterfaceDelegate(bool connectionState);
        updateConnectionInterfaceDelegate updateConnectionInterface;

        #endregion

        #region constants
        //State of each window can be
        const string windowInit = "0";
        const string windowCreated = "1";
        const string windowClosed = "2";
        const string windowFocused = "3";
        const string noIconWindow = "NoIcon";

        #endregion

        #region interfaceVariables

        private int _focusedPid;
        public int FocusedPid
        {
            get
            {
                return _focusedPid;
            }

            set
            {
                if (_focusedPid != value)
                {
                    _focusedPid = value;
                    RaisePropertyChanged("FocusedPid");
                }

            }
        }

        private ProcessInfo _focusedProcess;
        public ProcessInfo FocusedProcess
        {
            get
            {
                return _focusedProcess;
            }

            set
            {
                if (_focusedProcess != value)
                {
                    _focusedProcess = value;
                    RaisePropertyChanged("FocusedProcess");
                }

            }
        }

        private bool _ipTextEnabled;
        public bool IpTextEnabled
        {
            get
            {
                return _ipTextEnabled;
            }

            set
            {
                if (_ipTextEnabled != value)
                {
                    _ipTextEnabled = value;
                    RaisePropertyChanged("IpTextEnabled");
                }
            }
        }

        private string _serverIP;
        public string ServerIP
        {
            get
            {
                return _serverIP;
            }

            set
            {
                if (_serverIP != value)
                {
                    _serverIP = value;
                    RaisePropertyChanged("ServerIP");
                }

            }
        }

        private string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                if (value != _buttonText)
                {
                    _buttonText = value;
                    RaisePropertyChanged("ButtonText");
                }
            }
        }

        #endregion

        #region fields and properties
        private string _log;
        /// <summary>
        /// Retrieve last error log.
        /// </summary>
        public string Log
        {
            get { return _log; }
        }


        /// <summary>
        /// Return true if the server is connected false otherwise
        /// </summary>
        public bool connectedToServer
        {
            get
            {
                if (netObj != null)
                    return netObj.remoteIsConnected;
                else
                    return false;
            }
        }

        /// <summary>
        /// flag that signals to keep alive the tcp connection or not
        /// </summary>
        private bool keepConnection;
        /// <summary>
        /// flag that signals the user has stopped the connection
        /// </summary>
        private bool connectionClosedbyUser;


        //Default Icon Bytes
        private byte[] defaultIcon;





        private NetworkObject netObj;   //Oggetto di rete che incapsula socket ed altro        
        private Thread recThread;       //Thread incaricato della ricezione dei messaggi da parte del server
        private ThreadStart threadDelegate;

        private ObservableCollection<ProcessInfo> _processes;
        public ObservableCollection<ProcessInfo> Processes
        {
            get { return _processes; }
            set {
                _processes = value;
                RaisePropertyChanged("Processes");
            }
        }

        #endregion

        public ProcessesViewModel()
        {
            _log = string.Empty;
            _processes = new ObservableCollection<ProcessInfo>();
            ServerIP = "127.0.0.1";
            ButtonText = "Connect";
            IpTextEnabled = true;
            keepConnection = true;
            connectionClosedbyUser = false;

            defaultIcon= System.IO.File.ReadAllBytes("..\\..\\Resources\\icon_def.ico");


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
                tmp.Pid = tmpInt;
            else
                return false;

            ////Converting the state from string to int
            //if (System.Int32.TryParse(p.state, out tmpInt))
            //    tmp.state = tmpInt;
            //else
            //    return false;

            tmp.Title = p.title;
            tmp.Path = p.path;


            //Check if the proces has the icon or not
            if(string.Compare(p.icon, noIconWindow) == 0) 
            {
                //the proces doesn't have the icon so display default icon
                tmp.Icon = defaultIcon;

            }else
            {
                //The proces has the icon
                tmp.Icon = Convert.FromBase64String(p.icon);
                
            }
            

            //Process added to the list
            Processes.Add(tmp);    

            RaisePropertyChanged("Processes");
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
                int index = -1;
                foreach (ProcessInfo proc in Processes)
                {
                    if (proc.Pid == tmpPid)
                    {
                        index = Processes.IndexOf(proc);
                        break;
                    }
                }

                if (index != -1)
                {
                    Processes.RemoveAt(index);
                    RaisePropertyChanged("Processes");
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
                FocusedPid = tmpPid;

                //Search for the pid in the processes list
                //and Update the View
                foreach (ProcessInfo process in _processes)
                {
                    if (process.Pid == tmpPid)
                    {
                        FocusedProcess = process;
                        break;
                    }
                }
                    return true;
            }
            else //Se si verifica errore nella traduzione da stringa  pid intero
                return false;
            }

        /// <summary>
        /// Function used to connect to server
        /// </summary>
        /// <param name="serverIP">ip address of the server</param>
        public bool StartNetworkTask()
        {
            //Save the server ip in the current object
            try
            {
                netObj = new NetworkObject(ServerIP, 4444);
                threadDelegate = new ThreadStart(this.NetworkTask);
                recThread = new Thread(threadDelegate);
                keepConnection = true;
                recThread.Start();
            }
            catch(Exception ex)
            {
                _log = ex.Message;
                keepConnection = false;
                return false;
            }
            

            return true;
            //Codice obsoleto
            //recThread.IsBackground = true;  //Setto il thread in background
            //return true;        
            //return netObj.remoteIsConnected ? true : false;           
        }
       
        /// <summary>
        /// Method that has to be executed in separate thread
        /// </summary>
        public void NetworkTask()
        {
            //1 Connessione al server
            //2 ricezione dei messaggi
            //3 inserimento nella coda dei messaggi            
            //Register to the event
            netObj.messageReceived += handleReceivedMex;
            netObj.connectionStateChanged += handleConnectionStateChange;

            bool result = netObj.OpenTcpConnection();
            if (result)
            {               
                while (keepConnection)
                {
                    //If there is some truble receiving data
                    if (!netObj.ReceiveData())
                    {
                        //Close the connection 
                        if (netObj.remoteIsConnected)
                            netObj.CloseConnection();
                        if (!connectionClosedbyUser)
                        {
                            MessageBox.Show("Server Disconnected");

                        }else
                        {
                            connectionClosedbyUser = false;
                        }
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Server is not reachable.\n" + netObj.log);
                return;
            }
            
            //Close the connection and return
            netObj.CloseConnection();
        }

        /// <summary>
        /// This method handles the received mex from the server popping them from the messages queue
        /// </summary>
        private void handleReceivedMex(object source, EventArgs e)
        {
            List<ProcessInfoJsonStr> processesList = new List<ProcessInfoJsonStr>();

            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(List<ProcessInfoJsonStr>));
            byte[] recBuf;
            //Process messages in the queue
            while (netObj.receivedMexNumber != 0)
            {
                if(netObj.GetMessage(out recBuf))
                {
                    //Processo il messaggio
                    MemoryStream ms = new MemoryStream(recBuf);
                    try
                    {
                        processesList = (List<ProcessInfoJsonStr>)js.ReadObject(ms);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Errore di deserializzazione: " + ex.Message);
                        //Test if the received mex is a protocol message (Closing connection for example)
                        if (System.Text.UnicodeEncoding.Unicode.GetString(recBuf) == "exit")
                        {
                            //exit the method
                            break;
                        }
                    }

                    foreach (ProcessInfoJsonStr p in processesList)
                    {
                        switch (p.state)
                        {
                            case windowInit:
                                editProcesses+= addProcess;
                                Application.Current.Dispatcher.Invoke(editProcesses, p);
                                editProcesses -= addProcess;
                                break;
                            case windowCreated:
                                editProcesses += addProcess;
                                Application.Current.Dispatcher.Invoke(editProcesses, p);
                                editProcesses -= addProcess;
                                break;
                            case windowFocused:
                                editProcesses += updateFocusedProcess;
                                Application.Current.Dispatcher.Invoke(editProcesses, p);
                                editProcesses -= updateFocusedProcess;
                                break;
                            case windowClosed:
                                editProcesses += removeProcess;
                                Application.Current.Dispatcher.Invoke(editProcesses, p);
                                editProcesses -= removeProcess;
                                break;
                        }
                    }
                }
                else
                {
                    break;
                }
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
            
            updateConnectionInterface += updateConnectionGuiElement;
            Application.Current.Dispatcher.Invoke(updateConnectionInterface, netObj.remoteIsConnected);
            updateConnectionInterface -= updateConnectionGuiElement;
        }

        private void updateConnectionGuiElement(bool connectionState)
        {
            if (connectionState)
            {
                ButtonText = "Disconnect";
                IpTextEnabled = false;

            }
            else //Server is disconnected
            {
                ButtonText = "Connect";
                IpTextEnabled = true;
                //Clear the processes list
                Processes.Clear();
            }
        }

        public void closeApplication()
        {
            CloseConnection();
        }

        public void CloseConnection()
        {
            this.connectionClosedbyUser = true;
            this.keepConnection = false;
            if (netObj != null && netObj.remoteIsConnected)
            {
                netObj.SendVarData("-1|exit");
                netObj.CloseConnection();
            }
            //Clear the processes list
            Processes.Clear();
        }

    }
}
