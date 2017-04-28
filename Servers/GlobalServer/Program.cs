using System;
using GlobalServerLib.Server;
using Logger;
using System.Threading;
using ServerShared;
using GlobalServerLib;

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
                api.StartMode = Mode.Auto;
                api.Init(args);
            }
            catch (Exception e)
            {
                LOG.Error("Global init failed:{0}", e.ToString());
                api.Exit();
                return;
            }

            Thread thread = new Thread(api.Run);
            thread.Start();

            LOG.Info("Global Server OnReady..");

            while (thread.IsAlive)
            {
                api.ProcessInput();
                Thread.Sleep(1000);
            }

            api.Exit();
            LOG.Info("Global Server Exit..");
        }
    }
}
