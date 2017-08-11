using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniCorn.Http
{
    public class DefaultHttpRequestProvider : IHttpRequestProvider
    {
        private readonly CancellationToken cancellationToken;

        public DefaultHttpRequestProvider(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public async Task<IHttpRequest> ProvideAsync(ITcpClient acceptedTcpClient)
        {
            StreamReader streamReader = new StreamReader(acceptedTcpClient.Stream);

            IHttpRequest httpRequest = null;

            try
            {
                string line = await streamReader.ReadLineAsync().ContinueWith(f => f.Result, cancellationToken).ConfigureAwait(false);

                if (String.IsNullOrEmpty(line))
                    return null;

                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string httpMethod = parts[0];

                string httpVersion = parts[2];

                string uri = parts[1];

                var httpHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string headerName;
                string headerValue;

                while ((line = await streamReader.ReadLineAsync().ContinueWith(f => f.Result, cancellationToken).ConfigureAwait(false)) != "")
                {
                    parts = line.Split(new[] { ':' }, 2);

                    if (parts.Length > 1)
                    {
                        headerName = parts[0];

                        headerValue = parts[1].Trim();

                        httpHeaders[headerName] = headerValue;
                    }
                }

                byte[] rawBuffer = null;

                if (httpMethod == HttpMethod.POST || httpMethod == HttpMethod.PUT || httpMethod == HttpMethod.PATCH)
                {
                    int contentLength = 0;

                    string contentLengthVal;

                    if (httpHeaders.TryGetValue("Content-Length", out contentLengthVal))
                        int.TryParse(contentLengthVal, out contentLength);

                    char[] rawEncoded = new char[contentLength];

                    int readBytes = await streamReader.ReadAsync(rawEncoded, 0, contentLength).ConfigureAwait(false);

                    rawBuffer = Encoding.UTF8.GetBytes(rawEncoded, 0, readBytes);
                }

                rawBuffer = rawBuffer ?? new byte[0];

                httpRequest = new HttpRequest(httpHeaders, httpMethod, httpVersion, uri, rawBuffer, acceptedTcpClient);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
            }

            return httpRequest;
        }
    }
}