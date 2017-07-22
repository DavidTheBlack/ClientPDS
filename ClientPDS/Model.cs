
using System.ComponentModel;
using System.Runtime.Serialization;

//@TODO mettere un mutex per rendere il tutto thread safe

namespace ClientPDS
{

    public class ProcessInfoModel { }
    
     
    public class ProcessInfo //: INotifyPropertyChanged
        {

        //public event PropertyChangedEventHandler PropertyChanged;
        //private void RaisePropertyChanged(string property)
        //{
         //   if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(property));
        //    }
        //}


        private int _pid;
        public int Pid
        {
            get { return _pid; }
            set
            {
                if (value != _pid)
                {
                    _pid = value;
                    //RaisePropertyChanged("Pid");
                }
            }
        }

        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    //RaisePropertyChanged("Title");
                }                
            }
        }

        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                if (value != _path)
                {
                    _path = value;
                    //RaisePropertyChanged("Path");
                }
            }
        }

        private byte[] _icon;
        public byte[] Icon
        {
            get
            {
                return _icon;
            }

            set
            {
                if (value != _icon)
                {
                    _icon = value;
                    //RaisePropertyChanged("Icon");
                }
            }
        }
    }

    /// <summary>
    /// Data contract class for deserialization
    /// </summary>
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

    //public class AdditionalInfoModel : INotifyPropertyChanged
    //{
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void RaisePropertyChanged(string property)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(property));
    //        }
    //    }

    //    private int _focusedPid;
    //    public int FocusedPid
    //    {
    //        get
    //        {
    //            return _focusedPid;
    //        }

    //        set
    //        {
    //            if (_focusedPid != value)
    //            {
    //                _focusedPid = value;
    //                RaisePropertyChanged("FocusedPid");
    //            }

    //        }
    //    }

    //    private bool _ipTextEnabled;
    //    public bool IpTextEnabled
    //    {
    //        get
    //        {
    //            return _ipTextEnabled;
    //        }

    //        set
    //        {
    //            if(_ipTextEnabled != value)
    //            {
    //                _ipTextEnabled = value;
    //                RaisePropertyChanged("IpTextEnabled");
    //            }
    //        }
    //    }

    //    private string _serverIP;
    //    public string ServerIP
    //    {
    //        get
    //        {
    //            return _serverIP;
    //        }

    //        set
    //        {
    //            if (_serverIP != value)
    //            {
    //                _serverIP = value;
    //                RaisePropertyChanged("ServerIP");
    //            }

    //        }
    //    }

    //    private string _buttonText;
    //    public string ButtonText
    //    {
    //        get { return _buttonText; }
    //        set
    //        {
    //            if (value != _buttonText)
    //            {
    //                _buttonText = value;
    //                RaisePropertyChanged("ButtonText");
    //            }
    //        }
    //    }

    //    public AdditionalInfoModel()
    //    {
    //        FocusedPid = 0;
    //        IpTextEnabled = true;
    //        ServerIP = "192.168.43.101";
    //        ButtonText = "Connect";
    //    }

    //}


}















