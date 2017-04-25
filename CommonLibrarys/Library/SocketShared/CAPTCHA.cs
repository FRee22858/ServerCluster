using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketShared
{
    class CAPTCHA
    {
        static public class CAPTCHA
        {
            static public int EncodeCaptcha(string count,int server)
            {
                int temp = count.GetHashCode();
                return temp + server;
            }
            static public bool CheckCAPTCHA(string count,int server,int Captcha)
            {
                if (EncodeCaptcha(count,server)==Captcha)
                {
                    return true;                    
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
