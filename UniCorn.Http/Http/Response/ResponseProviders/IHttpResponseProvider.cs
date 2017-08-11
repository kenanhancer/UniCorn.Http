using System.Threading.Tasks;

namespace UniCorn.Http
{
    public interface IHttpResponseProvider
    {
        Task<IHttpResponse> ProvideAsync(IHttpRequest httpRequest);
    }
}