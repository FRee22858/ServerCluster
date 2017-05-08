using Logger;
using ServerSocket.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerShared;
using SocketShared;
using ServerProtocol;

namespace BattleManagerServerLib.Server
{
    public class BattleServer
    {
        ServerState _state = ServerState.Stopped;
        public ServerState State
        {
            get { return _state; }
            set { _state = value; }
        }

        int _mainId = 0;
        public int MainId
        {
            get { return _mainId; }
        }
        int _subId = 0;
        public int SubId
        {
            get { return _subId; }
        }

        Tcp _tcp = new Tcp();
        public Tcp Tcp
        {
            get { return _tcp; }
        }

        ushort _listenPort;
        public ushort ListenPort
        {
            get { return _listenPort; }
        }

        Api _api;
        public Api Api
        {
            get { return _api; }
        }

        BattleSeverManager _battleServerManager;
        internal BattleSeverManager BattleServerManager
        {
            get { return _battleServerManager; }
        }
        public BattleServer(Api api, BattleSeverManager battleServerManager, ushort listenPort)
        {
            _api = api;
            _battleServerManager = battleServerManager;
            _listenPort = listenPort;
        }

        public Dictionary<LogType, Queue<string>> LogList = new Dictionary<LogType, Queue<string>>();

        public void Init()
        {
            InitTcp();
            StartListen(ListenPort);
            BindResponser();
            InitLogList();
        }
        private void InitLogList()
        {
            LogList.Add(LogType.INFO, new Queue<string>());
            LogList.Add(LogType.WARN, new Queue<string>());
            LogList.Add(LogType.ERROR, new Queue<string>());
        }
        private void InitTcp()
        {
            Tcp.OnRead = OnRead;
            Tcp.OnAccept = OnAccept;
            Tcp.OnDisconnect = OnDisconnect;
        }

        private void StartListen(ushort listenPort)
        {
            Tcp.NeedListenHeartbeat = true;
            Tcp.Accept(listenPort);
        }

        public List<DateTime> DBExceptionList = new List<DateTime>();

        public delegate void Responser(MemoryStream stream);
        private Dictionary<uint, Responser> _responserList = new Dictionary<uint, Responser>();
        public Dictionary<uint, Responser> ResponserList
        {
            get { return _responserList; }
        }

        public void AddResponser(uint id, Responser responser)
        {
            _responserList.Add(id, responser);
        }

        void BindResponser()
        {
            //frTODO:绑定协议应答函数
            //ResponserEnd;
        }

        public void Update()
        {
            //frTODO:
            //判断服务器连接状态
            if (State == ServerState.Started || State == ServerState.Starting)
            {
                OnProcessProtocol();
            }
            UpdateLogList();
        }

        public void UpdateLogList()
        {
            lock (LogList)
            {
                while (LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.INFO].Dequeue();
                        LOG.Info(log);
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e.ToString());
                    }
                }
                while (LogList[LogType.WARN].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.WARN].Dequeue();
                        LOG.Warn(log);
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e.ToString());
                    }
                }
                while (LogList[LogType.ERROR].Count > 0)
                {
                    try
                    {
                        string log = LogList[LogType.ERROR].Dequeue();
                        LOG.Error(log);
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e.ToString());
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stream"></param>
        public void OnResponse(uint id,MemoryStream stream)
        {
            Responser responser = null;
            if (ResponserList.TryGetValue(id, out responser))
            {
                responser(stream); 
            }
            else
            {
                LOG.Warn("{0} get battle {1} subId {2} unsupported package id {3}", Api.ServerName, MainId, SubId, id);
            }
        }

        private void OnAccept(bool ret)
        {
            string log = string.Format("battle {0} sub {1} connected!");
            lock (LogList[LogType.INFO])
            {
                LogList[LogType.INFO].Enqueue(log);
            }
            State = ServerState.Starting;
            BattleServerManager.BindServer(this);
        }

        private void OnRead(MemoryStream stream)
        {
            int startIndex = 0;
            byte[] buffer = stream.GetBuffer();
            while ((stream.Length - startIndex) > sizeof(UInt16))
            {
                UInt16 size = BitConverter.ToUInt16(buffer, startIndex);
                if (size +SocketHeader.Size>stream.Length-startIndex)
                {
                    break;
                }
                UInt32 msgId = BitConverter.ToUInt32(buffer, startIndex + sizeof(UInt16));
                MemoryStream msg = new MemoryStream(buffer, startIndex + SocketHeader.Size, size,true,true);

                lock (_msgQueue)
                {
                    _msgQueue.Enqueue(new KeyValuePair<UInt32, MemoryStream>(msgId, msg));
                    startIndex += (size + SocketHeader.Size);
                }
            }
            stream.Seek(startIndex, SeekOrigin.Begin);
        }

        private void OnDisconnect()
        {
            lock (LogList)
            {
                string log = string.Format("battle {0} sub {1} disconnected", MainId, SubId);
                LogList[LogType.ERROR].Enqueue(log);
                UpdateLogList();
                lock (this)
                {
                    State = ServerState.DisConnect;
                    BattleServerManager.DistoryServer(this);
                    ResetMsgQueue();
                    State = ServerState.Stopped;
                }
            }
        }

        Queue<KeyValuePair<UInt32, MemoryStream>> _msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        Queue<KeyValuePair<UInt32, MemoryStream>> _dealQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        private void OnProcessProtocol()
        {
            lock (_msgQueue)
            {
                while (_msgQueue.Count>0)
                {
                    var msg = _msgQueue.Dequeue();
                    _dealQueue.Enqueue(msg);
                }
            }
            while (_dealQueue.Count>0)
            {
                var msg = _dealQueue.Dequeue();
                OnResponse(msg.Key, msg.Value);
            }
        }

        public bool Write<T> (T msg) where T:global::ProtoBuf.IExtensible
        {
            MemoryStream body = new MemoryStream();
            ProtoBuf.Serializer.Serialize(body, msg);

            MemoryStream header = new MemoryStream(sizeof(ushort)+sizeof(uint));
            ushort len = (ushort)body.Length;
            header.Write(BitConverter.GetBytes(len), 0, 2);
            header.Write(BitConverter.GetBytes(Id<T>.Value), 0, 4);
            return Write(header,body);
        }

        private bool Write(MemoryStream header, MemoryStream body)
        {
            if (Tcp ==null)
            {
                return false;    
            }
            return Tcp.Write(header,body);
        }

        private void ResetMsgQueue()
        {
            _msgQueue.Clear();
            _dealQueue.Clear();
        }

        public string GetKey()
        {
            return string.Format("{0}_{1}", MainId.ToString(), SubId.ToString());
        }

        public string MakeKey(int mainId,int subId)
        {
            return string.Format("{0}_{1}", mainId, subId);
        }
    }
}
