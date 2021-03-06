﻿using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleManagerServerLib.Server
{
    public class BattleSeverManager
    {
           Api _api = null;
        public Api Api
        {
            get { return _api; }
        }

        public BattleSeverManager(Api api)
        {
            this._api = api;
        }

        public List<BattleServer> AllBattleServers = new List<BattleServer>();

        public Dictionary<string, BattleServer> BattleServerList = new Dictionary<string, BattleServer>();

        private object allBattleServersLock = new object();

        public void UpdateServers()
        {
            lock (allBattleServersLock)   //frTODO:这里这个allBattleServersLock  不懂？？？？
            {
                foreach (var item in AllBattleServers)
                {
                    try
                    {
                        item.Update();
                    }
                    catch (Exception e)
                    {
                        LOG.Error(e.ToString());
                    }
                }
            }
        }

        public void BindServer(BattleServer battleServer)
        {
            if (battleServer == null)
            {
                LOG.Error("Bind battleserver failed:server is null!");
                return;
            }
            lock(allBattleServersLock)
            {
                AllBattleServers.Add(battleServer);
            }
        }


        internal void DistoryServer(BattleServer battleServer)
        {
            if (battleServer == null)
            {
                LOG.Error("Bind battleserver failed:server is null!");
                return;
            }
            lock (allBattleServersLock)
            {
                AllBattleServers.Remove(battleServer);
                string key = battleServer.GetKey();
                BattleServerList.Remove(key);
            }
        }
    }
}
