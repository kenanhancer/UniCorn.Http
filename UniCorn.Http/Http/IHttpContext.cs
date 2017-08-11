namespace UniCorn.Http
{
    public interface IHttpContext
    {
        IHttpRequest Request { get; }

        IHttpResponse Response { get; }
    }
}