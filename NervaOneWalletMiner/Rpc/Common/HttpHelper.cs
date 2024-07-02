using NervaOneWalletMiner.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class HttpHelper
    {
        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(50);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                    request.Content = new StringContent(postContent);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("HTTP.GPFS", "Calling POST: " + serviceUrl);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("HTTP.GPFS", "Call returned: " + serviceUrl);                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.GPFS", "Exception message: " + ex.Message);
            }
            
            return response;
        }

        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent, string userName, string password)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(50);

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                    var authentication = userName + ":" + password;
                    var base64EncodedAuthentication = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentication));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthentication);

                    request.Content = new StringContent(postContent);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("HTTP.GPSA", "Calling POST: " + serviceUrl);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("HTTP.GPSA", "Call returned: " + serviceUrl);                    
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.GPSA", "Exception message: " + ex.Message);
            }

            return response;
        }

        public static ServiceError GetHttpError(string source, HttpResponseMessage httpResponse)
        {
            ServiceError httpError = new();

            try
            {
                httpError.IsError = true;
                httpError.Code = httpResponse.StatusCode.ToString();
                httpError.Message = httpResponse.ReasonPhrase;
                httpError.Content = httpResponse.Content.ReadAsStringAsync().Result;

                Logger.LogError("HTTP.GHE", source + " - response failed. Code: " + httpResponse.StatusCode + ", Phrase: " + httpResponse.ReasonPhrase);
            }
            catch (Exception ex)
            {
                Logger.LogException("HTTP.GHER", ex);
            }

            return httpError;
        }

        public static string GetServiceUrl(RpcBase rpc, string path)
        {
            string serviceUrl = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(rpc.HTProtocol) || string.IsNullOrEmpty(rpc.Host) || rpc.Port < 1)
                {
                    Logger.LogError("HTTP.GSUL", "Rpc missing. Protocol: " + rpc.HTProtocol + ", Host: " + rpc.Host + ", Port: " + rpc.Port);
                }
                else
                {
                    serviceUrl =  rpc.HTProtocol + "://" + rpc.Host + ":" + rpc.Port + "/" + path;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("HTTP.GSUL", ex);
            }

            return serviceUrl;
        }
    }
}