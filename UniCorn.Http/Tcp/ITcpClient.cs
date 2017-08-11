using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UniCorn.Http
{
    public interface ITcpClient : IDisposable
    {
        TcpClient AcceptedTcpClient { get; }

        Stream Stream { get; }

        bool Connected { get; }

        IPEndPoint LocalEndPoint { get; }

        IPEndPoint RemoteEndPoint { get; }
    }
}