using NervaOneWalletMiner.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class HttpHelper
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(50);

        private static readonly HttpClient _client = new HttpClient()
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        // Cached Digest auth params - reused preemptively to avoid double round-trip
        private static string? _cachedRealm;
        private static string? _cachedNonce;
        private static string? _cachedQop;
        private static string? _cachedOpaque;
        private static int _cachedNc;
        private static string _cachedKey = string.Empty;

        #region No Auth
        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent)
        {
            return await GetPostFromService(serviceUrl, postContent, _defaultTimeout);
        }

        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent, TimeSpan timeout)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(timeout);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
                request.Content = new StringContent(postContent);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //Logger.LogDebug("HTTP.GPFS", "Calling POST: " + serviceUrl);

                response = await _client.SendAsync(request, cts.Token);

                //Logger.LogDebug("HTTP.GPFS", "Call returned: " + serviceUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.GPFS", "Exception type: " + ex.GetType().Name + " | Message: " + ex.Message + (ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : string.Empty));
            }

            return response;
        }
        #endregion // No Auth

        #region Basic Auth
        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent, string userName, string password)
        {
            return await GetPostFromService(serviceUrl, postContent, userName, password, _defaultTimeout);
        }

        public static async Task<HttpResponseMessage> GetPostFromService(string serviceUrl, string postContent, string userName, string password, TimeSpan timeout)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(timeout);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                var authentication = userName + ":" + password;
                var base64EncodedAuthentication = Convert.ToBase64String(Encoding.ASCII.GetBytes(authentication));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthentication);

                request.Content = new StringContent(postContent);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //Logger.LogDebug("HTTP.GPSA", "Calling POST: " + serviceUrl);

                response = await _client.SendAsync(request, cts.Token);

                //Logger.LogDebug("HTTP.GPSA", "Call returned: " + serviceUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.GPSA", "Exception message: " + ex.Message);
            }

            return response;
        }
        #endregion // Basic Auth

        #region Digest Auth
        public static async Task<HttpResponseMessage> GetPostFromServiceDigestAuth(RpcBase rpc, string path, string postContent)
        {
            return await GetPostFromServiceDigestAuth(rpc, path, postContent, _defaultTimeout);
        }

        public static async Task<HttpResponseMessage> GetPostFromServiceDigestAuth(RpcBase rpc, string path, string postContent, TimeSpan timeout)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(timeout);
                string serviceUrl = GetServiceUrl(rpc, path);

                // Send preemptively with cached auth if available, otherwise unauthenticated to get the challenge
                string? preemptiveAuth = TryBuildPreemptiveAuthHeader(HttpMethod.Post.Method, serviceUrl, rpc.UserName, rpc.Password);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
                request.Content = new StringContent(postContent);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                if (preemptiveAuth != null)
                {
                    request.Headers.TryAddWithoutValidation("Authorization", preemptiveAuth);
                }

                response = await _client.SendAsync(request, cts.Token);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Cache miss, stale nonce, or first request - do full challenge-response
                    string? authHeader = BuildDigestAuthHeader(response, HttpMethod.Post.Method, serviceUrl, rpc.UserName, rpc.Password);
                    if (authHeader != null)
                    {
                        HttpRequestMessage retryRequest = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
                        retryRequest.Content = new StringContent(postContent);
                        retryRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        retryRequest.Headers.TryAddWithoutValidation("Authorization", authHeader);
                        response = await _client.SendAsync(retryRequest, cts.Token);
                    }
                    else
                    {
                        // No WWW-Authenticate header - clear cache so the next call does a fresh challenge-response
                        _cachedRealm = null;
                        _cachedNonce = null;
                        _cachedKey = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.GPDA", "Exception message: " + ex.Message);
            }

            return response;
        }

        private static string? TryBuildPreemptiveAuthHeader(string method, string serviceUrl, string userName, string password)
        {
            string key = userName + ":" + password;
            if (_cachedRealm == null || _cachedNonce == null || _cachedKey != key)
            {
                return null;
            }

            int nc = Interlocked.Increment(ref _cachedNc);
            string ncStr = nc.ToString("x8");
            string cnonce = Guid.NewGuid().ToString("N")[..8];
            string uri = new Uri(serviceUrl).PathAndQuery;

            string credentialsHash = ComputeMD5($"{userName}:{_cachedRealm}:{password}");
            string requestHash = ComputeMD5($"{method}:{uri}");
            string digestResponse  = string.IsNullOrEmpty(_cachedQop)
                ? ComputeMD5($"{credentialsHash}:{_cachedNonce}:{requestHash}")
                : ComputeMD5($"{credentialsHash}:{_cachedNonce}:{ncStr}:{cnonce}:{_cachedQop}:{requestHash}");

            string header = $"Digest username=\"{userName}\", realm=\"{_cachedRealm}\", nonce=\"{_cachedNonce}\", uri=\"{uri}\", response=\"{digestResponse}\"";
            if (!string.IsNullOrEmpty(_cachedQop))    { header += $", qop={_cachedQop}, nc={ncStr}, cnonce=\"{cnonce}\""; }
            if (!string.IsNullOrEmpty(_cachedOpaque)) { header += $", opaque=\"{_cachedOpaque}\""; }

            return header;
        }

        private static string? BuildDigestAuthHeader(HttpResponseMessage challengeResponse, string method, string serviceUrl, string userName, string password)
        {
            try
            {
                string wwwAuth = challengeResponse.Headers.WwwAuthenticate.FirstOrDefault()?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(wwwAuth)) { return null; }

                string realm = GetDigestParam(wwwAuth, "realm");
                string nonce = GetDigestParam(wwwAuth, "nonce");
                string qop = GetDigestParam(wwwAuth, "qop");
                string opaque = GetDigestParam(wwwAuth, "opaque");
                string uri = new Uri(serviceUrl).PathAndQuery;
                string nc = "00000001";
                string cnonce = Guid.NewGuid().ToString("N")[..8];

                // RFC 2617 Digest auth: credentialsHash = MD5(user:realm:pass), requestHash = MD5(method:uri)
                string credentialsHash = ComputeMD5($"{userName}:{realm}:{password}");
                string requestHash = ComputeMD5($"{method}:{uri}");
                string digestResponse  = string.IsNullOrEmpty(qop)
                    ? ComputeMD5($"{credentialsHash}:{nonce}:{requestHash}")
                    : ComputeMD5($"{credentialsHash}:{nonce}:{nc}:{cnonce}:{qop}:{requestHash}");

                string header = $"Digest username=\"{userName}\", realm=\"{realm}\", nonce=\"{nonce}\", uri=\"{uri}\", response=\"{digestResponse}\"";
                if (!string.IsNullOrEmpty(qop))    { header += $", qop={qop}, nc={nc}, cnonce=\"{cnonce}\""; }
                if (!string.IsNullOrEmpty(opaque)) { header += $", opaque=\"{opaque}\""; }

                // Update cache so subsequent requests can authenticate preemptively
                _cachedRealm = realm;
                _cachedNonce = nonce;
                _cachedQop = qop;
                _cachedOpaque = opaque;
                _cachedNc = 1;
                _cachedKey = userName + ":" + password;

                return header;
            }
            catch (Exception ex)
            {
                Logger.LogError("HTTP.BDAH", "Exception: " + ex.Message);
                return null;
            }
        }

        private static string ComputeMD5(string input)
        {
            using MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string GetDigestParam(string header, string name)
        {
            Match match = Regex.Match(header, name + @"=""?([^"",\s]+)""?");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        #endregion // Digest Auth

        #region Helpers
        public static async Task<ServiceError> GetHttpError(string source, HttpResponseMessage httpResponse)
        {
            ServiceError httpError = new();

            try
            {
                httpError.IsError = true;
                httpError.Code = httpResponse.StatusCode.ToString();
                httpError.Message = httpResponse.ReasonPhrase;
                httpError.Content = await httpResponse.Content.ReadAsStringAsync();

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
        #endregion // Helpers
    }
}
