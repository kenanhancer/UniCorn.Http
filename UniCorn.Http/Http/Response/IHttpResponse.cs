using System.Collections.Generic;
using System.IO;

namespace UniCorn.Http
{
    public interface IHttpResponse
    {
        IDictionary<string, string> Headers { get; }

        HttpResponseCode ResponseCode { get; set; }

        string Status { get; set; }

        Stream OutputStream { get; }

        MemoryStream ContentStream { get; }

        string Content { get; set; }

        string Version { get; set; }

        string AcceptRanges { get; set; }
        string Age { get; set; }
        string CacheControl { get; set; }
        string Connection { get; set; }
        string Date { get; set; }
        string Location { get; set; }
        string ContentType { get; set; }
        string ContentLength { get; }
        string Vary { get; set; }
        string Warning { get; set; }
    }
}