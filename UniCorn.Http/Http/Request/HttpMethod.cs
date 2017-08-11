namespace UniCorn.Http
{
    public static class HttpMethod
    {
        /// <summary>
        /// Requests a representation of the specified resource.
        /// </summary>
        public const string GET = "GET";

        /// <summary>
        /// Asks for the response identical to the one that would correspond to a GET request, but without the response body
        /// </summary>
        public const string HEAD = "HEAD";

        /// <summary>
        /// Requests that the server accept the entity enclosed in the request as a new subordinate of the web resource identified by the URI
        /// </summary>
        public const string POST = "POST";

        /// <summary>
        /// Requests that the enclosed entity be stored under the supplied URI
        /// </summary>
        public const string PUT = "PUT";

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        public const string DELETE = "DELETE";

        /// <summary>
        /// Echoes back the received request so that a client can see what (if any) changes or additions have been made by intermediate servers
        /// </summary>
        public const string TRACE = "TRACE";

        /// <summary>
        /// Returns the HTTP methods that the server supports for the specified URL
        /// </summary>
        public const string OPTIONS = "OPTIONS";

        /// <summary>
        /// Converts the request connection to a transparent TCP/IP tunnel
        /// </summary>
        public const string CONNECT = "CONNECT";

        /// <summary>
        /// Is used to apply partial modifications to a resource
        /// </summary>
        public const string PATCH = "PATCH";
    }
}