using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public class TcpListenerSslDecorator : ITcpListener
    {
        private readonly ITcpListener tcpListener;
        private readonly X509Certificate certificate;

        public TcpListenerSslDecorator(ITcpListener tcpListener, X509Certificate certificate)
        {
            this.tcpListener = tcpListener;
            this.certificate = certificate;
        }

        public bool IsListening => tcpListener.IsListening;

        public IPEndPoint LocalEndPoint => tcpListener.LocalEndPoint;

        public async Task<ITcpClient> AcceptTcpClientAsync()
        {
            return new TcpClientSslDecorator(await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false), certificate);
        }

        public bool Pending() => tcpListener.Pending();

        public void Start() => tcpListener.Start();

        public void Start(int backlog) => tcpListener.Start(backlog);

        public void Stop() => tcpListener.Stop();

        public void Dispose() => tcpListener.Dispose();
    }
}