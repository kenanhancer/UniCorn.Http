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