using CommonUtility;
using Logger;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUtility
{
    public class DBManager
    {
        private string _strIp;
        private string _strDatabase;
        private string _strUsername;
        private string _strPassword;
        private string _strPort;
        private bool m_bOpened = false;

        private MySqlConnection _conn = null;
        public MySqlConnection Conn
        {
            get { return _conn; }
        }
        private Queue<AbstractDBQuery> _saveQueue;
        private Queue<AbstractDBQuery> _executionQueue = new Queue<AbstractDBQuery>();
        private Queue<AbstractDBQuery> _postUpdateQueue = new Queue<AbstractDBQuery>();
        private Queue<string> _exceptionLogQueue = new Queue<string>();

        public ReconnectRecord m_cReconnectInfo;

        public bool Init(string ip, string database, string username, string password, string port)
        {
            m_cReconnectInfo = new ReconnectRecord();
            m_cReconnectInfo.Init(60 * 1000);
            _saveQueue = new Queue<AbstractDBQuery>();
            _postUpdateQueue = new Queue<AbstractDBQuery>();

            this._strIp = ip;
            this._strDatabase = database;
            this._strUsername = username;
            this._strPassword = password;
            this._strPort = port;

            string strConn = string.Format("data source={0}; database={1}; user id={2}; password={3}; port={4}", _strIp, _strDatabase, _strUsername, _strPassword, _strPort);
            try
            {
                _conn = new MySqlConnection(strConn);
                _conn.Open();
                m_bOpened = true;
                return true;
            }
            catch (MySqlException e)
            {
                LOG.Error(e.ToString());
                return false;
            }

        }
        public bool Exit()
        {
            try
            {
                if (_conn != null)
                {
                    Conn.Close();
                    m_bOpened = false;
                    _conn = null;
                }
                return true;
            }
            catch (MySqlException e)
            {
                LOG.Error(e.ToString());
                return false;
            }
        }
        public bool IsDisconnected()
        {
            return (Conn.State == System.Data.ConnectionState.Closed || Conn.State == System.Data.ConnectionState.Broken);
        }

        public void AddDBQuery(AbstractDBQuery query)
        {
            query.Init(Conn);
            lock (_saveQueue)
            {
                _saveQueue.Enqueue(query);
            }
        }
        //Asynchronous
        public void Run()
        {
            var tempPostUpdateQueue = new Queue<AbstractDBQuery>();
            var time = new SystemTime();
        }

    }
}
