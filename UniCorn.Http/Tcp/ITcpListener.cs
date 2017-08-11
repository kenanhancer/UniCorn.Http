using System;
using System.Net;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public interface ITcpListener : IDisposable
    {
        bool IsListening { get; }

        IPEndPoint LocalEndPoint { get; }

        Task<ITcpClient> AcceptTcpClientAsync();

        bool Pending();

        void Start();

        void Start(int backlog);

        void Stop();
    }
}