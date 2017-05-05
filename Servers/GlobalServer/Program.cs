using System;
using BattleManagerServerLib.Server;
using Logger;
using System.Threading;
using ServerShared;
using BattleManagerServerLib;

namespace GlobalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Api api = new Api();
            try
            {
                api.ServerName = "GlobalServer";
                api.Init(args);
            }
            catch (Exception e)
            {
                LOG.Error("{0} init failed:{1}", api.ServerName, e.ToString());
                api.Exit();
                return;
            }

            Thread thread = new Thread(api.Run);
            thread.Start();

            LOG.Info("{0} OnReady..", api.ServerName);

            while (thread.IsAlive)
            {
                api.ProcessInput();
                Thread.Sleep(1000);
            }

            api.Exit();
            LOG.Info("{0} Exit..", api.ServerName);
        }
    }
}
