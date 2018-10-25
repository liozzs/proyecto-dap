using System.Collections.Generic;

namespace DAP.Mobile.Services
{
    public class ApiClientOption
    {
        public ApiClientRequestTypes RequestType { get; set; }

        /// <summary>
        /// Request Base Url. Use only for external request (No DataAccess request)
        /// </summary>
        public string BaseUrl { get; set; }

        public string Uri { get; set; }
        public object UriParameters { get; set; }

        /// <summary>
        /// Request content for POST & PUT
        /// </summary>
        public object RequestContent { get; set; }

        /// <summary>
        /// Timeout in milliseconds
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// By default Json
        /// </summary>
        public ApiClientRequestContentTypes ContentType { get; set; }

        public Dictionary<string, string> Headers { get; set; }
        public bool IsOptional { get; internal set; }
        public ApiClientServices Service { get; internal set; }
    }

    public enum ApiClientRequestTypes
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum ApiClientRequestContentTypes
    {
        Json,
        Form,
        Xml
    }

    public enum ApiClientServices
    {
        Api,
        Arduino
    }
}