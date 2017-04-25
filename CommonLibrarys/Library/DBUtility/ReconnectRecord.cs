using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUtility
{
    public class ReconnectRecord
    {
        private double m_dTryConnectTime = 0.0;
        private double m_dMaxConnectTime;
        private bool m_bNeedReconnect = false;
        public void Init(double maxConnectTime)
        {
            m_dTryConnectTime = 0.0;
            m_dMaxConnectTime = maxConnectTime;
            m_bNeedReconnect = false;
        }
        public void Reset()
        {
            m_dTryConnectTime = 0.0;
            m_bNeedReconnect = false;
        }

    }
}
