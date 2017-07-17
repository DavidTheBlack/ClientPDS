using System;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using NetworkStateObject;
using System.Windows;
using ClientPDS.HelperClass;

namespace Network
{
    public class NetworkObject
    {

        #region fields and properties

        //Remote Socket
        private Socket _remote;

        //Remote IP
        private string _remoteIP;
        public string remoteIP
        {
            get { return _remoteIP; }
            set { _remoteIP = value; }
        }
        //Remote Port
        private int _remotePort;
        public int remotePort
        {
            get { return _remotePort; }
            set { _remotePort = value; }
        }

        
        private bool _remoteIsConnected = false;
        /// <summary>
        /// True if the object is connected false otherwise
        /// </summary>
        public bool remoteIsConnected
        {
            get { return this._remoteIsConnected; }
        }

        //Used to log special event
        private string _log = string.Empty;
        public string log
        {
            get { return _log; }
        }

        /// <summary>
        /// Queue that contains the received mex
        /// </summary>
        private SyncBuffer msgQueue;
        public int receivedMexNumber
        {
            get { return msgQueue.queueSize; }
        }
               
        #endregion

        #region Events


        /// <summary>
        /// Evento lanciato quando lo stato del client cambia //Connesso o Disconnesso
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public delegate void ConnectionStateChangedEventHandler(object source, EventArgs e);
        public event ConnectionStateChangedEventHandler connectionStateChanged;
        protected virtual void OnConnectionStateChanged()
        {
            //Aggiorno lo stato della connessione tcp
            if (this._remoteIsConnected == true)
            {
                this._remoteIsConnected = false;
            }
            else
            {
                this._remoteIsConnected = true;
            }

            if (connectionStateChanged != null)
            {
                connectionStateChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Event raised when a new message has been received
        /// </summary>
        public delegate void MessageReceivedEventHandler(object source, EventArgs e);
        public event MessageReceivedEventHandler messageReceived;
        protected virtual void OnMessageReceived()
        {
            if (messageReceived != null)
            {
                messageReceived(this, EventArgs.Empty);
            }
        }

        #endregion

        #region methods



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ip">Ip address of the server</param>
        /// <param name="port">port number of the server</param>
        public NetworkObject(string ip, int port)
        {
            _remoteIP = ip;
            _remotePort = port;
        }
        
        /// <summary>
        /// Open tcp connection with the server
        /// </summary>
        public void OpenTcpConnection()
        {
            int retry = 2;
            while (retry != 0)
            {
                try
                {
                    //Create the remote endpoint for the socket
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_remoteIP), _remotePort);
                    ////Create TCP/IP socket                    
                    _remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    //start tcp connection
                    _remote.Connect(remoteEP);
                    //Notify the successfull connection
                    OnConnectionStateChanged();
                }
                catch (Exception ex)
                {
                    retry--;
                    if (retry == 0)
                    {
                        _log = "Server is not active. \n Please start server and try again. \n" + ex.ToString();
                        throw ex;
                    }
                }
            }
            
        }

        /// <summary>
        /// Close the tcp connection
        /// </summary>
        public void CloseConnection()
        {
            if (this._remoteIsConnected)
            {
                //Segnalo connessione disconnessione del client
                this._remote.Shutdown(SocketShutdown.Both);
                this._remote.Disconnect(true);
                this._remote.Close();
                this._remote = null;

                this.OnConnectionStateChanged();
            }
        }

        /// <summary>
        /// Retrieve message from the message buffer 
        /// </summary>
        /// <param name="receivedBytes"></param>
        /// <returns>return true if the pop </returns>
        public bool GetMessage(out byte[] receivedBytes)
        {
            return msgQueue.pop(out receivedBytes);
            
        }

        /// <summary>
        /// Sync receive method
        /// </summary>
        public void ReceiveData()
        {
            byte[] data = new byte[0];
            if (_remoteIsConnected)
            {
                
                try
                {
                    //read data from the remote server
                    byte[] datasize = new byte[4];
                    int bytesRead = _remote.Receive(datasize);

                    int total = 0;
                    int recv;
                    int size = BitConverter.ToInt32(datasize, 0);

                    data = new byte[size];
                    int dataleft = size;
                    while (total < size)
                    {
                        recv = _remote.Receive(data, total, dataleft, 0);
                        if (recv == 0)
                        {
                            data = UnicodeEncoding.Unicode.GetBytes("exit");
                            break;
                        }
                        total += recv;
                        dataleft -= recv;
                    }

                           
                }
                catch (Exception ex)
                {
                    throw ex;
                }


            }

            msgQueue.push(data);
            OnMessageReceived();
        }

        /// <summary>
        /// Method used to send send data
        /// </summary>
        /// <param name="mex">string of the message to send</param>
        /// <returns></returns>
        public int SendVarData(string mex)
        {
            int total = 0;
            int size = mex.Length;
            int dataleft = size;
            int sent;
            int sentSize;

            Byte[] dataSize = BitConverter.GetBytes(size);
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(mex.ToString());

            
            sentSize = _remote.Send(dataSize);

            while (total < size)
            {
                sent = _remote.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft -= sent;
            }
            return total;

        }

       

        #endregion
    }
}