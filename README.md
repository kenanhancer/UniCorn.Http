# UniCorn.Http

UniCorn.Http is a simple, flexible and fast web server application. If you don't want to lose time with complex configurations of web servers like IIS or Tomcat, it can be useful to serve static files and controller services practically. It can be easily embedded to service simple requests.

![1](https://cloud.githubusercontent.com/assets/1851856/24648484/6d9c1148-192c-11e7-9933-215339d72e08.PNG)

## Http Server Console-base Application

It is very easy to use. Notice that to serve static files, source directory should be set. In addition, it can serve GET and POST operations by defining routes. I developed a console-based application for simplicity as shown in below.

<pre>
using HttpServerLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServerApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(params string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;

                cts.Cancel();
            };

            string sourceDir = @"http\";

            TestController controller = new TestController();

            var getRoutes = new Dictionary<string, RouteInfo>();
            getRoutes.Add("/hello", new RouteInfo { ActionName = "Hello", ControllerType = typeof(Program) });
            getRoutes.Add("/account", new RouteInfo { ActionName = "Account", ControllerInstance = controller });

            var postRoutes = new Dictionary<string, RouteInfo>();
            postRoutes.Add("/", new RouteInfo { ActionName = "Post", ControllerType = typeof(Program) });

            using (SimpleHttpServer httpServer = new SimpleHttpServer("127.0.0.1", "8080", (8 * 1024), 3000, sourceDir, cts.Token) { GetRoutes = getRoutes, PostRoutes = postRoutes })
            {
                Console.WriteLine("Simple HTTP Server Running...");
                Console.WriteLine("=============================");


                Helper.OpenBrowser($"http://{httpServer.Ip}:{httpServer.PortNumber}");
                Helper.OpenBrowser($"http://{httpServer.Ip}:{httpServer.PortNumber}/hello");
                Helper.OpenBrowser($"http://{httpServer.Ip}:{httpServer.PortNumber}/account/Kenan/33");


                <b>await httpServer.RequestReceivedAsync(async httpRequest =>
                {</b>
                    await Console.Out.WriteLineAsync(httpRequest.ToString());
                <b>});</b>
            }
        }

        public static object <b>Hello</b>(HttpRequestEntity request)
        {
            request.Response.Content = "Hello World!";

            return null;
        }

        public static void <b>Account</b>(HttpRequestEntity request, string name, int age)
        {
            request.Response.Content = $"Welcome {name}! Your Age is {age}";
        }

        public static object <b>Post</b>(HttpRequestEntity request)
        {
            request.Response.Content = "OK";

            return null;
        }
    }
}
</pre>

Let's look at controller class. It is very simple.

<pre>
using HttpServerLib;

namespace HttpServerApp
{
    public class TestController
    {
        public object Hello(HttpRequestEntity request)
        {
            request.Response.Content = "Hello World!";

            return null;
        }

        public void Account(HttpRequestEntity request, string name, int age)
        {
            request.Response.Content = $"Welcome {name}! Your Age is {age}";
        }

        public object Post(HttpRequestEntity request)
        {
            request.Response.Content = "OK";

            return null;
        }
    }
}
</pre>
