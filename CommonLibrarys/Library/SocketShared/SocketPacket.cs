using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketShared
{
    public class SocketHeader:ICloneable
    {
        public UInt16 length;
        public UInt32 msg;

        /// <summary>
        /// 报头
        /// </summary>
        static readonly public int Size = sizeof(UInt16) + sizeof(Int32);
        public SocketHeader()
        {
            length = 0;
            msg = 0;
        }
        public SocketHeader(byte[] packetHeader)
        {
            length = BitConverter.ToUInt16(packetHeader, 0);
            msg = BitConverter.ToUInt32(packetHeader, sizeof(UInt16));
        }
        public SocketHeader(UInt16 length,UInt32 msg)
        {
            this.length = length;
            this.msg = msg;
        }
        public object Clone()
        {
            var clone = new SocketHeader();
            clone.length = length;
            clone.msg = msg;
            return clone;
        }
    }
    /// <summary>
    /// 报文
    /// </summary>
    public class SocketPacket:ICloneable
    {
        public SocketHeader header = new SocketHeader();
        public byte[] body;
        public SocketPacket(SocketHeader header)
        {
            this.header = (SocketHeader)header.Clone();
            this.body = new byte[this.header.length];
        }
        public SocketPacket(SocketHeader header,byte [] packetData)
        {
            this.header = (SocketHeader)header.Clone();
            this.body = new byte[this.header.length];
            Buffer.BlockCopy(packetData, 0, this.body, 0, this.header.length);
        }
        public object Clone()
        {
            return new SocketPacket(header, body);
        }
    }
}
