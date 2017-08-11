using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniCorn.Http
{
    public class HttpResponse : IHttpResponse
    {
        public IDictionary<string, string> Headers { get; private set; }

        public HttpResponseCode ResponseCode { get; set; } = HttpResponseCode.Ok;

        public string Status { get; set; } = "OK";

        public string Version { get; set; } = "HTTP/1.1";

        public string AcceptRanges
        {
            get => this["Accept-Ranges"];
            set => this["Accept-Ranges"] = value;
        }

        public string Age
        {
            get => this["Age"];
            set => this["Age"] = value;
        }

        public string CacheControl
        {
            get => this["Cache-Control"];
            set => this["Cache-Control"] = value;
        }

        public string Connection
        {
            get => this["Connection"];
            set => this["Connection"] = value;
        }

        public string Date
        {
            get => this["Date"];
            set => this["Date"] = value;
        }

        public string Location
        {
            get => this["Location"];
            set => this["Location"] = value;
        }

        public string ContentType
        {
            get => this["Content-Type"];
            set => this["Content-Type"] = value;
        }

        public string ContentLength
        {
            //get => this["Content-Length"];
            //set => this["Content-Length"] = value;
            get
            {
                return ContentStream.Length.ToString();
            }
        }

        public string Vary
        {
            get => this["Vary"];
            set => this["Vary"] = value;
        }

        public string Warning
        {
            get => this["Warning"];
            set => this["Warning"] = value;
        }

        public Stream OutputStream { get; private set; }

        public MemoryStream ContentStream { get; private set; }

        string content;
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;

                var bytes = Encoding.UTF8.GetBytes(content);

                ContentStream.Write(bytes, 0, bytes.Length);
            }
        }

        public HttpResponse(Stream outputStream, bool keepAlive = true)
        {
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Date", DateTime.UtcNow.ToString("R") },
                {"Content-Type",  "text/html; charset=utf-8"},
                {"Connection", keepAlive ? "keep-alive" : "close" },
                {"Content-Length", "0" }
            };

            //OutputStream = new StreamWriter(outputStream) { AutoFlush = false };

            OutputStream = outputStream;

            ContentStream = new MemoryStream();
        }

        public string this[string headerName]
        {
            get
            {
                string headerValue;
                return Headers.TryGetValue(headerName, out headerValue) ? headerValue : null;
            }
            set
            {
                Headers[headerName] = value;
            }
        }
    }
}