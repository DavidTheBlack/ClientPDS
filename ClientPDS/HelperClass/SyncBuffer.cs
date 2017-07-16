using System;
using System.Collections.Generic;
using System.Threading;

namespace ClientPDS.HelperClass
{
    class SyncBuffer
    {

        private Queue<byte[]> queue;

        /// <summary>
        /// Evento lanciato quando un nuovo messaggio è inserito nella coda
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

        //Syncronization mutex
        private Mutex mut = new Mutex();

        /// <summary>
        /// Public constructor
        /// </summary>
        public SyncBuffer()
        {
            queue= new Queue<byte[]>();
        }

        /// <summary>
        /// Method to push message in queue
        /// </summary>
        public void push(byte[] mex)
        {
            try
            {
                //con waitOne aspettiamo di prendere il controllo del mutex
                mut.WaitOne();
                queue.Enqueue(mex);                
            }
            finally
            {
                //Segnato a tutti che rilascio il mutex
                mut.ReleaseMutex();
            }

            OnMessageReceived();
        }

        /// <summary>
        /// Funzione che preleva il messaggio dalla coda
        /// </summary>
        /// <param name="mex"></param>
        /// <returns></returns>
        public bool pop(out byte[] mex)
        {
            bool ret= false;
            try
            {
                while (true)
                {
                    //con waitOne aspettiamo di prendere il controllo del mutex
                    mut.WaitOne();
                    if (queue.Count != 0)
                    {
                        mex = queue.Dequeue();
                        ret = true;
                        break;
                    }
                    //Rilascio il mutex prima di riciclare e riprenderlo
                    mut.ReleaseMutex();
                }
            }
            finally
            {
                mut.ReleaseMutex();                
            }

            return ret;
        }

    }
}
