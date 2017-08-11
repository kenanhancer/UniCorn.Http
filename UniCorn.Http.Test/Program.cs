using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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

                        await next();
                    });

                    httpServer.RequestReceived += async (sender, e) =>
                    {
                        var request = e.Request;
                        var response = e.Response;

                        if (e.Request.InputRaw.Length > 0)
                        {
                            await response.ContentStream.WriteAsync(e.Request.InputRaw, 0, e.Request.InputRaw.Length);
                        }
                        else
                        {
                            string message = "Hello World!";

                            response.Content = message;
                        }

                        await response.ContentStream.FlushAsync();
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
                            await StreamUtility.FileToStream(path, context.Response.ContentStream);
                        }

                        //throw new HttpException(HttpResponseCode.NotFound, "The resource you've looked for is not found");
                    });

                    httpServer.Use(async (context, next) =>
                    {
                        context.Response.ResponseCode = HttpResponseCode.NotFound;
                        context.Response.Content = "There are not found.";

                        await Task.CompletedTask;
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

            Console.WriteLine(result1);

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

                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, content);
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
    }
}