using System;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public interface IHttpRequestHandler
    {
        Task Handle(IHttpContext context, Func<Task> next);
    }
}