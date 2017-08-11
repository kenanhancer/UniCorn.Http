# UniCorn.Http

UniCorn.Http is a simple, flexible and fast web server application. If you don't want to lose time with complex configurations of web servers like IIS or Tomcat, it can be useful to serve static files and controller services practically. It can be easily embedded to service simple requests.

![1](https://cloud.githubusercontent.com/assets/1851856/24648484/6d9c1148-192c-11e7-9933-215339d72e08.PNG)

## Http Server Console-base Application

It is very easy to use. Notice that to serve static files, source directory should be set. In addition, it can serve GET and POST operations by defining routes. I developed a console-based application for simplicity as shown in below.

<pre>
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniCorn.Core;

namespace UniCorn.Http.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test1();

            Test2();
        }

        static void Test1()
        {
            ManualResetEventSlim listenerWait = new ManualResetEventSlim(false);
            ManualResetEventSlim requestWait = new ManualResetEventSlim(false);

            Task.Run(() =>
            {
                using (HttpServer httpServer = new HttpServer())
                {
                    X509Certificate serverCertificate = new X509Certificate2("kenancert1.pfx", "1");

                    httpServer.Use(new TcpListenerAdapter(new TcpListenerEx(IPAddress.Loopback, 8080)));

                    httpServer.Use(new TcpListenerSslDecorator(new TcpListenerAdapter(new TcpListenerEx(IPAddress.Loopback, 443)), serverCertificate));

                    httpServer.Use(new ExceptionHandler());

                    httpServer.Use(new TimingHandler());

                    httpServer.Use(new MyHandler());

                    httpServer.Use(new FileHandler());

                    httpServer.Use(new ErrorHandler());

                    httpServer.Use(async (context, next) =>
                    {
                        Console.WriteLine("Go Request!");

                        await next().ConfigureAwait(false);
                    });

                    httpServer.RequestReceived += async (sender, e) =>
                    {
                        var request = e.Request;
                        var response = e.Response;

                        if (e.Request.InputRaw.Length > 0)
                        {
                            await response.ContentStream.WriteAsync(e.Request.InputRaw, 0, e.Request.InputRaw.Length).ConfigureAwait(false);
                        }
                        else
                        {
                            string message = "Hello World!";

                            response.Content = message;
                        }

                        await response.ContentStream.FlushAsync().ConfigureAwait(false);
                    };

                    httpServer.Start();

                    Console.WriteLine("UniCorn.HTTP Server Running...");
                    Console.WriteLine("=============================");

                    requestWait.Set();

                    listenerWait.Wait();

                    httpServer.Stop();
                }
            });

            requestWait.Wait();

            string result1 = PostJsonData("http://127.0.0.1:8080", "{" + "'FirstName':'Kenan','LastName':'Hancer'" + "}\r\n\r\n").Result;

            Console.WriteLine(result1);

            Console.ReadKey();

            listenerWait.Set();
        }

        static void Test2()
        {
            ManualResetEventSlim listenerWait = new ManualResetEventSlim(false);
            ManualResetEventSlim requestWait = new ManualResetEventSlim(false);

            Task.Run(() =>
            {
                using (HttpServer httpServer = new HttpServer())
                {
                    X509Certificate serverCertificate = new X509Certificate2("kenancert1.pfx", "1");

                    httpServer.Use(new TcpListenerAdapter(new TcpListenerEx(IPAddress.Loopback, 8080)));

                    httpServer.Use(new TcpListenerSslDecorator(new TcpListenerAdapter(new TcpListenerEx(IPAddress.Loopback, 443)), serverCertificate));

                    httpServer.Use(async (context, next) =>
                    {
                        Console.WriteLine("Timing Handler");

                        Stopwatch sw = Stopwatch.StartNew();

                        await next().ConfigureAwait(false);

                        Console.WriteLine($"Request {context.Request.Uri} took {sw.Elapsed}");

                        Console.WriteLine();
                    });

                    httpServer.Use(async (context, next) =>
                    {
                        Console.WriteLine("Exception Handler");

                        try
                        {
                            await next().ConfigureAwait(false);
                        }
                        catch (HttpException ex)
                        {
                            context.Response.ResponseCode = ex.ResponseCode;
                            context.Response.Content = ex.Message;
                        }
                        catch (Exception ex)
                        {
                            context.Response.ResponseCode = HttpResponseCode.InternalServerError;
                            context.Response.Content = ex.Message;
                        }
                    });

                    httpServer.Use(async (context, next) =>
                    {
                        Console.WriteLine("File Handler");

                        string requestPath = context.Request.Uri.TrimStart('/');

                        string HttpRootDirectory = @"www\";

                        var httpRoot = Path.GetFullPath(HttpRootDirectory ?? ".");
                        var path = Path.GetFullPath(Path.Combine(httpRoot, requestPath));

                        if (!File.Exists(path))
                        {
                            await next().ConfigureAwait(false);
                        }
                        else
                        {
                            await StreamUtility.FileToStream(path, context.Response.ContentStream).ConfigureAwait(false);
                        }

                        //throw new HttpException(HttpResponseCode.NotFound, "The resource you've looked for is not found");
                    });

                    httpServer.Use(async (context, next) =>
                    {
                        Console.WriteLine("POST Handler");

                        if (context.Request.InputRaw.Length > 0)
                        {
                            context.Response.ResponseCode = HttpResponseCode.Ok;
                            await context.Response.ContentStream.WriteAsync(context.Request.InputRaw, 0, context.Request.InputRaw.Length).ConfigureAwait(false);
                        }
                        else
                        {
                            await next().ConfigureAwait(false);
                        }
                    });

                    httpServer.Use((context, next) =>
                    {
                        context.Response.ResponseCode = HttpResponseCode.NotFound;
                        context.Response.Content = "There are not found.";

                        return Task.CompletedTask;
                    });

                    httpServer.Start();

                    Console.WriteLine("UniCorn.HTTP Server Running...");
                    Console.WriteLine("=============================");

                    requestWait.Set();

                    listenerWait.Wait();

                    httpServer.Stop();
                }
            });

            requestWait.Wait();

            string result1 = PostJsonData("http://127.0.0.1:8080", "{" + "'FirstName':'Kenan','LastName':'Hancer'" + "}\r\n\r\n").Result;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Output:");
            Console.WriteLine(result1);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            string result2 = RequestPage("http://127.0.0.1:8080/iisstart.htm").Result;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Output:");
            Console.WriteLine(result2);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

            OpenBrowser("http://127.0.0.1:8080/iisstart.htm");

            Console.ReadKey();

            listenerWait.Set();
        }

        public static async Task<string> PostJsonData(string url, string json)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, content).ConfigureAwait(false);
                    {
                        string result = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static async Task<string> RequestPage(string url)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("text/html");

                    using (HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(url))
                    {
                        string result = await httpResponseMessage.Content.ReadAsStringAsync();

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
</pre>


<pre>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UniCorn.Http.Test
{
    public class MyHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            //var model = new ModelBinder(new ObjectActivator()).Get<MyModel>(context.Request.QueryString);

            await next().ConfigureAwait(false);
        }
    }

    public class TimingHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            Stopwatch stopWatch = Stopwatch.StartNew();

            await next().ConfigureAwait(false);

            Console.WriteLine($"request {context.Request.Uri} took {stopWatch.Elapsed}");
        }
    }

    public class ErrorHandler : IHttpRequestHandler
    {
        public Task Handle(IHttpContext context, System.Func<Task> next)
        {
            context.Response.ResponseCode = HttpResponseCode.NotFound;
            context.Response.Content = "These are not the droids you are looking for.";

            return Task.CompletedTask;
        }
    }

    public class ExceptionHandler : IHttpRequestHandler
    {
        public async Task Handle(IHttpContext context, Func<Task> next)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                context.Response.ResponseCode = ex.ResponseCode;

                context.Response.Content = "Error while handling your request. " + ex.Message;
            }
            catch (Exception ex)
            {
                context.Response.ResponseCode = HttpResponseCode.InternalServerError;
                context.Response.Content = "Error while handling your request. " + ex;
            }
        }
    }

    public class FileHandler : IHttpRequestHandler
    {
        public static string DefaultMimeType { get; set; }
        public static string HttpRootDirectory { get; set; }
        public static IDictionary<string, string> MimeTypes { get; private set; }

        static FileHandler()
        {
            DefaultMimeType = "text/plain";

            #region MimeTypes

            MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            { ".zip", "application/zip"}};

            #endregion MimeTypes
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path) ?? string.Empty;
            if (MimeTypes.ContainsKey(extension))
                return MimeTypes[extension];
            return DefaultMimeType;
        }
        public async Task Handle(IHttpContext context, System.Func<Task> next)
        {
            var requestPath = context.Request.Uri.TrimStart('/');

            var httpRoot = Path.GetFullPath(HttpRootDirectory ?? ".");
            var path = Path.GetFullPath(Path.Combine(httpRoot, requestPath));

            if (!File.Exists(path))
            {
                await next().ConfigureAwait(false);
            }
            else
            {
                await File.OpenRead(path).CopyToAsync(context.Response.ContentStream).ConfigureAwait(false);
            }
        }
    }
}
</pre>
