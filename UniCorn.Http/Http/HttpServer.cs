using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using UniCorn.AppBuilder;

namespace UniCorn.Http
{
    public sealed class HttpServer : IDisposable
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpServer));

        private const int DefaultMaxRequests = Int32.MaxValue;

        private static readonly int DefaultMaxAccepts = 5 * Environment.ProcessorCount;

        private PumpLimits pumpLimits;

        private int _currentOutstandingAccepts;

        private int _currentOutstandingRequests;

        private IList<ITcpListener> tcpListeners = new List<ITcpListener>();

        private IList<IHttpRequestHandler> httpRequestHandlers = new List<IHttpRequestHandler>();

        public int MaxAcceptLimit => pumpLimits.MaxOutstandingAccepts;

        public int MaxRequestLimit => pumpLimits.MaxOutstandingRequests;

        public CancellationToken cancellationToken { get; private set; }

        private readonly IHttpRequestProvider httpRequestProvider;

        private readonly IHttpResponseProvider httpResponseProvider;

        public event EventHandler<IHttpContext> RequestReceived;

        private Func<IHttpContext, Task> aggregatedHandler;

        //private IAppBuilder appBuilder;

        private bool CanAcceptMoreRequests
        {
            get
            {
                PumpLimits limits = pumpLimits;
                return (_currentOutstandingAccepts < limits.MaxOutstandingAccepts
                    && _currentOutstandingRequests < limits.MaxOutstandingRequests - _currentOutstandingAccepts);
            }
        }

        public HttpServer() :
            this(new DefaultHttpRequestProvider(CancellationToken.None), new DefaultHttpResponseProvider(CancellationToken.None), CancellationToken.None)
        {
        }

        public HttpServer(IHttpRequestProvider httpRequestProvider, IHttpResponseProvider httpResponseProvider, CancellationToken cancellationToken)
        {
            this.httpRequestProvider = httpRequestProvider;
            this.httpResponseProvider = httpResponseProvider;
            this.cancellationToken = cancellationToken;

            //appBuilder = new AppBuilder.AppBuilder();

            //appBuilder.Properties["DefaultApp"] = new Func<IHttpContext, Task>((IHttpContext context) => Task.CompletedTask);

            SetRequestProcessingLimits(DefaultMaxAccepts, DefaultMaxRequests);
        }

        /// <summary>
        /// These are merged as one call because they should be swapped out atomically.
        /// This controls how many requests the server attempts to process concurrently.
        /// </summary>
        /// <param name="maxAccepts">The maximum number of pending request receives.</param>
        /// <param name="maxRequests">The maximum number of active requests being processed.</param>
        public void SetRequestProcessingLimits(int maxAccepts, int maxRequests)
        {
            pumpLimits = new PumpLimits(maxAccepts, maxRequests);

            foreach (ITcpListener tcpListener in tcpListeners)
            {
                // Kick the pump in case we went from zero to non-zero limits.
                OffloadStartNextRequest(tcpListener);
            }
        }

        public void Use(ITcpListener tcpListener)
        {
            tcpListeners.Add(tcpListener);
        }

        public void Use(IHttpRequestHandler httpRequestHandler)
        {
            httpRequestHandlers.Add(httpRequestHandler);

            //appBuilder.Use<AnonymousHttpRequestHandlerAppBuilder>(httpRequestHandler);

            //appBuilder.UseFunc<Func<IHttpContext, Task>>(next => context => httpRequestHandler.Handle(context, () => next(context)));
        }

        public void Start()
        {
            aggregatedHandler = httpRequestHandlers.Aggregate<IHttpRequestHandler, IHttpContext>((hrh, nextHrh) => context => hrh.Handle(context, () => nextHrh != null ? nextHrh(context) : Task.CompletedTask));

            //aggregatedHandler = appBuilder.Build<Func<IHttpContext, Task>>();

            foreach (ITcpListener tcpListener in tcpListeners)
            {
                tcpListener.Start();

                OffloadStartNextRequest(tcpListener);
            }
        }

        public void Stop()
        {
            RequestReceived = null;
            foreach (ITcpListener tcpListener in tcpListeners)
            {
                try
                {
                    tcpListener.Stop();
                }
                catch (Exception)
                {
                }
            }
        }

        private void OffloadStartNextRequest(ITcpListener tcpListener)
        {
            if (tcpListener.IsListening && CanAcceptMoreRequests)
                Task.Factory.StartNew(() => ProcessRequestsAsync(tcpListener), cancellationToken);
        }

        private async Task ProcessRequestsAsync(ITcpListener tcpListener)
        {
            while (tcpListener.IsListening && CanAcceptMoreRequests)
            {
                Interlocked.Increment(ref _currentOutstandingAccepts);

                IHttpContext httpContext;

                try
                {
                    httpContext = await GetContextAsync(tcpListener).ConfigureAwait(false);
                }
                catch (ApplicationException ae)
                {
                    // These come from the thread pool if HttpListener tries to call BindHandle after the listener has been disposed.
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    //LogHelper.LogException(_logger, "Accept", ae);
                    return;
                }
                catch (HttpListenerException hle)
                {
                    // These happen if HttpListener has been disposed
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    //LogHelper.LogException(_logger, "Accept", hle);
                    return;
                }
                catch (ObjectDisposedException ode)
                {
                    // These happen if HttpListener has been disposed
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    //LogHelper.LogException(_logger, "Accept", ode);
                    return;
                }
                catch (Exception ex)
                {
                    // Some other unknown error. Log it and try to keep going.
                    Interlocked.Decrement(ref _currentOutstandingAccepts);
                    //LogHelper.LogException(_logger, "Accept", ex);
                    continue;
                }

                Interlocked.Decrement(ref _currentOutstandingAccepts);
                Interlocked.Increment(ref _currentOutstandingRequests);

                if (httpContext == null)
                    continue;

                await aggregatedHandler(httpContext).ConfigureAwait(false);

                OffloadStartNextRequest(tcpListener);

                await ProcessRequestAsync(httpContext);
            }
        }

        private async Task ProcessRequestAsync(IHttpContext httpContext)
        {
            try
            {
                if (RequestReceived != null)
                    RequestReceived(this, httpContext);

                await WriteResponseAsync(httpContext.Response);

                Interlocked.Decrement(ref _currentOutstandingRequests);
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref _currentOutstandingRequests);
                //LogHelper.LogException(_logger, "Exception during request processing.", ex);
            }
            finally
            {
                httpContext.Response.OutputStream.Close();
                httpContext.Request.TcpClient.AcceptedTcpClient.Close();
            }
        }

        public async Task<IHttpContext> GetContextAsync(ITcpListener tcpListener)
        {
            IHttpContext httpContext = null;

            try
            {
                ITcpClient acceptedTcpClient = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);

                IHttpRequest httpRequest = await httpRequestProvider.ProvideAsync(acceptedTcpClient).ConfigureAwait(false);

                if (httpRequest == null)
                    return httpContext;

                IHttpResponse httpResponse = await httpResponseProvider.ProvideAsync(httpRequest).ConfigureAwait(false);

                httpContext = new HttpContext(httpRequest, httpResponse);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }

            return httpContext;
        }

        private async Task WriteResponseAsync(IHttpResponse httpResponse)
        {
            byte[] headerBuffer = BuildHttpResponse(httpResponse);

            await httpResponse.OutputStream.WriteAsync(headerBuffer, 0, headerBuffer.Length);

            httpResponse.ContentStream.Position = 0;
            await httpResponse.ContentStream.CopyToAsync(httpResponse.OutputStream).ConfigureAwait(false);

            //int bytesRead;
            //byte[] responseBuffer = new byte[8192];

            //while ((bytesRead = await httpResponse.ContentStream.ReadAsync(responseBuffer, 0, responseBuffer.Length).ConfigureAwait(false)) != 0)
            //{
            //    await httpResponse.OutputStream.WriteAsync(responseBuffer, 0, bytesRead).ConfigureAwait(false);
            //}
        }

        private static byte[] BuildHttpResponse(IHttpResponse httpResponse)
        {
            StringBuilder sb = new StringBuilder();

            long contentLength;
            long.TryParse(httpResponse.ContentLength, out contentLength);

            string content = httpResponse.Content;

            sb.AppendLine($"{httpResponse.Version} {(long)httpResponse.ResponseCode} {httpResponse.ResponseCode}");
            sb.AppendLine($"Content-Type: {httpResponse.ContentType}");
            sb.AppendLine($"Content-Length: {httpResponse.ContentLength}");
            sb.AppendLine("Accept-Ranges: bytes");
            //foreach (var header in httpResponse.Headers)
            //{
            //    sb.AppendLine($"{header.Key}: {header.Value}");
            //}

            sb.AppendLine();

            if (!String.IsNullOrEmpty(content))
                sb.AppendLine(content);

            byte[] responseBuffer = Encoding.UTF8.GetBytes(sb.ToString());

            return responseBuffer;
        }

        #region IDisposable Support

        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RequestReceived = null;
                    IList<ITcpListener> tcpListeners_ = tcpListeners;
                    tcpListeners = null;
                    if (tcpListeners_ != null)
                    {
                        foreach (ITcpListener tcpListener in tcpListeners_)
                        {
                            tcpListener.Dispose();
                        }
                        tcpListeners_ = null;
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

    public static class AggregateExtensions
    {
        public static Func<R, Task> Aggregate<T, R>(this IList<T> handlers, Func<T, Func<R, Task>, Func<R, Task>> operation)
        {
            return handlers.Aggregate<T, R>(operation, 0);
        }

        private static Func<R, Task> Aggregate<T, R>(this IList<T> handlers, Func<T, Func<R, Task>, Func<R, Task>> operation, int index)
        {
            if (index == handlers.Count)
            {
                return null;
            }

            var currentHandler = handlers[index];
            var nextHandler = handlers.Aggregate<T, R>(operation, index + 1);

            return operation(currentHandler, nextHandler);
        }
    }
}