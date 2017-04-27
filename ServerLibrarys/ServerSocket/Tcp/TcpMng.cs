using Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSocket.Tcp
{
    class TcpMng
    {
        static bool _bIsAvailable = false;
        static public bool IsAvailable
        { get { return _bIsAvailable; } }
        static public void Begin()
        {
            _bIsAvailable = true;
        }
        static public void End()
        {
            _bIsAvailable = false;
        }

        /// <summary>
        /// listen sockets
        /// </summary>
        static Dictionary<ushort, Socket> _listeners = new Dictionary<ushort, Socket>();
        /// <summary>
        /// heartbeats
        /// </summary>
        /// <param name="port"></param>
        public delegate void Heartbeat(ushort port);
        static Dictionary<ushort, Heartbeat> _heartbeats = new Dictionary<ushort, Heartbeat>();
        /// <summary>
        /// disconnect asyncdisconnect callback
        /// </summary>
        public delegate void OnDisconnectCallback();
        static internal ConcurrentQueue<Tcp.AsyncDisconnectCallback> OnDisconnectCallbacks = new ConcurrentQueue<Tcp.AsyncDisconnectCallback>();

        static private void DefaultHearbeat(ushort port)
        {
            Console.WriteLine("===============use default heart beat,check it====================");
            new Tcp().Accept(port);
        }
        static internal Socket Accept(ushort port)
        {
            if (IsAvailable ==false)
            {
                return null;
            }
            Socket socket;
            if (_listeners.TryGetValue(port, out socket) == true)
            {
                return socket;
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listeners.Add(port, socket);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
            socket.Bind(ep);
            socket.Listen(10);
            socket.SendBufferSize = 16384;
            return socket;
        }
        static public void Listen(ushort port)
        {
                     if (IsAvailable ==false)
            {
                return ;
            }
            Heartbeat heartbeat = null;
            if (_heartbeats.TryGetValue(port,out heartbeat)==true)
            {
                heartbeat(port);
            }
            else
            {
                _heartbeats.Add(port, DefaultHearbeat);
                DefaultHearbeat(port);
            }
        }
        static public bool Listen(ushort port,Heartbeat heartbeat)
        {
            return Listen(port, 128, heartbeat);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="backlog">挂起连接队列的最大长度</param>
        /// <param name="heartbeat"></param>
        /// <returns></returns>
        static public bool Listen(ushort port,ushort backlog,Heartbeat heartbeat)
        {
            if (IsAvailable == false)
            {
                return false;
            }
            _heartbeats.Add(port, heartbeat);
            for (int i = 0; i < backlog; i++)
            {
                heartbeat(port);
            }
            return true;
        }
        static public void Update()
        {
            int count = OnDisconnectCallbacks.Count;
            Tcp.AsyncDisconnectCallback disconnectCallback;
            for (int i = 0; i < count; i++)
            {
                if (OnDisconnectCallbacks.TryDequeue(out disconnectCallback))
                {
                    if (disconnectCallback == null)
                    {
                        return;
                    }
                    else
                    {
                        try
                        {
                            disconnectCallback();
                        }
                        catch (Exception e)
                        {
                            LOG.Error(e.ToString());
                        }
                    }
                }
            }
        }

    }
}
