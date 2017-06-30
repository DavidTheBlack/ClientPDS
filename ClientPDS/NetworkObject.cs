using System;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using NetworkStateObject;

namespace Network
{
    public class NetworkObject
    {
        private string _locHostName;
        private IPAddress[] _localAddresses;
        private IPAddress _localAddressV4;
        public IPAddress localAddressV4
        {
            get { return _localAddressV4; }
        }

        //Listening connection port
        private int _localPort;

        //Remote Socket Serve avere anche questo handler per la trasmissione sincrona
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

        //Socket asincrono
        private Socket _listener;
        private bool _remoteIsConnected = false;
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

        //Stringa ricevuta
        private string _receivedMex = string.Empty;
        private int sentSize;
        private int sentData;

        public string receivedMex
        {
            get { return _receivedMex; }
        }


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
        /// Evento lanciato quando un nuovo messaggio è consegnato
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
        //Constructor as server
        public NetworkObject(int port)
        {
            this._localPort = port;
        }

        //Constructor as client
        public NetworkObject(string ip, int port)
        {
            _remoteIP = ip;
            _remotePort = port;
        }

        /// <summary>
        /// Funzione che mette in ascolto l'oggetto in attesa di connessioni TCP 
        /// </summary>
        public void waitForTcpConnection()
        {

            try
            {
                //Resolve the host name
                _locHostName = Dns.GetHostName();
                //Get all IP addresses of the host 
                _localAddresses = Dns.GetHostAddresses(_locHostName);
                //keep the first IPv4 Address from the configuration
                for (int i = 0; i < _localAddresses.Length; i++)
                {
                    if (IPAddress.Parse(_localAddresses[i].ToString()).AddressFamily == AddressFamily.InterNetwork)
                    {
                        _localAddressV4 = _localAddresses[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error trying to get local address {0}", ex.ToString());
            }

            // Verify we got an IP address. Tell the user if we did
            if (_localAddressV4 == null)
            {
                Console.WriteLine("Unable to get local address");
                return;
            }

            //Edit hai reso listener privato a livello di classe
            //Create a new listener socket on localPort and localAddress
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            _listener.Bind(new IPEndPoint(_localAddressV4, _localPort));
            _listener.Listen(1); //Accetta connessioni da un solo socket per volta
            _listener.BeginAccept(new AsyncCallback(this.TcpConnectionCallbackServer), _listener);


        }

        /// <summary>
        /// Funzione che inizializza una connessione tcp come client
        /// </summary>
        public void OpenTcpConnection()
        {
            try
            {
                //Create the remote endpoint for the socket
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_remoteIP), _remotePort);
                ////Create TCP/IP socket
                //netobj.clientSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ////Start connection to remote endpoint
                //netobj.clientSocket.BeginConnect(remoteEP, new AsyncCallback(TcpConnectionCallback), netobj);
                //Crea socket verso server
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //avvia connessione verso endpoint remoto
                socket.BeginConnect(remoteEP, new AsyncCallback(TcpConnectionCallbackClient), socket);
            }
            catch (Exception ex)
            {
                this._log = "Server is not active. \n Please start server and try again. \n" + ex.ToString();
            }
        }


        private void TcpConnectionCallbackClient(IAsyncResult ar)
        {
            try
            {
                //Retrive network information
                Socket handler = (Socket)ar.AsyncState;

                //complete the connections
                handler.EndConnect(ar);
                //Salva handler
                this._remote = handler;
                NetStateObject netStateObject = new NetStateObject();
                netStateObject.socket = handler;

                //Start to listen for data from the server
                handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), netStateObject);

                this._log = "Connection created";
                //Segnalo avvenuta connessione al server
                OnConnectionStateChanged();
                //Attendo 1,5sec prima di ritornare
                System.Threading.Thread.Sleep(1500);
            }
            catch (Exception ex)
            {
                this._log = "Error: " + ex.Message;
            }
        }



        /// <summary>
        /// Finalizza la connessione, quando la connessione TCP è stabilita solleva evento OnConnectionStateChanged
        /// </summary>
        /// <param name="ar"></param>
        private void TcpConnectionCallbackServer(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            //Salvo il socket per poterlo usare per inviare dati in maniera sincrona
            this._remote = handler;


            NetStateObject netStateObject = new NetStateObject();
            netStateObject.socket = handler;
            //Mette in ascolto l'oggetto per comunicazioni 
            handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                new AsyncCallback(ReceiveCallback), netStateObject);

            this.OnConnectionStateChanged();
        }

        //Async received data callback method
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the NetObject and the client socket from the asynchronous state object
                NetStateObject netStateObj = (NetStateObject)ar.AsyncState;
                Socket handler = netStateObj.socket;
                //read data from the remote server
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    
                    int total = 0;
                    int recv;

                    byte[] datasize = new byte[4];
                    //Estrapolare i primi 4 byte perchè rappresetnano la dimensione del messaggio
                    if (netStateObj.recBuffer.Length == 4)
                    {
                        datasize = netStateObj.recBuffer;
                    }else if(netStateObj.recBuffer.Length>4)
                    {
                        for(int i = 0; i < 4; i++)
                        {
                            datasize[i] = netStateObj.recBuffer[i];
                        }
                    }

                    int size = BitConverter.ToInt32(datasize,0);

                    int dataleft = size;
                    byte[] data = new byte[size];
                    while (total < size) {
                        recv = netStateObj.socket.Receive(data, total, dataleft, 0);                                                       
                        if (recv == 0) {
                            data = Encoding.ASCII.GetBytes("exit");
                            break;
                        }
                        total += recv;
                        dataleft -= recv;
                    }                    

                    this._receivedMex = System.Text.Encoding.Unicode.GetString(data);
                    OnMessageReceived();
                    //Rimango in ascolto di altri messaggi
                    netStateObj.socket.BeginReceive(netStateObj.recBuffer, 0, netStateObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netStateObj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message); 
            }
        }

        public int SendVarData(string mex)
        {
            int total = 0;
            int size = mex.Length;
            int dataleft = size;
            int sent;

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

        /// <summary>
        /// Close the tcp connection
        /// </summary>
        public void closeConnection()
        {
            if (this._remoteIsConnected)
            {
                //Segnalo connessione disconnessione del client
                this._remote.Shutdown(SocketShutdown.Both);
                this._remote.Disconnect(true);
                this._remote.Close();
                this._remote = null;

                this._listener.Close();
                this._listener = null;


                this.OnConnectionStateChanged();
            }
        }

        #endregion
    }
}