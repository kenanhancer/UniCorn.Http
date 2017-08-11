using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace UniCorn.Http
{
    internal sealed class HttpRequest : IHttpRequest
    {
        /// <summary>
        /// CONNECT, DELETE, GET, HEAD, OPTIONS, PATCH, POST, PUT
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// HTTP/1.0
        /// HTTP/1.1
        /// </summary>
        public string Version { get; set; }

        public string Uri { get; private set; }

        public IPEndPoint LocalEndpoint { get; private set; }

        public IPEndPoint RemoteEndpoint { get; private set; }

        public byte[] InputRaw { get; private set; }

        public Stream Stream { get; private set; }

        public IReadOnlyDictionary<string, string> Headers { get; private set; }

        public ITcpClient TcpClient { get; private set; }

        #region Headers

        public string Accept => this["Accept"];
        public string AcceptCharset => this["Accept-Charset"];
        public string AcceptEncoding => this["Accept-Encoding"];
        public string AcceptLanguage => this["Accept-Language"];
        public string Authorization => this["Authorization"];
        public string CacheControl => this["Cache-Control"];
        public string Connection => this["Connection"];
        public string Date => this["Date"];
        public string Expect => this["Expect"];
        public string From => this["From"];
        public string Host => this["Host"];
        public string IfMatch => this["If-Match"];
        public string IfModifiedSince => this["If-Modified-Since"];
        public string IfNoneMatch => this["If-None-Match"];
        public string IfRange => this["If-Range"];
        public string IfUnmodifiedSince => this["If-Unmodified-Since"];
        public string UserAgent => this["User-Agent"];
        public string Via => this["Via"];
        public string Warning => this["Warning"];

        #endregion Headers

        #region Content.Headers

        public string Allow => this["Allow"];
        public string ContentDisposition => this["Content-Disposition"];
        public string ContentEncoding => this["Content-Encoding"];
        public string ContentLanguage => this["Content-Language"];
        public string ContentLength => this["Content-Length"];
        public string ContentLocation => this["Content-Location"];
        public string ContentMD5 => this["Content-MD5"];
        public string ContentRange => this["Content-Range"];
        public string ContentType => this["Content-Type"];
        public string Expires => this["Expires"];
        public string LastModified => this["Last-Modified"];

        #endregion Content.Headers

        public HttpRequest(IDictionary<string, string> headers, string method, string version, string uri, byte[] inputRaw, ITcpClient tcpClient)
        {
            Headers = new ReadOnlyDictionary<string, string>(headers);
            Method = method;
            Version = version;
            Uri = uri;
            InputRaw = inputRaw;
            TcpClient = tcpClient;

            LocalEndpoint = tcpClient.LocalEndPoint;
            RemoteEndpoint = tcpClient.RemoteEndPoint;
            Stream = tcpClient.Stream;
        }

        public string this[string headerName]
        {
            get
            {
                string headerValue;
                return Headers.TryGetValue(headerName, out headerValue) ? headerValue : null;
            }
        }
    }
}