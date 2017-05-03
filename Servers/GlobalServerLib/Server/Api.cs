using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalServerLib.Server
{
    public class Api:AbstractServer
    {

        public override void InitModule(string[] args)
        {

            return;
        }

        public override void Exit()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            Thread.Sleep(10);
        }


    }
}
