using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace Azure.Costs.Common
{
    public class MyHttpClient //: HttpClient
    {
        public int HttpCallCount = 0;
        protected HttpClient _httpClient;

        public MyHttpClient(string baseAddress, int timeoutSecs, string accessToken) {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
                    ,
                Timeout = TimeSpan.FromSeconds(timeoutSecs)
            };

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string? url) {
            HttpCallCount++;

            if (HttpCallCount % 10 == 0)
            {
                //Debug.WriteLine($"Http # {HttpCallCount}");
            }
            return await _httpClient.GetAsync(url);  
        }

        public async Task<HttpResponseMessage> PostAsync(string? url, string content)
        {
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            
            HttpCallCount++;

            if (HttpCallCount % 10 == 0)
            {
                //Debug.WriteLine($"Http # {HttpCallCount}");
            }
            return await _httpClient.PostAsync(url, httpContent);
        }
    }
}
