
using System;
using System.ComponentModel;
using System.Runtime.Serialization;


namespace ClientPDS
{

    public class ProcessInfoModel { }
    
     
    public class ProcessInfo : INotifyPropertyChanged
        {

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        private int _pid;
        public int Pid
        {
            get { return _pid; }
            set
            {
                if (value != _pid)
                {
                    _pid = value;
                    RaisePropertyChanged("Pid");
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
                    RaisePropertyChanged("Title");
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
                    RaisePropertyChanged("Path");
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
                    RaisePropertyChanged("Icon");
                }
            }
        }

        /// <summary>
        /// Istante di tempo nel quale l'applicazione riceve il focus
        /// </summary>
        private TimeSpan _focusTimeStamp;
        public TimeSpan FocusTimeStamp
        {
            get {
                return _focusTimeStamp;
            }
            set
            {
                if (value != _focusTimeStamp)
                    _focusTimeStamp = value;
            }
        }
        /// <summary>
        /// Total time the process is focused
        /// </summary>
        private TimeSpan _totalFocusTime ;
        public TimeSpan TotalFocusTime
        {
            get { return _totalFocusTime; }
            set { if (value != _totalFocusTime)
                    _totalFocusTime = value;
            }
        }

        /// <summary>
        /// Total percentge time 
        /// </summary>
        private double _focusPercentage;
        public double FocusPercentage
        {
            get { return _focusPercentage; }
            set { if (_focusPercentage != value)
                    _focusPercentage = value;
                RaisePropertyChanged("FocusPercentage");
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
}















