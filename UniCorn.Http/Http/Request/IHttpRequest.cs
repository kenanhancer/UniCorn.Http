using System.Collections.Generic;
using System.IO;
using System.Net;

namespace UniCorn.Http
{
    public interface IHttpRequest
    {
        string Method { get; }

        string Version { get; }

        string Uri { get; }

        IPEndPoint LocalEndpoint { get; }

        IPEndPoint RemoteEndpoint { get; }

        byte[] InputRaw { get; }

        IReadOnlyDictionary<string, string> Headers { get; }

        Stream Stream { get; }

        ITcpClient TcpClient { get; }

        #region Headers

        string Accept { get; }
        string AcceptCharset { get; }
        string AcceptEncoding { get; }
        string AcceptLanguage { get; }
        string Authorization { get; }
        string CacheControl { get; }
        string Connection { get; }
        string Date { get; }
        string Expect { get; }
        string From { get; }
        string Host { get; }
        string IfMatch { get; }
        string IfModifiedSince { get; }
        string IfNoneMatch { get; }
        string IfRange { get; }
        string IfUnmodifiedSince { get; }
        string UserAgent { get; }
        string Via { get; }
        string Warning { get; }

        #endregion Headers

        #region Content.Headers

        string Allow { get; }
        string ContentDisposition { get; }
        string ContentEncoding { get; }
        string ContentLanguage { get; }
        string ContentLength { get; }
        string ContentLocation { get; }
        string ContentMD5 { get; }
        string ContentRange { get; }
        string ContentType { get; }
        string Expires { get; }
        string LastModified { get; }

        #endregion Content.Headers
    }
}