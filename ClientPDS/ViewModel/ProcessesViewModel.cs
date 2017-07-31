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
using System.Diagnostics;

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

        delegate void clearProcessesDelegate();
        clearProcessesDelegate clearProcesses;

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

        private bool _shortcutToggleEnabled;
        public bool ShortcutToggleEnabled
        {
            get { return _shortcutToggleEnabled; }
            set
            {
                if (_shortcutToggleEnabled != value)
                {
                    _shortcutToggleEnabled = value;
                    RaisePropertyChanged("ShortcutToggleEnabled");
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
                if (!CheckIPValid(value))
                {
                    MessageBox.Show("Invalid IP Address!");
                    return;
                }
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
        /// flag that signals the user has stopped the connection
        /// </summary>
        private bool connectionClosedbyUser;
        public bool ApplicationIsClosing;

        private bool _progressRing;
        public bool ProgressRing { 
            get
            {
                return _progressRing;
            }

            set
            {
                if (_progressRing != value)
                {
                    _progressRing = value;
                    RaisePropertyChanged("ProgressRing");
                }
            }
        }

        //Default Icon Bytes
        private byte[] defaultIcon;

        private NetworkObject netObj;   //Oggetto di rete che incapsula socket ed altro        
        private Thread recThread;       //Thread incaricato della ricezione dei messaggi da parte del server
        private ThreadStart threadNetworkDelegate;

        private Stopwatch stopWatch;    
        private ThreadStart threadStopWatchDelegate;
        private Thread stopWatchThread;
        private bool firstTimeFocused;          //Used to initialize the starting point of the watchdog timer
        private bool terminateWatchThread;      //Used to terminate the watchdog thread


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

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessesViewModel()
        {
            _log = string.Empty;
            _processes = new ObservableCollection<ProcessInfo>();
            ServerIP = "127.0.0.1";
            ButtonText = "Connect";
            IpTextEnabled = true;
            ShortcutToggleEnabled = false;
            connectionClosedbyUser = false;
            ApplicationIsClosing = false;
            ProgressRing = false;

            firstTimeFocused = true;            //Used to initialize the starting point of the watchdog timer
            terminateWatchThread = false;       //Used to terminate the watchdog thread
            stopWatch = new Stopwatch(); 


            defaultIcon= System.IO.File.ReadAllBytes("..\\..\\Resources\\icon_def.ico");

            threadNetworkDelegate = new ThreadStart(this.NetworkTask);
           
            threadStopWatchDelegate = new ThreadStart(this.ComputePercentageTask);



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

            //Initialization of the focus timer
            tmp.FocusPercentage = 0;
            tmp.FocusTimeStamp = TimeSpan.Zero;
            tmp.TotalFocusTime = TimeSpan.Zero;

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
        /// Method used to clear the processes list
        /// </summary>
        public void clearProcessesList()
        {
            Processes.Clear();
            RaisePropertyChanged("Processes");
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

                lock (this)
                {
                    //Search for the pid in the processes list
                    //and Update the View
                    foreach (ProcessInfo process in _processes)
                    {
                        if (process.Pid == tmpPid)
                        {
                            FocusedProcess = process;
                            //Condition used to start the focus percentage calculation on the first focus event
                            if (firstTimeFocused)
                            {
                                firstTimeFocused = false;
                                terminateWatchThread = false;
                                stopWatchThread = new Thread(threadStopWatchDelegate);
                                //Lets free the stopwatch thread
                                stopWatchThread.Start();
                            }
                            break;
                        }
                    }
                    return true;
                }
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
                ProgressRing = true;
                netObj = new NetworkObject(ServerIP, 4444);
                recThread = new Thread(threadNetworkDelegate);
                recThread.Start();
            }
            catch(Exception ex)
            {
                _log = ex.Message;
                return false;
            }
            

            return true;
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
            netObj.messageReceived += HandleReceivedMex;
            netObj.connectionStateChanged += HandleConnectionStateChange;

            bool result = netObj.OpenTcpConnection();
            if (result)
            {                                           
                while (true)
                {
                    //If there is some truble receiving data
                    if (!netObj.ReceiveData())
                    {
                        //The connection is already closed                                              
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Server is not reachable.\n" + netObj.log);
                ProgressRing = false;
            }

            netObj.messageReceived -= HandleReceivedMex;
            netObj.connectionStateChanged -= HandleConnectionStateChange;

        }

        /// <summary>
        /// Start procedures for closing in a proper way the connection with the server
        /// </summary>
        public void RequestCloseConnection()
        {
            connectionClosedbyUser = true;
            if (netObj != null && netObj.remoteIsConnected)
            {
                netObj.SendVarData(NetworkObject.closeRequest);
            }
        }

        /// <summary>
        /// This method handles the connection state
        /// </summary>
        private void HandleConnectionStateChange(object source, EventArgs e)
        {

            if( !ApplicationIsClosing )
            {
                updateConnectionInterface += UpdateConnectionGuiElement;
                Application.Current.Dispatcher.Invoke(updateConnectionInterface, netObj.remoteIsConnected);
                updateConnectionInterface -= UpdateConnectionGuiElement;
                connectionClosedbyUser = false;
                ProgressRing = false;
            }
           
        }

        /// <summary>
        /// This method handles the received mex from the server popping them from the messages queue
        /// </summary>
        private void HandleReceivedMex(object source, EventArgs e)
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
                    string recMessStr = System.Text.UnicodeEncoding.Unicode.GetString(recBuf);
                    switch (recMessStr)
                    {
                        case NetworkObject.connLost:        //Connection lost because the server shutdown the socket
                        case NetworkObject.closeRequest:    //The connection was closed in proper way
                            {
                                HandleDisconnectionMex(recMessStr);
                                return;
                                break;
                            }                                                      
                    }

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
        }       

        /// <summary>
        /// Handle the network disconnection analizing the received disconnection message
        /// </summary>
        /// <param name="disconnectMex">message received from the network object</param>
        public void HandleDisconnectionMex(string disconnectionMex)
        {
            terminateWatchThread = true;    //Stop the percentage calculation in the stopWatchThread
            firstTimeFocused = true;        //Used to start the StopWatchThreadat the next connection

            switch (disconnectionMex)
            {
                case NetworkObject.closeRequest:
                    if (!connectionClosedbyUser)
                    {
                        MessageBox.Show("Server disconnected");
                    }
                    break;
                case NetworkObject.connLost:
                    MessageBox.Show("Connection to the server lost - check you network");
                    break;
            }

            netObj.CloseConnection();       //Close the socket and dispose the network resources

        }

        /// <summary>
        /// Used to send the key command string to the server
        /// </summary>
        /// <param name="keyComStr">string of keyboard command</param>
        public void SendKeyboardCom(string keyComStr)
        {
            if (netObj.remoteIsConnected && FocusedProcess != null)
            {
                string focusPidStr = FocusedProcess.Pid.ToString();
                string message = focusPidStr + "|" + keyComStr;
                netObj.SendVarData(message);
            } 
        }

        /// <summary>
        /// Method that has to be executed in separate thread to compute the focus percentage time 
        /// for each running application
        /// </summary>
        private void ComputePercentageTask()
        {
            TimeSpan timeSinceStart = TimeSpan.Zero;
            TimeSpan oldTimeSinceStart = TimeSpan.Zero;
            TimeSpan deltaTime = TimeSpan.Zero;

            stopWatch.Start();
            while (!terminateWatchThread)
            {
                lock (this)
                {
                    timeSinceStart = stopWatch.Elapsed;
                    deltaTime = timeSinceStart - oldTimeSinceStart;
                    if (FocusedProcess != null)
                        FocusedProcess.TotalFocusTime += deltaTime;//elapsed - FocusedProcess.FocusTimeStamp;

                    foreach (ProcessInfo p in Processes)
                    {
                        try
                        {
                            p.FocusPercentage = Math.Round((p.TotalFocusTime.TotalMilliseconds / timeSinceStart.TotalMilliseconds) * 100, 2);
                        }
                        catch (DivideByZeroException ex)
                        {
                            p.FocusPercentage = 0;
                        }
                    }

                    oldTimeSinceStart = timeSinceStart;
                    Thread.Sleep(500);
                }
            }
            stopWatch.Stop();
        }

        private void UpdateConnectionGuiElement(bool connectionState)
        {
            Processes.Clear();

            if (connectionState)
            {
                ButtonText = "Disconnect";
                IpTextEnabled = false;
                ShortcutToggleEnabled = true;

            }
            else //Server is disconnected
            {
                ButtonText = "Connect";
                IpTextEnabled = true;
                ShortcutToggleEnabled = false;
                
            }
        }

        public void closeApplication()
        {
            ApplicationIsClosing = true;
            RequestCloseConnection();
        }
     
        /// <summary>
        /// Check IP Address, will accept 0.0.0.0 as a valid IP
        /// </summary>
        /// <param name="strIP">0.0.0.0 style IP address</param>
        /// <returns>bool</returns>
        public bool CheckIPValid(string strIP)
        {
            // Split string by ".", check that array length is 3
            char chrFullStop = '.';
            string[] arrOctets = strIP.Split(chrFullStop);
            if (arrOctets.Length != 4)
            {
                return false;
            }
            // Check each substring checking that the int value is less than 255 
            // and that is char[] length is !>     2
            Int16 MAXVALUE = 255;
            Int32 temp; // Parse returns Int32
            foreach (String strOctet in arrOctets)
            {
                if (strOctet.Length > 3)
                {
                    return false;
                }
                
               if(int.TryParse(strOctet,out temp))
                {
                    if (temp > MAXVALUE)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
               
            }
            return true;
        }
    }
}
