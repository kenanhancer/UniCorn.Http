using System;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public static class HttpServerExtensions
    {
        public static void Use(this HttpServer httpServer, Func<IHttpContext, Func<Task>, Task> handlerMethod)
        {
            httpServer.Use(new AnonymousHttpRequestHandler(handlerMethod));
        }
    }

    public class AnonymousHttpRequestHandler : IHttpRequestHandler
    {
        private readonly Func<IHttpContext, Func<Task>, Task> _handlerMethod;

        public AnonymousHttpRequestHandler(Func<IHttpContext, Func<Task>, Task> handlerMethod)
        {
            _handlerMethod = handlerMethod;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            return _handlerMethod(context, next);
        }
    }

    public class AnonymousHttpRequestHandlerAppBuilder
    {
        private Func<IHttpContext, Task> _nextHandler;
        private IHttpRequestHandler _requestHandler;

        public AnonymousHttpRequestHandlerAppBuilder(Func<IHttpContext, Task> nextAppHandler, IHttpRequestHandler requestHandler)
        {
            _nextHandler = nextAppHandler;
            _requestHandler = requestHandler;
        }

        public Task Invoke(IHttpContext context)
        {
            return _requestHandler.Handle(context, () => _nextHandler(context));
        }
    }
}