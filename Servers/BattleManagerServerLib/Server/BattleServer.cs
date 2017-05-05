using Logger;
using ServerShared;
using ServerSocket.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private void InitTcp()
        {
            Tcp.OnRead = OnRead;
            Tcp.OnAccept = OnAccept;
            Tcp.OnDisconnect = OnDisconnect;
        }

        private void OnDisconnect()
        {
            throw new NotImplementedException();
        }

        private void OnAccept(bool ret)
        {
            throw new NotImplementedException();
        }

        private void OnRead(MemoryStream stream)
        {
            throw new NotImplementedException();
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

        protected void OnAccept(bool ret)
        {
            string log = string.Format("battle {0} sub {1} connected!");
            lock (LogList[LogType.INFO])
            {
                LogList[LogType.INFO].Enqueue(log);
            }
            State = ServerState.Starting;
            BattleServerManager.BindServer(this);
        }

        protected void OnDisconnect()
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
                    Reset();
                    State = ServerState.Stopped;
                }
            }
        }

        private void OnProcessProtocol()
        {
            throw new NotImplementedException();
        }
    }
}
