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
        private byte[] recvStream = new byte[4096];
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
        enum State
        {
            IDLE = 0,
            WAIT,
            RUN,
            CLOSE,
        }

        IList<ArraySegment<byte>> sendStreams = new List<ArraySegment<byte>>();
        IList<ArraySegment<byte>> waitStreams = new List<ArraySegment<byte>>();
        private int _waitStreamsCount;
        public int WaitStreamsCount
        { get { return _waitStreamsCount; } }
        public string IP { get; set; }
        private Socket _socket = null;
        private ushort _port = 0;
        private int _state = (int)State.IDLE;
        private int _offset = 0;
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
            if (Interlocked.CompareExchange(ref _state, (int)State.WAIT, (int)State.IDLE) != (int)State.IDLE)
            {
                return false;
            }
            Socket listenSocket = TcpMng.Accept(port);
            if (listenSocket == null)
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
                LOG.Error("tcp Accept exception :{0}", e.ToString());
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
                    LOG.Error("tcp ListenComplete exception:{0}", e.ToString());
                }
            }
            try
            {
                _socket = listenSocket.EndAccept(ar);
                if (Interlocked.CompareExchange(ref _state, (int)State.RUN, (int)State.WAIT) != (int)State.WAIT)
                {
                    _socket.Close();
                    _socket = null;
                    OnAccept(false);
                    return;
                }
                _socket.BeginReceive(recvStream, 0, 2048, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                IP = _socket.RemoteEndPoint.ToString().Split(':')[0];
                OnAccept(true);
                return;
            }
            catch (Exception e)
            {
                LOG.Error("tcp ListenComplete _socket exception:{0}", e.ToString());
            }
            _socket = null;
            OnAccept(false);
        }
        private void RecvComplete(IAsyncResult ar)
        {
            SocketError error;
            if (_socket == null)
            {
                return;
            }
            try
            {
                int len = (int)_socket.EndReceive(ar, out error);
                if (len <= 0)
                {
                    Disconnect();
                    return;
                }
                len = _offset + len;
                MemoryStream transfered = new MemoryStream(recvStream, 0, len, true, true);
                if (OnRead!=null)
                {
                    OnRead(transfered);
                }
                _offset = (int)len - (int)transfered.Position;
                if (_offset<0)
                {
                    Disconnect();
                    return;
                }
                int size = 16384;
                if (transfered.Position==0)
                {
                    size = (int)(transfered.Length * 2);
                }
                if (size>65535)
                {
                    Disconnect();
                    return;
                }
                byte[] buffer = new byte[size];
                Array.Copy(recvStream, transfered.Position, buffer, 0, _offset);
                recvStream = buffer;
                _socket.BeginReceive(recvStream, _offset, size - _offset, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                return;
            }
            catch (Exception e)
            {
                LOG.Error("tcp RecvComplete exception:{0}", e.ToString());
            }
        }
        private void Disconnect()
        {
            if (_socket == null)
            {
                return;
            }
            if (_state == (int)State.IDLE)
            {
                return;
            }
            _state = (int)State.IDLE;
            _socket.Close(0);
            _socket = null;
            waitStreams.Clear();
            sendStreams.Clear();
            _offset = 0;
            if (OnDisconnect != null)
            {
                TcpMng.OnDisconnectCallbacks.Enqueue(OnDisconnect);
            }
        }
        public bool Connect(string ip, ushort port)
        {
            if (_socket != null)
            {
                return false;
            }
            if (!TcpMng.IsAvailable)
            {
                return false;
            }
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.BeginConnect(ip, port, ConnectComplete, null);
                return true;
            }
            catch (Exception e)
            {
                LOG.Error("tcp Connect exception :{0}", e.ToString());
            }
            return false;
        }
        private void ConnectComplete(IAsyncResult ar)
        {
            if (_socket == null)
            {
                OnConnect(false);
                return;
            }
            try
            {
                _socket.EndConnect(ar);
                if (Interlocked.CompareExchange(ref _state, (int)State.RUN, (int)State.WAIT) != (int)State.WAIT)
                {
                    _socket.Close();
                    _socket = null;
                    OnConnect(false);
                    return;
                }
                _state = (int)State.RUN;
                _socket.BeginReceive(recvStream, 0, 2048, SocketFlags.None, new AsyncCallback(RecvComplete), null);
                OnConnect(true);
                return;
            }
            catch (Exception e)
            {
                LOG.Error("tcp ConnectComplete exception :{0}", e.ToString());
            }
            _socket = null;
            _state = (int)State.IDLE;
            OnConnect(false);
        }
        public bool IsClosed()
        {
            lock (this)
            {
                if (_socket == null)
                {
                    return true;
                }
                if (_state != (int)State.RUN)
                {
                    return true;
                }
                return false;
            }
        }
        public bool Write(MemoryStream stream)
        {
            if (stream.Length == 0)
            {
                return true;
            }
            stream.Seek(0, SeekOrigin.Begin);
            lock (this)
            {
                if (_state != (int)State.RUN)
                {
                    return false;
                }
                ArraySegment<byte> segment = new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(segment);
                    try
                    {
                        _socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception e)
                    {
                        LOG.Error("tcp  Write exception :{0}", e.ToString());
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(segment);
                    _waitStreamsCount = waitStreams.Count;
                }
            }
            return true;
        }
        public bool Write(MemoryStream head, MemoryStream body)
        {
            head.Seek(0, SeekOrigin.Begin);
            body.Seek(0, SeekOrigin.Begin);
            if (body.Length == 0)
            {
                return Write(head);
            }
            lock (this)
            {
                if (_state != (int)State.RUN)
                {
                    return false;
                }
                ArraySegment<byte> frist = new ArraySegment<byte>(head.GetBuffer(), 0, (int)head.Length);
                ArraySegment<byte> second = new ArraySegment<byte>(body.GetBuffer(), 0, (int)body.Length);
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(frist);
                    sendStreams.Add(second);
                    try
                    {
                        _socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception e)
                    {
                        LOG.Error("tcp Write exception :{0}", e.ToString());
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(frist);
                    waitStreams.Add(second);
                    _waitStreamsCount = waitStreams.Count;
                }
            }
            return true;
        }
        /// <summary>
        /// 发送字节数组
        /// </summary>
        /// <param name="first">报头</param>
        /// <param name="second">报文</param>
        /// <returns></returns>
        private bool Write(ArraySegment<byte> first, ArraySegment<byte> second)
        {
            lock (this)
            {
                if (_state!=(int)State.RUN)
                {
                    return false;
                }
                if (sendStreams.Count == 0)
                {
                    sendStreams.Add(first);
                    sendStreams.Add(second);
                    try
                    {
                        _socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                    catch (Exception e)
                    {
                        LOG.Error("tcp Write exception :{0}", e.ToString());
                        return false;
                    }
                }
                else
                {
                    waitStreams.Add(first);
                    waitStreams.Add(second);
                    _waitStreamsCount = waitStreams.Count;
                }
            }
            return true;
        }
        private void SendComplete(IAsyncResult ar)
        {
            try
            {
                int len = _socket.EndSend(ar);
                if (len == 0)
                {
                    return;
                }
                lock (this)
                {
                    sendStreams.Clear();
                    if (waitStreams.Count > 0)
                    {
                        IList<ArraySegment<byte>> temp = sendStreams;
                        sendStreams = waitStreams;
                        waitStreams = temp;
                        _socket.BeginSend(sendStreams, SocketFlags.None, SendComplete, null);
                    }
                }
            }
            catch (Exception e)
            {
                LOG.Error("tcp SendComplete exception :{0}", e.ToString());
            }
        }
    }
}
