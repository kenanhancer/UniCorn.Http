using System.Net;
using System.Net.Sockets;

namespace UniCorn.Http
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPEndPoint localEP) :base(localEP)
        {
        }

        public TcpListenerEx(IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        public new bool Active
        {
            get { return base.Active; }
        }
    }
}