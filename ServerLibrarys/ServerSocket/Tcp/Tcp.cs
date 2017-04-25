using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logger;

namespace ServerSocket.Tcp
{
    public class Tcp
    {

        private byte[] recvstream = new byte[4096]; 
        private bool _needListenHeartbeat = true;
        public bool NeedListenHeartbeat
        {
            get { return _needListenHeartbeat; }
            set { _needListenHeartbeat = value; }
        }

        public delegate void AsyncReadCallback(MemoryStream stream);
        public delegate void AsyncConnectCallback(bool ret);
        public delegate void AsyncAcceptCallback(bool ret);
        public delegate void AsyncDisconnectCallback();

        private AsyncReadCallback onRead = DefaultOnRead;
        public AsyncReadCallback OnRead
        {
            set { onRead = value; }
            get { return onRead; }
        }

        private AsyncConnectCallback onConnect = DefaultOnConnect;
        public AsyncConnectCallback OnConnect
        {
            set { onConnect = value; }
            get { return onConnect; }
        }

        private AsyncAcceptCallback onAccept = DefaultOnAccept;
        public AsyncAcceptCallback OnAccept
        {
            set { onAccept = value; }
            get { return onAccept; }
        }

        private AsyncDisconnectCallback onDisconnect = DefaultOnDisconnect;
        public AsyncDisconnectCallback OnDisconnect
        {
            set { onDisconnect = value; }
            get { return onDisconnect; }
        }

        static private void DefaultOnRead(MemoryStream stream)
        {
            stream.Seek(0, SeekOrigin.End);
        }
        static private void DefaultOnConnect(bool ret)
        {
            Console.WriteLine("default on DefaultOnConnect function called,check it");
        }
        static private void DefaultOnAccept(bool ret)
        {
            Console.WriteLine("default on DefaultOnAccept function called,check it");
        }
        static private void DefaultOnDisconnect()
        {
            Console.WriteLine("default on DefaultOnDisconnect function called,check it");
        }

        IList<ArraySegment<byte>> sendStream = new List<ArraySegment<byte>>();
        IList<ArraySegment<byte>> waitStream = new List<ArraySegment<byte>>();

        enum State
        {
            IDLE=0,
            WAIT,
            RUN,
            CLOSE,
        }
        public string IP { get; set; }
        private Socket _socket = null;
        private ushort _port = 0;
        private int _state = (int)State.IDLE;
        private int offset = 0; 
        public bool Accept(ushort port)
        {
            this._port = port;
            if (_socket != null)
            {
                return false;
            }
            if (!TcpMng.IsAvailable)
            {
                return false;
            }
            if (Interlocked.CompareExchange(ref _state,(int)State.WAIT,(int)State.IDLE)!=(int)State.IDLE)
            {
                return false;
            }
            Socket listenSocket = TcpMng.Accept(port);
            if (listenSocket==null)
            {
                return false;
            }
            try
            {
                listenSocket.BeginAccept(new AsyncCallback(ListenComplete), listenSocket);
                return true;
            }
            catch (Exception e)
            {
                LOG.Error("tcp accept exception :{0}",e.ToString());
            }
            return false;
        }

        private void ListenComplete(IAsyncResult ar)
        {
            Socket listenSocket = (Socket)ar.AsyncState;
            if (_needListenHeartbeat)
            {
                try
                {
                    TcpMng.Listen(_port);
                }
                catch (Exception e)
                {
                    LOG.Error("ListenComplete Listen exception:{0}", e.ToString());
                }
            }
            try
            {
                _socket = listenSocket.EndAccept(ar);
                if (Interlocked.CompareExchange(ref _state,(int)State.RUN,(int)State.WAIT)!=(int)State.WAIT)
                {
                    _socket.Close();
                    _socket = null;
                    OnAccept(false);
                    return;
                }
                _socket.BeginReceive(recvstream, 0, 2048, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                IP = _socket.RemoteEndPoint.ToString().Split(':')[0];
                OnAccept(true);
                return;
            }
            catch (Exception e)
            {
                LOG.Error("ListenComplete _socket exception:{0}", e.ToString());
            }
            _socket = null;
            OnAccept(false);
        }

        private void RecvComplete(IAsyncResult ar)
        {
            SocketError error;
            if (_socket==null)
            {
                return;
            }
            try
            {
                int len = (int)_socket.EndReceive(ar, out error);
                if(len <=0)
                {
                    Disconnect();
                    return;
                }
            }
            catch (Exception e)
            {
                LOG.Error("RecvComplete _socket exception:{0}", e.ToString());
            }
        }

        private void Disconnect()
        {
            if (_socket==null)
            {
                return;
            }
            if (_state==(int)State.IDLE)
            {
                return;
            }
            _state = (int)State.IDLE;
            _socket.Close(0);
            _socket = null;
            waitStream.Clear();
            sendStream.Clear();
            offset = 0;
            if (OnDisconnect!=null)
            {
                TcpMng.OnDisconnectCallbacks.Enqueue(OnDisconnect);
            }
        }
    }
}
