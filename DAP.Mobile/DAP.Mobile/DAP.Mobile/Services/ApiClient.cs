using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DAP.Mobile.Services
{
    public class ApiClient : IApiClient
    {
        private readonly int? timeout;

        public ApiClient()
        {
            timeout = 60000;
        }

        public async Task<string> InvokeDataServiceStringAsync(ApiClientOption option)
        {
            using (var client = new HttpClient())
            {
                if (option.Headers != null)
                {
                    foreach (var item in option.Headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                client.BaseAddress = new Uri(option.BaseUrl);
                //Set timeout if is configured
                if (option.Timeout.HasValue) client.Timeout = new TimeSpan(TimeSpan.TicksPerMillisecond * option.Timeout.Value);
                else if (timeout.HasValue) client.Timeout = new TimeSpan(TimeSpan.TicksPerMillisecond * timeout.Value);

                var uri = GenerateUri(option);
                Task<HttpResponseMessage> response = null;
                if (option.RequestType == ApiClientRequestTypes.Get)
                {
                    response = client.GetAsync(uri);
                    if (option.RequestContent != null) throw new Exception("The request type selected doesn't allow content");
                }
                else if (option.RequestType == ApiClientRequestTypes.Delete)
                {
                    response = client.DeleteAsync(uri);
                    if (option.RequestContent != null) throw new Exception("The request type selected doesn't allow content");
                }
                else
                {
                    HttpContent httpContent = null;
                    if (option.ContentType == ApiClientRequestContentTypes.Xml)
                    {
                        if (option.RequestContent is XDocument)
                        {
                            var requestContent = option.RequestContent as XDocument;
                            httpContent = new StringContent(requestContent.ToString(), Encoding.UTF8, "application/xml");
                        }
                        else
                        {
                            var requestContent = option.RequestContent as string;
                            httpContent = new StringContent(requestContent, Encoding.UTF8, "text/xml");
                        }
                    }
                    if (option.ContentType == ApiClientRequestContentTypes.Form)
                    {
                        var requestObject = JsonConvert.SerializeObject(option.RequestContent);
                        var listObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestObject);
                        httpContent = new FormUrlEncodedContent(listObject.ToList());
                    }
                    if (option.ContentType == ApiClientRequestContentTypes.Json)
                    {
                        var requestObject = JsonConvert.SerializeObject(option.RequestContent);
                        httpContent = new StringContent(requestObject, Encoding.UTF8, "application/json");
                    }
                    if (httpContent == null) throw new InvalidOperationException($"{option.ContentType.ToString()} is not valid");
                    if (option.RequestType == ApiClientRequestTypes.Post) response = client.PostAsync(uri, httpContent);
                    if (option.RequestType == ApiClientRequestTypes.Put) response = client.PutAsync(uri, httpContent);
                }
                try
                {
                    if (response != null) await response;
                    if (!response.Result.IsSuccessStatusCode)
                    {
                        Exception exception = null;

                        if (response.Result.StatusCode != HttpStatusCode.InternalServerError &&
                            response.Result.StatusCode != HttpStatusCode.BadRequest)
                        {
                            exception = new HttpRequestException(response.Result.ReasonPhrase);
                        }
                        else
                        {
                            exception = await ReadResponse(response, exception, response.Result.StatusCode);
                        }
                        exception.Data["Server"] = client.BaseAddress;
                        exception.Data["Url"] = uri;
                        exception.Data["StatusCode"] = response.Result.StatusCode;
                        //log.Error("External Server Error.", exception);
                        if (option.IsOptional) return null;
                        throw exception;
                    }
                    var result = await response.Result.Content.ReadAsStringAsync();
                    return result;
                }
                catch (AggregateException ex)
                {
                    var newException = new Exception(ex.InnerException.Message);
                    newException.Data["Server"] = client.BaseAddress;
                    newException.Data["Url"] = uri;
                    //log.Error("Error on external server response", newException);
                    if (option.IsOptional) return null;
                    throw newException;
                }
                catch (Exception ex)
                {
                    var newException = new Exception(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    newException.Data["Server"] = client.BaseAddress;
                    newException.Data["Url"] = uri;
                    //log.Error("Unkown error trying connect to external server", newException);
                    if (option.IsOptional) return null;
                    throw newException;
                }
            }
        }

        public async Task<TResponse> InvokeDataServiceAsync<TResponse>(ApiClientOption option) where TResponse : class
        {
            var result = await InvokeDataServiceStringAsync(option);
            if (string.IsNullOrEmpty(result))
            {
                if (option.IsOptional) return null;
                else if (option.RequestType == ApiClientRequestTypes.Get) throw new Exception("verb Get must be set an empty array when not found results");
            }
            if (option.ContentType == ApiClientRequestContentTypes.Json) return JsonConvert.DeserializeObject<TResponse>(result);
            if (option.ContentType == ApiClientRequestContentTypes.Xml || option.ContentType == ApiClientRequestContentTypes.Form)
            {
                var xmlSerializer = new XmlSerializer(typeof(TResponse));
                var xml = new XDocument();
                using (TextReader reader = new StringReader(result))
                {
                    return (TResponse)xmlSerializer.Deserialize(reader);
                }
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        public async Task InvokeDataServiceAsync(ApiClientOption option)
        {
            await InvokeDataServiceStringAsync(option);
        }

        private static string GenerateUri(ApiClientOption option)
        {
            if (option.UriParameters == null) return string.IsNullOrEmpty(option.BaseUrl) ? option.Uri : option.BaseUrl;

            var properties = option.UriParameters.GetType().GetProperties();

            if (properties.Length == 0) return option.Uri;

            var objectParameters = new object[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var objectValue = properties[i].GetValue(option.UriParameters);
                if (objectValue is DateTime date)
                {
                    objectParameters[i] = date.ToString("o", CultureInfo.InvariantCulture);
                }
                else if (objectValue is string value)
                {
                    objectParameters[i] = WebUtility.HtmlEncode(value);
                }
                else
                {
                    objectParameters[i] = objectValue;
                }
            }
            return string.Format(string.IsNullOrEmpty(option.BaseUrl) ? option.Uri : option.BaseUrl, objectParameters);
        }
        
        private static async Task<Exception> ReadResponse(Task<HttpResponseMessage> response, Exception exception, HttpStatusCode statusCode)
        {
            var responseMessage = await response.Result.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(responseMessage))
            {
                if (response.Result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    if ((responseMessage.StartsWith("{") || responseMessage.StartsWith("[")) && (responseMessage.EndsWith("}") || responseMessage.EndsWith("]")))
                    {
                        var definition = new { Error = "", Data = new Dictionary<string, object>(), StackTrace = "" };
                        try
                        {
                            var jsonResult = JsonConvert.DeserializeAnonymousType(responseMessage, definition);
                            exception = new Exception(jsonResult.Error);
                            exception.Data["Data"] = jsonResult.Data;
                            exception.Data["StackTrace"] = jsonResult.StackTrace;
                        }
                        catch
                        {
                            exception = new Exception(responseMessage);
                        }
                    }
                    else
                    {
                        exception = new Exception(responseMessage);
                    }
                }
                else if (response.Result.StatusCode == HttpStatusCode.BadRequest)
                {
                    var definition = new { ValidationMessage = string.Empty };
                    var jsonResult = JsonConvert.DeserializeAnonymousType(responseMessage, definition);
                    exception = new Exception(jsonResult.ValidationMessage);
                }
                else
                {
                    exception = new Exception(response.Result.ReasonPhrase);
                }

            }
            else
            {
                exception = new Exception(response.Result.ReasonPhrase);
            }
            return exception;
        }

    }
}