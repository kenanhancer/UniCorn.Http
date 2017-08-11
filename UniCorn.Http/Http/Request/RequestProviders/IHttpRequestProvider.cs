using System.Threading.Tasks;

namespace UniCorn.Http
{
    public interface IHttpRequestProvider
    {
        Task<IHttpRequest> ProvideAsync(ITcpClient acceptedTcpClient);
    }
}