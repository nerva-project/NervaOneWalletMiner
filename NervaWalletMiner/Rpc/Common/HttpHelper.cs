
using NervaWalletMiner.Helpers;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Common
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
                    client.Timeout = TimeSpan.FromSeconds(30);

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
                Logger.LogException("HTTP.GPFS", ex);
            }
            
            return response;
        }

        public static string GetServiceUrl(RpcSettings rpc)
        {
            if (string.IsNullOrEmpty(rpc.HTProtocol) || string.IsNullOrEmpty(rpc.Host) || rpc.Port < 1)
            {
                Logger.LogError("HTTP.GSU", "Rpc missing. Protocol: " + rpc.HTProtocol + ", Host: " + rpc.Host + ", Port: " + rpc.Port);
                return string.Empty;
            }
            else
            {
                return rpc.HTProtocol + "://" + rpc.Host + ":" + rpc.Port + "/json_rpc";
            }
        }
    }
}