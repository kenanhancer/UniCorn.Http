using System.Threading;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public class DefaultHttpResponseProvider : IHttpResponseProvider
    {
        private readonly CancellationToken cancellationToken;

        public DefaultHttpResponseProvider(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public Task<IHttpResponse> ProvideAsync(IHttpRequest httpRequest)
        {
            IHttpResponse httpResponse = new HttpResponse(httpRequest.Stream) { Version = httpRequest.Version };

            return Task.FromResult(httpResponse);
        }
    }
}