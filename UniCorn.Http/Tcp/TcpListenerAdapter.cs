using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public class TcpListenerAdapter : ITcpListener
    {
        private TcpListenerEx tcpListener;

        public bool IsListening => tcpListener.Active;

        public IPEndPoint LocalEndPoint => (IPEndPoint)tcpListener.LocalEndpoint;

        public TcpListenerAdapter(TcpListenerEx tcpListener) => this.tcpListener = tcpListener;

        public virtual async Task<ITcpClient> AcceptTcpClientAsync() => new TcpClientAdapter(await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false));

        public virtual bool Pending() => tcpListener.Pending();

        public virtual void Start() => tcpListener.Start();

        public virtual void Start(int backlog) => tcpListener.Start(backlog);

        public virtual void Stop() => tcpListener.Stop();

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TcpListener tcpListener_ = tcpListener;
                    tcpListener = null;
                    if (tcpListener_ != null)
                    {
                        tcpListener_.Stop();
                        tcpListener_ = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}