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
using System.Net;

namespace ClientPDS
{
    public class ProcessesViewModel
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

        delegate bool editProcessesDelegate(ProcessInfoJsonStr p);
        editProcessesDelegate editProcesses;

        #endregion


        #region constants
        //State of each window can be
        const string windowInit = "0";
        const string windowCreated = "1";
        const string windowClosed = "2";
        const string windowFocused = "3";

        #endregion

        #region interfaceVariables

        AdditionalInfoModel addInfoModel = new AdditionalInfoModel();


        /*

        private string _buttonText = "Connect";
        public string ButtonText
        {
            get
            {
                return _buttonText;
            }

            set
            {
                if (value != _buttonText)
                {
                    _buttonText = value;
                    RaisePropertyChanged("ButtonText");
                }
            }        
        }

        private bool _serverTextEnabled = true;
        public bool ServerTextEnabled
        {
            get { return _serverTextEnabled; }
            set
            {
                if (value != _serverTextEnabled)
                {
                    _serverTextEnabled = value;
                    RaisePropertyChanged("ServerTextEnabled");
                }
            }
        }

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

        */


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


        //The server ip
        private string _serverIP = "127.0.0.1";
        public string ServerIP
        {
            get { return _serverIP; }
            set
            {
                // verify that IP consists of 4 parts
                if (value.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length == 4)
                {
                    IPAddress ipAddr;
                    if (IPAddress.TryParse(value, out ipAddr))
                        _serverIP = value;
                    else
                        MessageBox.Show("Invalid Ip Address");
                }
                else
                    // invalid IP
                    MessageBox.Show("Invalid Ip Address");
            }
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
        private bool keepConnection = true;


       



        
        


        private NetworkObject netObj;   //Oggetto di rete che incapsula socket ed altro        
        private Thread recThread;       //Thread incaricato della ricezione dei messaggi da parte del server
        private ThreadStart threadDelegate;


        private List<ProcessInfoJsonStr> processesList;

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
            //tmp.icon = p.icon;

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
                addInfoModel.FocusedPid = tmpPid;                
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
                netObj = new NetworkObject(_serverIP, 4444);
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
            
            
            bool result = netObj.OpenTcpConnection();
            if (result)
            {
                //Register to the event
                netObj.messageReceived += handleReceivedMex;
                netObj.connectionStateChanged += handleConnectionStateChange;

                while (keepConnection)
                {
                    //If there is some truble receiving data
                    if (!netObj.ReceiveData())
                    {
                        //Close the connection 
                        if (netObj.remoteIsConnected)
                            netObj.CloseConnection();
                        MessageBox.Show("Errore nella ricezione dati dal server.\n" + netObj.log);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Impossibile connettersi al server.\n" + netObj.log);
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
            if (netObj.remoteIsConnected)
            {
                addInfoModel.ButtonText = "Disconnect";
                addInfoModel.IpTextEnabled = false;
               
            }else
            {
                addInfoModel.ButtonText = "Connect";
                addInfoModel.IpTextEnabled = true;
            }            
            
        }

        public void closeApplication()
        {
            CloseConnection();
        }

        public void CloseConnection()
        {
            this.keepConnection = false;
            if (netObj != null && netObj.remoteIsConnected)
            {
                netObj.SendVarData("exit");
                netObj.CloseConnection();
            }
        }
    }

    /*
    public class ProcessesViewModel
    {
        public ObservableCollection<ProcessInfo> Processes;


    }

    */






}
