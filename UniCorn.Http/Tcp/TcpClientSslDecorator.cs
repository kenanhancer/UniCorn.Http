using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace UniCorn.Http
{
    public class TcpClientSslDecorator : ITcpClient
    {
        private readonly ITcpClient tcpClient;

        public TcpClient AcceptedTcpClient => tcpClient.AcceptedTcpClient;

        private readonly SslStream sslStream;

        public Stream Stream => sslStream;

        public bool Connected => tcpClient.Connected;

        public IPEndPoint LocalEndPoint => tcpClient.LocalEndPoint;

        public IPEndPoint RemoteEndPoint => tcpClient.RemoteEndPoint;

        public TcpClientSslDecorator(ITcpClient tcpClient, X509Certificate certificate)
        {
            this.tcpClient = tcpClient;
            sslStream = new SslStream(tcpClient.Stream);
            //sslStream = new SslStream(tcpClient.Stream, true, ValidateServerCertificate);

            sslStream.AuthenticateAsServerAsync(certificate, false, System.Security.Authentication.SslProtocols.Tls, true).Wait();
        }

        public void Dispose() => tcpClient.Dispose();
    }
}