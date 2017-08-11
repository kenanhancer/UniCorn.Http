using System.Dynamic;

namespace UniCorn.Http
{
    public class HttpContext : IHttpContext
    {
        private IHttpRequest httpRequest;

        private IHttpResponse httpResponse;

        private readonly ExpandoObject state = new ExpandoObject();

        public IHttpRequest Request => httpRequest;

        public IHttpResponse Response => httpResponse;

        public dynamic State => state;

        public HttpContext(IHttpRequest httpRequest, IHttpResponse httpResponse)
        {
            this.httpRequest = httpRequest;
            this.httpResponse = httpResponse;
        }
    }
}