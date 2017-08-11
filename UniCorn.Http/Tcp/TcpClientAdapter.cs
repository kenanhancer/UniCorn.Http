using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UniCorn.Http
{
    public class TcpClientAdapter : ITcpClient
    {
        private readonly TcpClient tcpClient;

        public TcpClient AcceptedTcpClient => tcpClient;

        public Stream Stream => tcpClient.GetStream();

        public bool Connected => tcpClient.Connected;

        public IPEndPoint LocalEndPoint => (IPEndPoint)tcpClient.Client.LocalEndPoint;

        public IPEndPoint RemoteEndPoint => (IPEndPoint)tcpClient.Client.RemoteEndPoint;

        public TcpClientAdapter(TcpClient tcpClient) => this.tcpClient = tcpClient;

        public void Dispose() => tcpClient.Dispose();
    }
}