using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBUtility
{
    public class DBManagerPool
    {
        private string _strIp;
        private string _strDatabase;
        private string _strUsername;
        private string _strPassword;
        private string _strPort;

        private int _poolCount = 0;

        private List<DBManager> _lstDBMng = new List<DBManager>();
        public List<DBManager> DBMngLst
        { get { return _lstDBMng; } }

        private List<Thread> _lstDBThread = new List<Thread>();
        public Dictionary<int, int> m_diDBCallCount = new Dictionary<int, int>();
        public Dictionary<string, int> m_diDBCallName = new Dictionary<string, int>();

        public DBManagerPool(int count)
        {
            _poolCount = count;
            for (int i = 0; i < _poolCount; i++)
            {
                DBManager db = new DBManager();
                _lstDBMng.Add(db);
                m_diDBCallCount.Add(i,0);
            }
        }

        public bool Init(string ip, string database, string username, string password, string port)
        {
            this._strIp = ip;
            this._strDatabase = database;
            this._strUsername = username;
            this._strPassword = password;
            this._strPort = port;
            foreach (var db in DBMngLst)
            {
                if (!db.Init(_strIp, _strDatabase, _strUsername, _strPassword, _strPort))
                {
                    return false;
                }
                Thread dbThread = new System.Threading.Thread(db.Run);
                _lstDBThread.Add(dbThread);
                dbThread.Start();
            }
            return true;
        }

    }
}
