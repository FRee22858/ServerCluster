using BattleServerLib.Server;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Api api = new Api();
            try
            {
                api.ServerName = "BattleServer";
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
